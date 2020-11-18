using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.ModelDto;
using ReportLogAPI.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Interface
{
	public interface IReportService
	{
		Task<(bool IsSucess, string Message)> InsertLogAsync(Log inputDto);
		Task<(bool IsSucess, string Message)> InsertAllLogAsync(List<Log> logs);
		Task<(bool IsSucess, string Message)> InsertAllLogFlattenAsync(List<Log> inputDto);
		Task<(bool IsSucess, ReportLogOutputDto outputDto, string Message)> GetReportCount();
		Task<(bool IsSucess, PagedResult<FlattenLogOutput> outputDto, string Message)> GetReportLog(int page = 1, int take = 10);

	}
}
