using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;
using ReportLogAPI.Repos;



namespace ReportLogAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReportController : ControllerBase
	{
		private readonly ILogProcessorService _logSerializer;
		private readonly IHostingEnvironment _hostingEnvironment;
		private readonly IReportService _reportService;
		private readonly ILogger<ReportController> _logger;
		public ReportController(ILogProcessorService logSerializer, IHostingEnvironment hostingEnvironment,
							    IReportService reportService, ILogger<ReportController> logger)
		{
			_logSerializer = logSerializer;
			_hostingEnvironment = hostingEnvironment;
			_reportService = reportService;
			_logger = logger;
		}

		// GET: api/<ReportController>
		[HttpGet]
		public async Task<ActionResult> GetMiniReport()
		{
			try
			{
				var (IsSucess, outputDto, Message) = await _reportService.GetReportCount();
				if (IsSucess)
				{
					return Ok(new ReturnModelObject<ReportLogOutputDto> { Data = outputDto, Error = false, Message = String.Empty });
				}
				return Ok(new ReturnModelObject<ReportLogOutputDto> { Data = null, Error = true, Message = Message });
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error Occured at GetMiniReport, {ex.Message}");
				return BadRequest(ex.Message);
			}
		}

		// GET: api/<ReportController>/1/10
		[HttpGet("{page}/{pageSize}")]
		public async Task<ActionResult> GetLogData(int page, int pageSize)
		{
			try
			{
				var (IsSucess, outputDto, Message) = await _reportService.GetReportLog(page: page, take : pageSize);
				if (IsSucess)
				{
					return Ok(new ReturnModelObject<PagedResult<FlattenLogOutput>> { Data = outputDto, Error = false, Message = String.Empty });
				}
				return Ok(new ReturnModelObject<PagedResult<FlattenLogOutput>> { Data = null, Error = true, Message = Message });
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error Occured at GetLogData, {ex.Message}");
				return BadRequest(ex.Message);
			}
		}
	}
}
