using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Interface;

namespace ReportLogAPI.HubServices
{
	public class ReportLogHub : Hub
	{
		private readonly ILogger<ReportLogHub> _logger;
		private readonly IReportService _reportService;

		public ReportLogHub(ILogger<ReportLogHub> logger, IReportService reportService)
		{
			_logger = logger;
			_reportService = reportService;
		}

		public async Task UpdateReportLog()
		{
			try
			{
				var (IsSucess, outputDto, Message) = await _reportService.GetReportCount();
				if (IsSucess)
					await Clients.All.SendAsync("RecordUpdate", outputDto);
				await Clients.All.SendAsync("RecordUpdate", "No Record to Update");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error Occured at UpdateReportLog,{ex.Message}");
				throw new HubException(ex.Message);
			}
		}
	}
}
