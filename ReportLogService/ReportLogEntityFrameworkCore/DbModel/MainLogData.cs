using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppMainLogData")]
	public class MainLogData : BaseEntity
	{
		public int MessageId { get; set; }
		public DateTime Timestamp { get; set; }
		public string Channel { get; set; }
		public Severity Severity { get; set; }
		public string Type { get; set; }
		public string Message { get; set; }
		[NotMapped]
		public string ActivityId { get; set; }
		//public LogMessageData LogMessageData { get; set; }
		public LogActivityData LogActivityData { get; set; }

		public static Severity SeverityCreator(string severity)
		{
			if (severity.Equals("Error"))
			{
				return Severity.Error;
			}
			else if (severity.Equals("Warning"))
			{
				return Severity.Warning;
			}
			else if (severity.Equals("Info"))
			{
				return Severity.Info;
			}
			else
			{
				return Severity.Default;
			}
		}
	}

	public enum Severity
	{
		Error,
		Warning,
		Info,
		Debug,
		Default
	}


}
