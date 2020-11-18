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
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "reportlog")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }


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