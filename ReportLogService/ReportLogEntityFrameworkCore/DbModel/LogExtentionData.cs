using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppLogExtentionData")]
	public class LogExtentionData : BaseEntity
	{
		public int FunctionId { get; set; }
		public int ModuleId { get; set; }
		public int MessageType { get; set; }
		public int ErrorCode { get; set; }
		public int EventId { get; set; }
		public string Message { get; set; }
	}
}
