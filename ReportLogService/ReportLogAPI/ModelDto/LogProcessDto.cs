using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
	public class LogProcessDto
	{
	}

	public class LogProcessOutputDto
	{
		public int Id { get; set; }
		public DateTime DateCreated { get; set; }
		public string CreatedBy { get; set; }
		public string FileName { get; set; }
		public string ExecutionTime { get; set; }
	}
}
