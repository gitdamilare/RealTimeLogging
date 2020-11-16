using System;
using System.Collections.Generic;
using System.Text;

namespace logBlobTriggerFunction.Model
{
	public class LogProcessData
	{
		//public int Id { get; set; } 
		public string DateCreated { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		public string DateModified { get; set; } = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff");
		public string CreatedBy { get; set; } = "Tecan User";
		public string ModifiedBy { get; set; }
		public string FileName { get; set; }
		public string ExecutionTime { get; set; }
	}
}
