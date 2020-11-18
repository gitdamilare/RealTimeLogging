using Dapper;
using logBlobTriggerFunction.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace logBlobTriggerFunction.SQLHelper
{
	public static class SqlHelper
	{

		public static async Task<(bool IsSuccess, string Message)> InsertLogData(DataTable logData, ILogger log, string connectionString)
		{
			

			try
			{
				SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString);
				bulkCopy.DestinationTableName = "AppFlattenLogData";
				await bulkCopy.WriteToServerAsync(logData);
				return (true, "Success");
			}
			catch (Exception ex)
			{
				log.LogError($"An Error Occured in InsertLogData Function : {ex.Message}");
				return (false, "Success");
			}
		}

		public static async Task<(bool IsSuccess, string Message)> InsertLogProcessData(LogProcessData logData, ILogger log, string connectionString)
		{
			
			string sql = @"INSERT INTO AppLogProcessData (DateCreated, DateModified, CreatedBy, ModifiedBy , FileName ,ExecutionTime) 
									VALUES (@DateCreated, @DateModified, @CreatedBy, @ModifiedBy , @FileName , @ExecutionTime)";
			try
			{
				using (var conn = new SqlConnection(connectionString))
				{
					using (SqlCommand cmd = conn.CreateCommand())
					{
						conn.Open();

						//var param = new DynamicParameters();

						var affectedRows = await conn.ExecuteAsync(sql, logData);
						if (affectedRows <= 0)
						{
							log.LogError("Inserting into LogProcessData Failed");
							return (false, "Inserting into LogProcessData Failed");
						}
						return (true, string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				log.LogError($"An Error Occured in InsertLogData Function : {ex.Message}");
				return (false, ex.Message);
			}
		}


		public static async Task<(bool IsSuccess, ReportLogOutputDto outputDto, string Message)> SelectReportLogCount(ILogger log, string connectionString)
		{

			string messageCountSql = SqlCounterStatmentGenerator(isBase: true);
			string totalWarningSql = SqlCounterStatmentGenerator(condition: (int)Severity.Warning, isBase: false);
			string totalInfoSql = SqlCounterStatmentGenerator(condition: (int)Severity.Info, isBase: false);
			string totalErrorSql = SqlCounterStatmentGenerator(condition: (int)Severity.Error, isBase: false);
			string processLogSql = "SELECT Id, DateCreated, CreatedBy, FileName, ExecutionTime From AppLogProcessData";


			try
			{
				using (var conn = new SqlConnection(connectionString))
				{
					using (SqlCommand cmd = conn.CreateCommand())
					{
						conn.Open();
						var messageCount = await conn.ExecuteScalarAsync<int>(messageCountSql);
						var totalWarningCount = await conn.ExecuteScalarAsync<int>(totalWarningSql);
						var totalInfoCount = await conn.ExecuteScalarAsync<int>(totalInfoSql);
						var totalErrorCount = await conn.ExecuteScalarAsync<int>(totalErrorSql);
						var processLogData = conn.QueryAsync<LogProcessOutputDto>(processLogSql).Result.ToList();
						var result = new ReportLogOutputDto(messageCount, totalWarningCount, totalInfoCount, totalErrorCount, processLogData.Count, processLogData);
						return (true, result, string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				log.LogError($"An Error Occured in InsertLogData Function : {ex.Message}");
				return (false, null, ex.Message);
			}
		}

		public static async Task<(bool IsSuccess, IList<FlattenLogOutputDto> outputDto, string Message)> SelectReportLog(ILogger log = null, string connectionString = "")
		{

			connectionString = "Server=tcp:tecansolution.database.windows.net,1433;Initial Catalog=ReportLogDb;Persist Security Info=False;User ID=damilare;Password=Oyebanjidami95@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
			string queryLogSql = "SELECT Id, DateCreated, MessageId, Timestamp, Channel, Severity, Type, Message, ActivityName  From AppFlattenLogData";


			try
			{
				using (var conn = new SqlConnection(connectionString))
				{
					using (SqlCommand cmd = conn.CreateCommand())
					{
						conn.Open();
						var result =  conn.QueryAsync<FlattenLogOutputDto>(queryLogSql).Result.ToList();
						return (true, result, string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				if(log != null)
					log.LogError($"An Error Occured in InsertLogData Function : {ex.Message}");
				return (false, null, ex.Message);
			}
		}

		public static string SqlCounterStatmentGenerator(int condition = 0, bool isBase = false)
		{
			if (isBase)
				return "SELECT COUNT(*) FROM AppFlattenLogData";
			return $"SELECT COUNT(*) FROM AppFlattenLogData WHERE Severity = {condition}";
			
		}
		public static DataTable DataTableCreator()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("Id", typeof(int));
			dt.Columns.Add("DateCreated", typeof(DateTime));
			dt.Columns.Add("DateModified", typeof(DateTime));
			dt.Columns.Add("CreatedBy", typeof(string));
			dt.Columns.Add("ModifiedBy", typeof(string));
			dt.Columns.Add("MessageId", typeof(int));
			dt.Columns.Add("Timestamp", typeof(DateTime));
			dt.Columns.Add("Channel", typeof(string));
			dt.Columns.Add("Severity", typeof(int));
			dt.Columns.Add("Type", typeof(string));
			dt.Columns.Add("Message", typeof(string));
			dt.Columns.Add("ActivityId", typeof(string));
			dt.Columns.Add("RootActivityId", typeof(string));
			dt.Columns.Add("ParentActivityId", typeof(string));
			dt.Columns.Add("ActivityName", typeof(string));
			return dt;
		}

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

		public static async Task<(bool IsSuccess, List<Log> Logs, string Message)> DataTransformationAsync(Stream inputData, ILogger log)
		{
			List<Log> result = new List<Log>();
			try
			{
				using (var stream = new StreamReader(inputData))
				{
					XmlTextReader xmlTextReader = new XmlTextReader(stream);
					while (xmlTextReader.Read())
					{
						if ((xmlTextReader.NodeType == XmlNodeType.Element) && (xmlTextReader.Name.Equals("Log")))
						{
							if (xmlTextReader.HasAttributes)
							{
								result.Add(new Log
								{
									MessageId = xmlTextReader.GetAttribute("MsgID"),
									TimeStamp = xmlTextReader.GetAttribute("TimeStamp"),
									Channel = xmlTextReader.GetAttribute("Channel"),
									Type = xmlTextReader.GetAttribute("Type"),
									Severity = xmlTextReader.GetAttribute("Severity"),
									RootActivityId = xmlTextReader.GetAttribute("RootActivityId"),
									ParentActivityId = xmlTextReader.GetAttribute("ParentActivityId"),
									ActivityId = xmlTextReader.GetAttribute("ActivityId"),
									ActivityName = xmlTextReader.GetAttribute("ActivityName"),
									Message = xmlTextReader.GetAttribute("Message"),
								});
							}
						}
					}
					//XmlDataLog xmlDataLog = (XmlDataLog)reader.Deserialize(stream);
					//bool isNotNull = xmlDataLog.GetType()
					//							.GetProperties().All(p => p.GetValue(xmlDataLog) != null);


					await Task.Delay(1);
					if (result.Count > 0)
					{
						return (true, result, string.Empty);
					}
					log.LogError("Error Occured at DataTransformationAsync, while trying to read LogFile. Logfile is empty");
					return (false, null, "An Error Occured");
				}
			}
			catch (Exception ex)
			{
				log.LogError("Error Occured at DataTransformationAsync, " + ex.Message);
				return (false, null, ex.Message);
			}

		}
	}
}
