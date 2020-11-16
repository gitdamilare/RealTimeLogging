using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
	public class ReportLogDto
	{

	}

	public class ReportLogOutputDto
	{
		public int TotalMessage { get; set; }
		public int TotalWarning { get; set; }
		public int TotalInfo { get; set; }
		public int TotalError { get; set; }
		public int TotalLogFile { get; set; }
		public List<LogProcessOutputDto> LogProcessOutputDto { get; set; }


		public ReportLogOutputDto(int totalMessage, int totalWarning, int totalInfo, int totalError, int totalLogFile, List<LogProcessOutputDto> logProcessOutputDto)
		{
			TotalMessage = totalMessage;
			TotalWarning = totalWarning;
			TotalInfo = totalInfo;
			TotalError = totalError;
			TotalLogFile = totalLogFile;
			LogProcessOutputDto = logProcessOutputDto;
		}
	}
}
