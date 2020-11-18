using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;
using ReportLogAPI.Repos;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ReportLogAPI.Services
{
	public class LogProcessorService : ILogProcessorService
	{
		private readonly ILogger<ReportService> _logger;
		private readonly IHostingEnvironment _hostingEnvironment;
		private readonly IReportService _reportService;
		private readonly IAzureBlobService _azureBlobService;
		private readonly IGenericRepository<LogProcessData> _logProcessData;
		public LogProcessorService(ILogger<ReportService> logger, IHostingEnvironment hostingEnvironment, 
								   IReportService reportService, IAzureBlobService azureBlobService,
								   IGenericRepository<LogProcessData> logProcessData)
		{
			_logger = logger;
			_hostingEnvironment = hostingEnvironment;
			_reportService = reportService;
			_azureBlobService = azureBlobService;
			_logProcessData = logProcessData;
		}

		[Obsolete("AzureFunction, LogTransformationAsync(..) now handles this Operation")]
		public async Task<(bool IsSuccess, List<Log> Logs, string Message)> DataTransformationAsync(string filePath)
		{
			List<Log> result = new List<Log>();
			try
			{
				using (var stream = new StreamReader(filePath))
				{
					//XmlSerializer reader = new XmlSerializer(typeof(XmlDataLog));
					XmlTextReader xmlTextReader = new XmlTextReader(stream);
					while (xmlTextReader.Read())
					{
						if((xmlTextReader.NodeType == XmlNodeType.Element) && (xmlTextReader.Name.Equals("Log")))
						{
							if (xmlTextReader.HasAttributes)
							{
								result.Add(new Log { 
									MessageId = xmlTextReader.GetAttribute("MsgID"),
									TimeStamp = xmlTextReader.GetAttribute("TimeStamp"),
									Channel = xmlTextReader.GetAttribute("Channel"),
									Type = xmlTextReader.GetAttribute("Type"),
									Severity = xmlTextReader.GetAttribute("Severity"),								
									RootActivityId = xmlTextReader.GetAttribute("RootActivityId"),
									ParentActivityId = xmlTextReader.GetAttribute("ParentActivityId"),
									ActivityId = xmlTextReader.GetAttribute("ActivityId"),
									ActivityName = xmlTextReader.GetAttribute("ActivityName"),
									Message = xmlTextReader.GetAttribute("Message"),
								});
							}
						}
					}
					//XmlDataLog xmlDataLog = (XmlDataLog)reader.Deserialize(stream);
					//bool isNotNull = xmlDataLog.GetType()
					//							.GetProperties().All(p => p.GetValue(xmlDataLog) != null);


					await Task.Delay(1);
					if (result.Count > 0)
					{
						return  (true, result, string.Empty);
					}
					_logger.LogError("Error Occured at DataTransformationAsync, while trying to read LogFile. Logfile is empty");
					return (false, null, "An Error Occured");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error Occured at DataTransformationAsync, " + ex.Message);
				return (false, null, ex.Message);
			}

		}

		//This Function takes the incoming zipped files
		//Extract the file and saves to uploads to Azure blob container
		public async Task<(bool IsSucess, List<string> output,  string Message)> UploadZipFile(IFormFileCollection uploadedFiles)
		{
			var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads"); // Save to Blob
			//var uploads = Path.Combine(@"C:\\Users\\damil\\Desktop\\Tecan\\solution\\ReportLogService\\ReportLogAPI", "uploads");
			List<string> outputDto = new List<string>();
			if (uploadedFiles.Count > 0 )
			{		
				foreach (var file in uploadedFiles)
				{
					if (file.FileName.Contains(".zlf"))
					{
						var filePath = Path.Combine(uploads, file.FileName);
						if (await CheckIfFileExist(file.FileName))
						{
							if (file.FileName.Length > 0)
							{
								try
								{
									using (var fileStream = new FileStream(filePath, FileMode.Create))
									{
										await file.CopyToAsync(fileStream);
									}

									using (ZipArchive archive = ZipFile.OpenRead(filePath))
									{
									
										var sample = archive.Entries[0];
										if (sample != null)
										{
											using (var zipEntrystream = sample.Open())
											{
												using (StreamReader reader = new StreamReader(zipEntrystream))
												{
													var result = await reader.ReadToEndAsync();
													(bool success, AzureBlobOutputDto azureBlobOutputDto, string message) = await _azureBlobService.UploadToBlobAsync($"{Path.GetFileNameWithoutExtension(filePath.Trim())}.xml",
																															AzureBlobInputDto.FolderPath, string.Empty, result, zipEntrystream);
													if (!success)
													{
														outputDto.Add($"An Error Occured during UploadZipFile, FileName: {file.FileName}, Error: {message}, status: {azureBlobOutputDto.Status}");
														continue;
													}
													else { continue; }
													//await Encode($"{file.Name}.xml", result);
												}
											}
										}

									}

									return (true, outputDto, string.Empty);
								}
								catch (Exception ex)
								{
									outputDto.Add($"An Error Occured during UploadZipFile, FileName: {file.FileName}, Error: {ex.Message}");
									continue;
								}

							}
							outputDto.Add($"File Seems to be Empty, FileName: {file.FileName}");
							continue;
						}
						 outputDto.Add($"Dulipcate File, File Already Exist, FileName: {file.FileName}");
						 continue;
					}					 
					outputDto.Add($"An Error Occured during UploadZipFile, File Type not Supported, FileName: {file.FileName}");
					continue;
				}
				if(outputDto.Count <= 0)
					return (true, outputDto, string.Empty);
			}

			return (false, outputDto, string.Empty);

		}

		//Checks if the FileName already exist 
		private async Task<bool> CheckIfFileExist(string fileName)
		{
			var logProcessData = await _logProcessData.GetAllAsync();
			var simpleName = GetFileName(fileName).ToLower();
			var getExisitFile = logProcessData.Where(xx => GetFileName(xx.FileName).ToLower() == simpleName).FirstOrDefault();
			if (getExisitFile != null)
				return false;
			return true;

		}

		//Strips the extention from a file
		private string GetFileName(string fileName)
		{
			int fileExtPos = fileName.LastIndexOf(".");
			if (fileExtPos >= 0)
				fileName = fileName.Substring(0, fileExtPos);
			return fileName;
		}

	}
}
