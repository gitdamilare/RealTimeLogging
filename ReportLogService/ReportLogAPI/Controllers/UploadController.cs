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
	}
}
