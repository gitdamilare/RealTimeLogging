using Microsoft.AspNetCore.Http;
using ReportLogAPI.Interface;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ReportLogTest
{
	public class UploadTest
	{
		private readonly ILogProcessorService _logProcessorService;
		private readonly string pathZlf;
		private readonly string pathXml;
		public UploadTest(ILogProcessorService logProcessorService)
		{
			_logProcessorService = logProcessorService; 
			pathZlf = Path.Combine(@"C:\\Users\\damil\\Desktop\\Tecan\\solution\\ReportLogService\\ReportLogTest\\Logs", "LogFile 2017-05-29 09.48.24.675.zlf");
			pathXml = Path.Combine(@"C:\\Users\\damil\\Desktop\\Tecan\\solution\\ReportLogService\\ReportLogTest\\Logs", "LogFile 2017-05-29 13.02.03.863.xml");
		}


		[Fact]
		public async Task IsUploadSucessfulAsync()
		{
			var fileList = await ReturnFormFile();
			var result = await _logProcessorService.UploadZipFile(fileList);
			Assert.True(result.IsSucess,result.output?[0]);
		}

		[Fact]
		public async Task IsLogParseSucessfulAsync()
		{
			var result = await _logProcessorService.DataTransformationAsync(pathXml);
			Assert.True((result.IsSuccess && result.Logs.Count > 0), result.Message);
		}

		private async Task<IFormFileCollection> ReturnFormFile()
		{
			
			var fileList = new FormFileCollection();
			using (var stream = File.OpenRead(pathZlf))
			{
				using (var ms = new MemoryStream())
				{
					await stream.CopyToAsync(ms);
					var file = new FormFile(ms, 0, stream.Length, null, "LogFile 2017-05-29 09.48.24.675.zlf")
					{
						Headers = new HeaderDictionary(),
						ContentType = "application/octet-stream"
					};

					if(file != null)
				{
					//await file.CopyToAsync(fileStream);
					fileList.Add(file);
				}
				}
				
				
			}
			return fileList;
		}
	}
}
