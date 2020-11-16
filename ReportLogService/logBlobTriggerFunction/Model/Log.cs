using System;
using System.Collections.Generic;
using System.Text;

namespace logBlobTriggerFunction
{
	public class Log
	{
		public string MessageId { get; set; }
		public string TimeStamp { get; set; }
		public string Channel { get; set; }
		public string Type { get; set; }
		public string Severity { get; set; }
		public string RootActivityId { get; set; }
		public string ParentActivityId { get; set; }
		public string ActivityId { get; set; }
		public string ActivityName { get; set; }
		public string Message { get; set; }
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
