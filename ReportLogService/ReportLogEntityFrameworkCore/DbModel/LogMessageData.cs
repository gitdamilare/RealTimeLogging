using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppLogMessageData")]
	public class LogMessageData
	{
		public int MainMessageId { get; set; }
		public int MessageId { get; set; }
		public int LogId { get; set; }
		public string Message { get; set; }
		public MainLogData MainLogData { get; set; }
		
	}
}
