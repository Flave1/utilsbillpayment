using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.BLL.Models;

namespace VendTech.BLL.PlatformApi
{
    public class PlatformApi_Sochitel : AbstractPlatformApiConnection
    {
        public override ExecutionResponse CheckStatus(ExecutionContext executionContext)
        {
            ExecutionResponse response = new ExecutionResponse();

            ConfigValidationReport validationReport = ValidateConfig(executionContext);
            if (validationReport.IsInvalid)
            {
                //We can set this to Failed because we have not performed the check yet
                response.Status = (int)TransactionStatus.Pending;
                response.ErrorMsg = validationReport.ValidationMsg;
                return response;
            }

            string username = executionContext.PlatformApiConfig["username"];
            string password = executionContext.PlatformApiConfig["password"];
            string url = executionContext.PlatformApiConfig["url"];

            var salt = GenerateSalt();

            ApiAuth auth = new ApiAuth(username, password, salt);
            var getTransactionCmd = new GetTransaction(auth, executionContext.UserReference, executionContext.ApiTransactionId);
            string json = JsonConvert.SerializeObject(getTransactionCmd);
            var payload = new StringContent(json, Encoding.UTF8, "application/json");

            ApiRequestInfo apiRequestInfo = new ApiRequestInfo
            {
                Request = json,
            };

            base.ExecutePOSTRequest(apiRequestInfo, url, payload);

            response.AddApiCall(apiRequestInfo);

            //If there is a timeout we will flag as pending so it will be checked later
            if (apiRequestInfo.IsTimeout || apiRequestInfo.IsNotTimeoutButError)
            {
                if (apiRequestInfo.IsNotTimeoutButError)
                {
                    response.ErrorMsg = apiRequestInfo.ErrorMsg;
                }

                response.Status = (int)TransactionStatus.Pending;
                return response;
            }

            if (apiRequestInfo.Response.Contains("\"result\":[]"))
            {
                //Our model has a 'result' field that is a Dictionary.
                //Wewill change the empty array to an empty object
                apiRequestInfo.Response = apiRequestInfo.Response.Replace("\"result\":[]", "\"result\":{}");
            }

            //API Request was successful from a network perspective.
            ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(apiRequestInfo.Response);
            ApiResponseStatus status = apiResponse.Status;

            ProcessStatus(response, status.Id);

            if (apiResponse.Result != null)
            {
                ProcessResultDictionary(response, apiResponse.Result);
            }

            return response;
        }

        public override ExecutionResponse Execute(ExecutionContext executionContext)
        {
            ExecutionResponse response = new ExecutionResponse();

            ConfigValidationReport validationReport = ValidateConfig(executionContext);
            if (validationReport.IsInvalid)
            {
                response.Status = (int)TransactionStatus.Failed;
                response.ErrorMsg = validationReport.ValidationMsg;
                return response;
            }

            string username = executionContext.PlatformApiConfig["username"];
            string password = executionContext.PlatformApiConfig["password"];
            string url = executionContext.PlatformApiConfig["url"];
            string operatorId = executionContext.PerPlatformParams["operatorId"];
            string productId = executionContext.PerPlatformParams["productId"];
            string msisdn = executionContext.Msisdn;
            string accountId = executionContext.AccountId;
            decimal amount = executionContext.Amount;

            var salt = GenerateSalt();
            var userReference = GenerateSalt().Substring(0, 30);
            //set the user reference on the response.
            response.UserReference = userReference;

            ApiAuth auth = new ApiAuth(username, password, salt);
            var execTransactionCmd = new ExecuteTransaction(auth, userReference, operatorId, productId, amount, msisdn, accountId);
            string json = JsonConvert.SerializeObject(execTransactionCmd);
            var payload = new StringContent(json, Encoding.UTF8, "application/json");

            ApiRequestInfo apiRequestInfo = new ApiRequestInfo
            {
                Request = json,
            };

            base.ExecutePOSTRequest(apiRequestInfo, url, payload);

            response.AddApiCall(apiRequestInfo);

            //If there is a timeout we will flag as pending so it will be checked later
            if (apiRequestInfo.IsTimeout)
            {
                response.Status = (int)TransactionStatus.Pending;
                return response;
            }

            if (apiRequestInfo.IsNotTimeoutButError)
            {
                response.Status = (int)TransactionStatus.Failed;
                response.ErrorMsg = apiRequestInfo.ErrorMsg;
                return response;
            }

            //API Request was successful from a network perspective.

            ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(apiRequestInfo.Response);
            ApiResponseStatus status = apiResponse.Status;

            ProcessStatus(response, status.Id);
            
            if (apiResponse.Result != null)
            { 
                ProcessResultDictionary(response, apiResponse.Result);
            }

            return response;
        }

