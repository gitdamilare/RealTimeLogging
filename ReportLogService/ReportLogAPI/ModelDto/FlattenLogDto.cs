using ReportLogAPI.Repos;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.ModelDto
{
	public class FlattenLogDto
	{
	}

	public class FlattenLogOutput
	{
		public int Id { get; set; }
		public DateTime DateCreated { get; set; }
		public int MessageId { get; set; }
		public DateTime Timestamp { get; set; }
		public string Channel { get; set; }
		public string Severity { get; set; }
		public string Type { get; set; }
		public string Message { get; set; }
		public string ActivityName { get; set; }
		
	}

	public class LogDataOutPutDto
	{
		public PagedResult<FlattenLogOutput> Data { get; set; }
		public int TotalCount { get; set; }

		public LogDataOutPutDto(PagedResult<FlattenLogOutput> data, int totalCount)
		{
			Data = data;
			TotalCount = totalCount;
		}
	}
}
