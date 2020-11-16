using Dapper;
using logBlobTriggerFunction.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

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
	}
}