        private static void ProcessStatus(ExecutionResponse response, int status)
        {
            switch (status)
            {
                //successful
                case 0:
                    response.Status = (int)TransactionStatus.Successful;
                    break;

                //Pending/In Progress
                case 9:
                case 46:
                    response.Status = (int)TransactionStatus.Pending;
                    break;
                //Any other status is failed
                default:
                    response.Status = (int)TransactionStatus.Failed;
                    break;
            }
        }

        private static void ProcessResultDictionary(ExecutionResponse response, Dictionary<string, dynamic> result)
        {
            if (result.ContainsKey("id"))
            {
                response.ApiTransactionId = (string)result["id"];
            }
            if (result.ContainsKey("operator"))
            {
                //var operatorField = result["operator"]["reference"];
                //var reference = operatorField["reference"];
                response.OperatorReference = result["operator"]["reference"];
            }
            if (result.ContainsKey("pin"))
            {
                var pin = result["pin"];
                response.PinNumber = pin["number"];
                response.PinSerial = pin["serial"];
                response.PinInstructions = pin["instructions"];
            }
        }

        public override IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFields()
        {
            return GetApiConfig();
        }

        private static IDictionary<string, PlatformApiConfig> GetApiConfig()
        {
            Dictionary<string, PlatformApiConfig> configFields = new Dictionary<string, PlatformApiConfig>()
            {
                { "url", new PlatformApiConfig { Name = "URL", IsUrl = true, HtmlCssClass = "form-control"} },
                { "username", new PlatformApiConfig { Name = "Username", HtmlCssClass = "form-control" } },
                { "password", new PlatformApiConfig { Name = "Password", HtmlCssClass = "form-control" } },
                { "productId", new PlatformApiConfig {
                        Name = "Product Id",
                        HtmlCssClass = "form-control",
                        IsPerPlatformProductParam = true,
                        HtmlStyle = "width: 30%"
                    }
                },
                { "operatorId", new PlatformApiConfig {
                        Name = "Operator ID",
                        HtmlCssClass = "form-control",
                        IsPerPlatformProductParam = true,
                        HtmlStyle = "width: 30%"
                    }
                }
            };

            return configFields;
        }

        private static string GenerateSalt()
        {
            var guid = Guid.NewGuid();
            return guid.ToString().Replace("-", "");
        }

        private static ConfigValidationReport ValidateConfig(ExecutionContext executionContext)
        {
            ConfigValidationReport validationReport = new ConfigValidationReport();
            validationReport.IsInvalid = true;

            //Get the Per Platform Parameters where the Operator and Product is set.
            if (!executionContext.PlatformApiConfig.ContainsKey("url"))
            {
                validationReport.ValidationMsg = "URL not configured";
                return validationReport;
            }
            if (!executionContext.PlatformApiConfig.ContainsKey("username"))
            {
                validationReport.ValidationMsg = "Username not configured";
                return validationReport;
            }
            if (!executionContext.PlatformApiConfig.ContainsKey("password"))
            {
                validationReport.ValidationMsg = "Password not configured";
                return validationReport;
            }

            //Get the Per Platform Parameters where the Operator and Product is set.
            if (!executionContext.PerPlatformParams.ContainsKey("operatorId"))
            {
                validationReport.ValidationMsg = "Operator ID not configured";
                return validationReport;
            }

            if (!executionContext.PerPlatformParams.ContainsKey("productId"))
            {
                validationReport.ValidationMsg = "Product ID not configured";
                return validationReport;
            }

            validationReport.IsInvalid = false;
            return validationReport;
        }

