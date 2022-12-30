using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;

namespace VendTech.BLL.PlatformApi
{
    public interface IPlatformApi
    {
        ExecutionResponse Execute(ExecutionContext executionContext);
        ExecutionResponse CheckStatus(ExecutionContext executionContext);
        IDictionary<string, Object> GetTransactionContext();
        IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFields();
    }


    public class PlatformApiTypeConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
    }

    public class ExecutionContext
    {
        public const string REQUEST_SENT = "requestSent";
        public const string RESPONSE_RECEIVED = "responseReceived";
        public const string TRANX_ID = "id";
        public const string PLATFORM_ID = "platformId";
        public const string STATUS = "status";
        public const string AMOUNT = "amount";
        public const string OPERATOR_REFERENCE = "operatorReference";
        public const string USER_REFERENCE = "userReference";
        public const string TIMEOUT = "requestTimeout";
        public const string TIMEOUT_TIME = "requestTimeoutTime";


        public long TransactionId { get; set; }

        public IDictionary<string, string> PlatformApiConfig { get; set; }
        public IDictionary<string, string> PerPlatformParams { get; set; }
        public decimal Amount { get; set; }
        public string AccountId { get; set; }
        public string Msisdn { get; set; }
        public int Status { get; set; }
        public string UserReference { get; set; }   
        public string ApiTransactionId { get; set; }
    }

    public class ExecutionResponse
    {
        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }
        [JsonProperty("isError")]
        public bool IsError { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("operatorReference")]
        public string OperatorReference { get; set; }
        [JsonProperty("userReference")]
        public string UserReference { get; set; }
        [JsonProperty("apiTransactionId")]
        public string ApiTransactionId { get; set; }
        [JsonProperty("pinNumber")]
        public string PinNumber { get; set; }
        [JsonProperty("pinSerial")]
        public string PinSerial { get; set; }
        [JsonProperty("pinInstructions")]
        public string PinInstructions { get; set; }

        [JsonProperty("apiCalls")]
        public List<ApiRequestInfo> ApiCalls { get; }

        public ExecutionResponse()
        {
            ApiCalls = new List<ApiRequestInfo>();
        }

        public void AddApiCall(ApiRequestInfo apiReqInfo)
        {
            ApiCalls.Add(apiReqInfo);
        }

        public bool IsErrorResponse()
        {
            return Status == (int)TransactionStatus.Failed;
        }
    }
}
