using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;
using ReportLogAPI.Repos;
using ReportLogEntityFrameworkCore.AppContext;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Services
{
	public class ReportService : IReportService
	{
		private readonly ILogger<ReportService> _logger;
		private readonly IGenericRepository<MainLogData> _mainloggenericRepository;
		private readonly IGenericRepository<LogActivityData> _logactivitygenericRepository;
		private readonly IGenericRepository<FlattenLogData> _flattenlogenericRepository;
		private readonly IMapper _mapper;
		private ReportLogDbContext _reportLogDbContext;

		public ReportService(ILogger<ReportService> logger, 
							IGenericRepository<MainLogData> mainloggenericRepository,
							IGenericRepository<LogActivityData> logactivitygenericRepository,
							IGenericRepository<FlattenLogData> flattenlogenericRepository,
							IMapper mapper,
							ReportLogDbContext reportLogDbContext)
		{
			_logger = logger;
			_mainloggenericRepository = mainloggenericRepository;
			_logactivitygenericRepository = logactivitygenericRepository;
			_mapper = mapper;
			_flattenlogenericRepository = flattenlogenericRepository;
			_reportLogDbContext = reportLogDbContext;
		}
		public async Task<(bool IsSucess, string Message)> InsertLogAsync(Log inputDto)
		{
			try
			{
				if(inputDto != null)
				{
					var mainLogData = _mapper.Map<MainLogData>(inputDto);
					await _mainloggenericRepository.InsertAsync(mainLogData);
					var logActivityData = _mapper.Map<LogActivityData>(inputDto);
					logActivityData.LogId = mainLogData.Id;
					await _logactivitygenericRepository.InsertAsync(logActivityData);
					return (true, "Success");
				}
				return (false, "inputDto cannot be null");
			}
			catch (Exception ex)
			{
				string err = "Error Occured at InsertLogAsync " + ex.Message;
				_logger.LogError(err);
				return (false, err);
			}
		}


		public async Task<(bool IsSucess, string Message)> InsertAllLogFlattenAsync(List<Log> inputDto)
		{
			try
			{
				if (inputDto != null)
				{
					var mainLogList = _mapper.Map<List<FlattenLogData>>(inputDto);
					//foreach (var row in mainLogList)
					_flattenlogenericRepository.BulkInsert(mainLogList);
					//await _flattenlogenericRepository.SaveAsync();
					await Task.Delay(1);
					return (true, "Success");
					
				}
				return (false, "inputDto cannot be null");
			}
			catch (Exception ex)
			{
				string err = "Error Occured at InsertLogAsync " + ex.Message;
				_logger.LogError(err);
				return (false, err);
			}
		}


		public async Task<(bool IsSucess, string Message)> InsertAllLogAsync(List<Log> inputDto)
		{
			try
			{
				if (inputDto != null)
				{
					var mainLogList = _mapper.Map<List<MainLogData>>(inputDto);
					var bulkInsert = mainLogList.Where(xx => string.IsNullOrEmpty(xx.ActivityId));
					var saveInsert = inputDto.Where(xx => !string.IsNullOrEmpty(xx.ActivityId));
					_mainloggenericRepository.BulkInsert(bulkInsert);

					foreach (var row in saveInsert)
					{
							var mainLogData = mainLogList.FirstOrDefault(xx => xx.MessageId.ToString().Equals(row.MessageId));
							await _mainloggenericRepository.InsertAsync(mainLogData);
							var logActivityData = _mapper.Map<LogActivityData>(row);
							logActivityData.LogId = mainLogData.Id;
							await _logactivitygenericRepository.InsertAsync(logActivityData, false);
												
					}
					//await _mainloggenericRepository.SaveAsync(); //I only make a trip to the Db When Needed
					await _logactivitygenericRepository.SaveAsync();//
					return (true, "Success");
				}
				return (false, "inputDto cannot be null");
			}
			catch (Exception ex)
			{
				string err = "Error Occured at InsertLogAsync " + ex.Message;
				_logger.LogError(err);
				return (false, err);
			}
		}

		public async Task<(bool IsSucess, ReportLogOutputDto outputDto, string Message)> GetReportCount()
		{
			try
			{
				var messageCount = _reportLogDbContext.FlattenLogDatas.AsNoTracking().Count();
				var totalWarning = _reportLogDbContext.FlattenLogDatas.AsNoTracking().Where(xx => xx.Severity == Severity.Warning).Count();
				var totalInfo = _reportLogDbContext.FlattenLogDatas.AsNoTracking().Where(xx => xx.Severity == Severity.Info).Count();
				var totalError = _reportLogDbContext.FlattenLogDatas.AsNoTracking().Where(xx => xx.Severity == Severity.Error).Count();
				
				var logProcessData = _reportLogDbContext.LogProcessDatas.AsNoTracking().ToList();
				var totalLogFile = logProcessData.Count();				
				var logProcess = _mapper.Map<List<LogProcessOutputDto>>(logProcessData);

				var result = new ReportLogOutputDto(messageCount,totalWarning, totalInfo, totalError, totalLogFile, logProcess);

				return (true, result, string.Empty);

			}
			catch (Exception ex)
			{
				string err = $"An Error Occured during GetLogReport, {ex.Message}";
				_logger.LogError(err);
				return (false, null, err);
			}
		}


		public async Task<(bool IsSucess, PagedResult<FlattenLogOutput> outputDto, string Message)> GetReportLog(int page = 1, int take = 10)
		{
			try
			{
				var result = _reportLogDbContext.FlattenLogDatas
												.AsNoTracking()
												.Select(xx => new FlattenLogOutput() 
												{ 
													Id = xx.Id,
													MessageId = xx.MessageId,
													Message = xx.Message,
													DateCreated = xx.DateCreated,
													Channel = xx.Channel,
													ActivityName = xx.ActivityName,
													Severity = xx.Severity.ToString(),
													Timestamp = xx.Timestamp,
													Type =xx.Type														
												}).GetPaged(page, take);

				return (true, result, string.Empty);

			}
			catch (Exception ex)
			{
				string err = $"An Error Occured during GetLogReport, {ex.Message}";
				_logger.LogError(err);
				return (false, null, err);
			}
		}

	}
}