        internal class ConfigValidationReport
        {
            public bool IsInvalid { get; set; }
            public string ValidationMsg { get; set; }
        }

        public class ApiResponse
        {
            [JsonProperty("status")]
            public ApiResponseStatus Status { get; set; }

            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("reference")]
            public string Reference { get; set; }

            [JsonProperty("result")]
            public Dictionary<string, dynamic> Result { get; set; }
        }
        public class ApiResponseStatus
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("type")]
            public int Type { get; set; }
            [JsonProperty("typeName")]
            public string TypeName { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }

    public class ArtxModel
    {
        [JsonProperty("auth")]
        public ApiAuth Auth { get; }

        [JsonProperty("command")]
        public string Command { get; }

        [JsonProperty("version")]
        public string Version = "5";

        public ArtxModel(ApiAuth auth, string command)
        {
            this.Auth = auth;
            this.Command = command;
        }
    }

    public class ApiAuth
    {
        [JsonProperty("username")]
        public string UserName { get; }

        [JsonProperty("password")]
        public string Password { get; }

        [JsonProperty("salt")]
        public string Salt { get; }

        public ApiAuth(string username, string password, string salt)
        {
            //@todo - validate parameters
            this.UserName = username;
            this.Password = Utility.GeneratePasswordHash(salt, password);
            this.Salt = salt;
        }
    }

    public class ExecuteTransaction : ArtxModel
    {
        [JsonProperty("operator")]
        public string Operator { get; }
        [JsonProperty("productId")]
        public string Product { get; }
        [JsonProperty("amount")]
        public decimal Amount { get; }
        [JsonProperty("msisdn")]
        public string Msisdn { get; }
        [JsonProperty("accountId")]
        public string AccountId { get; }
        [JsonProperty("userReference")]
        public string UserReference { get; }

        public ExecuteTransaction(ApiAuth auth,
            string userReference,
            string operatorId,
            string productId,
            decimal amount,
            string msisdn,
            string accountId) : base(auth, "execTransaction")
        {
            //@todo - validate parameters
            this.Operator = operatorId;
            this.Product = productId;
            this.Amount = amount;
            this.Msisdn = msisdn;
            this.AccountId = accountId;
            this.UserReference = userReference;
        }
    }

    /**
     * Model of the 'getTransaction' command
     */
    public class GetTransaction : ArtxModel
    {
        [JsonProperty("userReference")]
        public string userReference { get; }
        [JsonProperty("id")]
        public string Id { get; }

        public GetTransaction(
            ApiAuth auth,
            string userReference,
            string id) : base(auth, "getTransaction")
        {
            //@todo - validate parameters
            this.userReference = userReference;
            this.Id = id;
        }
    }

    public class Utility
    {
        public static string SHA_1(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input to hash cannot be null or empty.");

            using (var sha1 = SHA1.Create())
            {
                var hashSh1 = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                // declare stringbuilder
                var sb = new StringBuilder(hashSh1.Length * 2);

                // computing hashSh1
                foreach (byte b in hashSh1)
                {
                    // "x2"
                    sb.Append(b.ToString("X2").ToLower());
                }

                // final output
                //Console.WriteLine(string.Format("The SHA1 hash of {0} is: {1}",input,sb.ToString()));

                return sb.ToString();
            }
        }

        public static string GeneratePasswordHash(string salt, string password)
        {
            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Salt cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty.");

            var hash = SHA_1(password);
            var finalHash = SHA_1(salt + hash);

            return finalHash;
        }
    }
}
