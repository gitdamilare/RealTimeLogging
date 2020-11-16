using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
	public class AzureBlobInputDto
	{
		public static string FolderPath { get; set; } = "report-logs";
		public IFormFileCollection Files { get; set; }
		//public string Status { get; set; }
	}

	public class AzureBlobOutputDto
	{
		public string BaseUri { get; set; }
		public string SasToken { get; set; }
		public string Status { get; set; }
	}
}
