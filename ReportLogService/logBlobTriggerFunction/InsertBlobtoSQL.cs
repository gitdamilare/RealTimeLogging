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

			//var (IsSuccess, Logs, Message) = await DataTransformationAsync(myBlob, log);

			var result = await LogTransformationAsync(myBlob, log);
			watch.Start();
			var insertResult = await SqlHelper.InsertLogData(result.Logs, log, connectionString);
			watch.Stop();
			var executionTime = $"{watch.ElapsedMilliseconds}";
			if (insertResult.IsSuccess)
			{
				var insertProcessLog = await SqlHelper.InsertLogProcessData(new LogProcessData { FileName = name, ExecutionTime = executionTime }, log, connectionString);
				var makeCall = await CallSignlar();
				if (result.IsSuccess)
					log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
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


		public static async Task<string> CallSignlar()
		{
			using (var client = new HttpClient())
			{

				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("PARAM1", "VALUE1");
				dictionary.Add("PARAM2", "VALUE2");

				string json = JsonConvert.SerializeObject(dictionary);
				var requestData = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync(String.Format("http://localhost:7071/api/updateui"), requestData);
				var result = await response.Content.ReadAsStringAsync();

				return result;
			}
		}








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
