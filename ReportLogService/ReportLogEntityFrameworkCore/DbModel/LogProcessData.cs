using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ReportLogEntityFrameworkCore.DbModel
{
	[Table("AppLogProcessData")]
	public class LogProcessData
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.Now;
		public DateTime DateModified { get; set; }
		public string CreatedBy { get; set; } = "Tecan User";
		public string ModifiedBy { get; set; }
		[MaxLength(70)]
		public string FileName { get; set; }
		[MaxLength(10)]
		public string ExecutionTime { get; set; }
	}
}
