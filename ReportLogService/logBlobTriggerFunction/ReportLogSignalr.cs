using logBlobTriggerFunction.SQLHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace logBlobTriggerFunction
{
    public static class ReportLogSignalr
    {
        /// <summary>The Function is called via HTTP Post Request, 
        /// it negotiate a connection and sends information about the connection to a connect device.
        /// The first function to be called
        /// </summary
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "reportlog")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }



        /// <summary>The Function is called via HTTP Post Request 
        /// <example>For example: http://localhost7701/updateui 
        /// <param>{"data" : 1}</param>
        /// result-  push {"data" : 1} to all connected devices
        /// </example>
        /// </summary>
        [FunctionName("updateui")]
        public static Task UpdateUI(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
        [SignalR(HubName = "reportlog")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newUpdate",
                    Arguments = new[] { message }
                });
        } 
    }
}