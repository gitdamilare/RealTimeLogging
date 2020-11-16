using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Dtos.InputDto
{
	public class MainLogDataInputDto
	{
		public string MessageId { get; set; }
		public string Timestamp { get; set; }
		public string Channel { get; set; }
		public string Type { get; set; }
		public string Severity { get; set; }	
		public string RootActivityId { get; set; }
		public string ParentActivityId { get; set; }
		public string ActivityId { get; set; }
		public string ActivityName { get; set; }
		public string Message { get; set; }
	}


	public class MainLogDataOutputDto
	{
		public string MessageId { get; set; }
		public string Timestamp { get; set; }
		public string Channel { get; set; }
		public string Type { get; set; }
		public string Severity { get; set; }
		public string Message { get; set; }
		public string RootActivityId { get; set; }
		public string ParentActivityId { get; set; }
		public string ActivityId { get; set; }
		public string ActivityName { get; set; }
		
	}
}
