using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReportLogAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UploadController : ControllerBase
	{
		private readonly ILogProcessorService _logSerializer;
		private readonly IHostingEnvironment _hostingEnvironment;
		private readonly IReportService _reportService;
		public UploadController(ILogProcessorService logSerializer, IHostingEnvironment hostingEnvironment, IReportService reportService)
		{
			_logSerializer = logSerializer;
			_hostingEnvironment = hostingEnvironment;
			_reportService = reportService;
		}

		// POST api/<UploadController>
		[HttpPost]
		private async Task<ActionResult> UploadXmlFile([FromForm] IFormFileCollection uploadedFiles)
		{
			var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads"); // Save to Blob
			var file = uploadedFiles[0];
			var filePath = Path.Combine(uploads, file.FileName);
			if(file.FileName.Length > 0)
			{
				try
				{
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}
					var result = await _logSerializer.DataTransformationAsync(filePath);
					
					if (result.IsSuccess)
					{
						await _reportService.InsertAllLogAsync(result.Logs);
						return Ok(new ReturnModel<Log>{Data = result.Logs, Error = false, Message = String.Empty });
					}
					return BadRequest(result.Message);
				}
				catch (Exception ex)
				{
					return BadRequest(ex.Message);
					throw;
				}
				
			}
			return BadRequest("Error , File is Empty");
		}
	
		[HttpPost]
		public async Task<ActionResult> UploadZipFiles([FromForm] IFormFileCollection uploadedFiles)
		{
			try
			{
				var result = await _logSerializer.UploadZipFile(uploadedFiles);
				if(result.IsSucess)
					return Ok(new ReturnModel<string> { Data = result.output , Error = false, Message = String.Empty });
				return BadRequest(new ReturnModel<string>{ Data = result.output,  Error = true, Message = result.Message});
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
				throw;
			}
		}

		private static void Encode(string path, string text)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(path))
				{
					writer.Write(text);
				}
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
