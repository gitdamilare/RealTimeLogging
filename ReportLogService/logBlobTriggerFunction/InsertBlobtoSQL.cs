using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using logBlobTriggerFunction.Model;
using logBlobTriggerFunction.SQLHelper;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace logBlobTriggerFunction
{
    public static class InsertBlobtoSQL
    {
        [FunctionName("insertBlobtoSQL")]
        public static async Task Run([BlobTrigger("report-logs/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log, ExecutionContext context)
        {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var config = new ConfigurationBuilder()
							 .SetBasePath(context.FunctionAppDirectory)
							 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
							 .AddEnvironmentVariables()
							 .Build();
			var connectionString = config.GetConnectionString("Azuresource");

			var dataTransformation = await LogTransformationAsync(myBlob, log);
			if (dataTransformation.IsSuccess)
			{
				watch.Start();
				var dataInsertion = await SqlHelper.InsertLogData(dataTransformation.Logs, log, connectionString);
				watch.Stop();
				var executionTime = $"{watch.ElapsedMilliseconds}";
				if (dataInsertion.IsSuccess)
				{

					var processLogInsertion = await SqlHelper.InsertLogProcessData(new LogProcessData { FileName = name, ExecutionTime = executionTime }, log, connectionString);
					var (IsSuccess, outputDto, _) = await SqlHelper.SelectReportLogCount(log, connectionString);

					if (IsSuccess)
						await NotifySignalrAsync(outputDto);

					if (processLogInsertion.IsSuccess)
						log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
				}
			}

		}


		#region LogTransformation
		public static async Task<(bool IsSuccess, DataTable Logs, string Message)> LogTransformationAsync(Stream inputData, ILogger log)
		{
			//List<Log> result = new List<Log>();
			DataTable dt = SqlHelper.DataTableCreator();
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
								var dtRows = dt.NewRow();
								dtRows["DateCreated"] = DateTime.Now;
								dtRows["DateModified"] = DateTime.Now;
								dtRows["CreatedBy"] = "Tecan User";
								dtRows["ModifiedBy"]  =  "";
								dtRows["MessageId"]  =  xmlTextReader.GetAttribute("MsgID");
								dtRows["Timestamp"]  =  DateTime.Parse(xmlTextReader.GetAttribute("TimeStamp"));
								dtRows["Channel"]  =  xmlTextReader.GetAttribute("Channel");
								dtRows["Severity"]  = SqlHelper.SeverityCreator(xmlTextReader.GetAttribute("Severity"));
								dtRows["Type"]  =  xmlTextReader.GetAttribute("Type");
								dtRows["Message"]  =  xmlTextReader.GetAttribute("Message");
								dtRows["ActivityId"]  =  xmlTextReader.GetAttribute("ActivityId");
								dtRows["RootActivityId"]  =  xmlTextReader.GetAttribute("RootActivityId");
								dtRows["ParentActivityId"]  =  xmlTextReader.GetAttribute("ParentActivityId");
								dtRows["ActivityName"]  =  xmlTextReader.GetAttribute("ActivityName");
								dt.Rows.Add(dtRows);
							}
						}
					}

					await Task.Delay(1);
					if (dt != null)
					{
						return (true, dt, string.Empty);
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
		#endregion

		#region Notification SignalR
		//Make a Call to SingalR
		public static async Task NotifySignalrAsync(ReportLogOutputDto inputDto)
		{

			using var client = new HttpClient();
			string json = JsonConvert.SerializeObject(inputDto);
			var requestData = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(String.Format("http://localhost:7071/api/updateui"), requestData);
		}
		#endregion


		private static DataTable ObjToDataTable<T>(T obj) where T : class
		{
			DataTable dt = new DataTable();
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
			foreach (PropertyDescriptor info in props)
			{
				dt.Columns.Add(info.Name, info.PropertyType);
			}
			dt.AcceptChanges();
			return dt;
		}
	}
}
