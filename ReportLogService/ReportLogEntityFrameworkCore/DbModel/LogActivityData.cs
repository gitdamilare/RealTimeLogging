using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppLogActivityData")]
	public class LogActivityData 
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public Guid LogId { get; set; }		
		public string ActivityId { get; set; }
		public string RootActivityId { get; set; }
		public string ParentActivityId { get; set; }
		public string ActivityName { get; set; }
		public MainLogData MainLogData { get; set; }
	}
}
