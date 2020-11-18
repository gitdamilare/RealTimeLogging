using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ReportLogAPI.HubServices;
using ReportLogAPI.Interface;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace ReportLogAPI.Extentions
{
	[Obsolete("No Longer Supported")]
	public class ReportDatabaseSubscription : IDatabaseSubscription
	{
		public ILogger<ReportDatabaseSubscription> _logger { get; set; }
		private bool disposedValue = false;
		private IHubContext<ReportLogHub> _reportLogHub;
		///private readonly IReportService _reportService;

		private SqlTableDependency<LogProcessData> _tableDependency;

		public ReportDatabaseSubscription(ILogger<ReportDatabaseSubscription> logger, 
										  IHubContext<ReportLogHub> reportLogHub
										  )
		{
			_logger = logger;
			_reportLogHub = reportLogHub;
			//_reportService = reportService;IReportService reportService
		}

		public void Configure(string connectionString)
		{
			_tableDependency = new SqlTableDependency<LogProcessData>(connectionString);
			_tableDependency.OnChanged += Changed;
			_tableDependency.OnError += TableDependency_OnError;
			_tableDependency.Start();
		}

		private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
		{
			_logger.LogError("SqlTableDependency error:" + e.Error.Message);
		}

		private void Changed(object sender, RecordChangedEventArgs<LogProcessData> e)
		{
			if (e.ChangeType == ChangeType.Insert)
			{
				UpdateReportLog();
			}
		}

		public async void UpdateReportLog()
		{
			try
			{
				//var (IsSucess, outputDto, Message) = await _reportService.GetReportCount();
				//if (IsSucess)
				await _reportLogHub.Clients.All.SendAsync("RecordUpdate", "Make Update");
				//await _reportLogHub.Clients.All.SendAsync("RecordUpdate", "No Record to Update");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error Occured at UpdateReportLog,{ex.Message}");
				throw new HubException(ex.Message);
			}
		}

		#region IDisposable

		~ReportDatabaseSubscription()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_tableDependency.Stop();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
