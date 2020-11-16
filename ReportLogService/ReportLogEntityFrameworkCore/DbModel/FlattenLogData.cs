using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppFlattenLogData")]
	public class FlattenLogData
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.Now;
		public DateTime DateModified { get; set; }
		public string CreatedBy { get; set; } = "Tecan User";
		public string ModifiedBy { get; set; }
		public int MessageId { get; set; }
		public DateTime Timestamp { get; set; }
		
		public string Channel { get; set; }
		public Severity Severity { get; set; }
		
		public string Type { get; set; }
		
		public string Message { get; set; }
		
		public string ActivityId { get; set; }
		//public LogMessageData LogMessageData { get; set; }
		
		public string RootActivityId { get; set; }
		
		public string ParentActivityId { get; set; }
		
		public string ActivityName { get; set; }

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
			else if (severity.Equals("Debug"))
			{
				return Severity.Debug;
			}
			else
			{
				return Severity.Default;
			}
		}


	}



}
