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


    public partial class Response
    {
        [JsonProperty("status")]
        public ResponseStatus Status { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("reference")]
        public long Reference { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productType")]
        public Country ProductType { get; set; }

        [JsonProperty("userReference")]
        public string UserReference { get; set; }

        [JsonProperty("operator")]
        public Operator Operator { get; set; }

        [JsonProperty("country")]
        public Country Country { get; set; }

        [JsonProperty("status")]
        public ResultStatus Status { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("amount")]
        public Amount Amount { get; set; }

        [JsonProperty("currency")]
        public Amount Currency { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
    }

    public partial class Amount
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("channel")]
        public string ChannelChannel { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public partial class Country
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Operator
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public partial class ResultStatus
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }
    }

    public partial class ResponseStatus
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }
    }
}
