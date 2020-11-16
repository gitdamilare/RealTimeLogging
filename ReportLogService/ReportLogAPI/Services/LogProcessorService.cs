using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;
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
		public LogProcessorService(ILogger<ReportService> logger, IHostingEnvironment hostingEnvironment, IReportService reportService, IAzureBlobService azureBlobService)
		{
			_logger = logger;
			_hostingEnvironment = hostingEnvironment;
			_reportService = reportService;
			_azureBlobService = azureBlobService;
		}	
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

		public async Task<(bool IsSuccess, string Message)> LocalUploader(IFormFileCollection uploadedFiles)
		{
			var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads"); // Save to Blob
			var file = uploadedFiles[0];
			var filePath = Path.Combine(uploads, file.FileName);
			if (file.FileName.Length > 0)
			{
				try
				{
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}
					var result = await DataTransformationAsync(filePath);
					
					if (result.IsSuccess)
					{
						//await _reportService.InsertLogAsync(result.Logs);
						return (true, "Success");
					}
					return (false, result.Message);
				}
				catch (Exception ex)
				{
					return (false, $"Error Occured at LocalUploader, {ex.Message}");
					throw;
				}

			}
			return (false, "Error Occured at LocalUploader, File does not exist");
		}

		
		public async Task<(bool IsSucess, string Message)> UploadZipFile(IFormFileCollection uploadedFiles)
		{
			var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads"); // Save to Blob

			if (uploadedFiles.Count > 0 )
			{
				foreach (var file in uploadedFiles)
				{
					//var file = uploadedFiles[0];
					var filePath = Path.Combine(uploads, file.FileName);
					
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
											(bool status, AzureBlobOutputDto azureBlobOutputDto, string message) = await _azureBlobService.UploadToBlobAsync($"{Path.GetFileNameWithoutExtension(filePath)}.xml", 
																													AzureBlobInputDto.FolderPath, string.Empty, result , zipEntrystream);
											
											await Encode($"{file.Name}.xml", result);
										}
									}
								}

							}

							return (true, string.Empty);
						}
						catch (Exception ex)
						{
							return (false, $"An Error Occured during UploadZipFile, {ex.Message}");
							throw;
						}

					}
					return (false, $"An Error Occured during UploadZipFile, File Seems to be Empty ");

				}

			}

			return (false, $"An Error Occured during UploadZipFile, Uploaded File Seems to be Empty");

		}

		private async Task Encode(string path, string text)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(path))
				{
					await writer.WriteAsync(text);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"An Error Occured during Encode, {ex.Message}");			
			}
		}

	}
}
