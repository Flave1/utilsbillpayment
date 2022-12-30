using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;

namespace VendTech.BLL.PlatformApi
{
    public abstract class AbstractPlatformApiConnection : IPlatformApi
    {
        private HttpClient _client;

        public AbstractPlatformApiConnection() {
            //.Net 4.8
            //_client = new HttpClient (new HttpClientHandler { SslProtocols = SslProtocols.Tls12 })
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
        }

        public abstract IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFields();

        public abstract ExecutionResponse Execute(ExecutionContext executionContext);

       public abstract ExecutionResponse CheckStatus(ExecutionContext executionContext);

        protected void ExecutePOSTRequest(ApiRequestInfo apiRequestInfo, string url, HttpContent payload)
        {
            try
            {
                apiRequestInfo.RequestSent = DateTime.UtcNow;

                using (var responseTask = _client.PostAsync(url, payload))
                {
                    responseTask.Wait();
                    var result = responseTask.Result;
                    var resultReadTask = result.Content.ReadAsStringAsync();
                    resultReadTask.Wait();
                    apiRequestInfo.ResponseReceived = DateTime.UtcNow;
                    apiRequestInfo.Response = resultReadTask.Result;
                }
            }
            catch(TaskCanceledException e) when (e.InnerException is TimeoutException)
            {
                apiRequestInfo.IsTimeout = true;
                apiRequestInfo.TimeoutTime = DateTime.UtcNow;
                apiRequestInfo.ErrorMsg = e.Message;
            }
            catch (Exception e)
            {  
                apiRequestInfo.IsNotTimeoutButError = true;
                apiRequestInfo.ErrorMsg = e.Message;
            }
        }

        public IDictionary<string, Object> GetTransactionContext()
        {
            return null;
        }
    }


    public class ApiRequestInfo
    {
        [JsonProperty("timeout")]
        public bool IsTimeout { get; set; }
        [JsonProperty("timeoutTime")]
        public DateTime TimeoutTime { get; set; }

        [JsonProperty("requestSent")]
        public DateTime RequestSent { get; set; }

        [JsonProperty("responseReceived")]
        public DateTime ResponseReceived { get; set; }
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("request")]
        public string Request { get; set; }
        [JsonProperty("isNotTimeoutButError")]
        public bool IsNotTimeoutButError { get; set; }
        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }
        public string RequestSentStr
        {
            get
            {
                return RequestSent.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }
        public string ResponseReceivedStr
        {
            get
            {
                return ResponseReceived.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }
    }

}
