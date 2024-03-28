using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class RTSEDSAManager : BaseManager, IRTSEDSAManager
    {

        async Task<PagingResult<RtsedsaTransaction>> IRTSEDSAManager.GetTransactionsAsync(TransactionRequest model)
        {
            var result = new PagingResult<RtsedsaTransaction>();

            RtsedsaTransactionRequest requestObj = new RtsedsaTransactionRequest();
            requestObj.Header = new Header
            {
                System = "SL",
                UserName = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString(),
                Password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString(),
            };

            requestObj.Date = Convert.ToInt64(model.Date);
            var req = JsonConvert.SerializeObject(requestObj, Formatting.Indented);

            string url = WebConfigurationManager.AppSettings["RTSEDSATransaction"].ToString();
            var resp = await SendHttpRequestAsync(url, HttpMethod.Post, req);

            var list = JsonConvert.DeserializeObject<RtsedsaTransactionResp>(resp);
            //for (var i = 0; i < list.Data.Count; i++)
            //{
            //    list.Data[i].DateTransaction = Utilities.ConvertEpochTimeToDate(Convert.ToInt64(list.Data[i].DateTransaction)).ToString();
            //}

            result.List = list.Data;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;

        }

        async Task<PagingResult<RtsedsaTransaction>> IRTSEDSAManager.GetSalesInquiry(InquiryRequest model)
        {
            var result = new PagingResult<RtsedsaTransaction>();
            //return null;
            RtsedsaInquiryRequest requestObj = new RtsedsaInquiryRequest();
            requestObj.Header = new Header
            {
                System = "SL",
                UserName = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString(),
                Password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString(),
            };

            requestObj.DateFrom = model.FromDate;
            requestObj.DateTo = model.ToDate;
            requestObj.MeterSerial = model.MeterSerial;

            var req = JsonConvert.SerializeObject(requestObj, Formatting.Indented);

            string url = WebConfigurationManager.AppSettings["RTSEDSAEnquiry"].ToString();
            var resp = await SendHttpRequestAsync(url, HttpMethod.Post, req);

            var list = JsonConvert.DeserializeObject<RtsedsaTransactionResp>(resp);

            //for ( var i = 0; i < list.Data.Count; i++)
            //{
            //    list.Data[i].DateTransaction = Utilities.ConvertEpochTimeToDate(Convert.ToInt64(list.Data[i].DateTransaction)).ToString();
            //}
            result.List = list.Data;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;

        }

        private async Task<string> SendHttpRequestAsync(string requestUrl, HttpMethod httpMethod, string requestBody = null)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(requestUrl),
                        Method = httpMethod,
                        Content = !string.IsNullOrEmpty(requestBody) ? new StringContent(requestBody, Encoding.UTF8, "application/json") : null
                    };

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();

                    return responseContent;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        List<SelectListItem> IRTSEDSAManager.MeterNumbers(long userID)
        {
            var result = new List<SelectListItem>();

                result = Context.Meters
                 .Where(p => !p.IsDeleted && p.UserId == userID && p.IsSaved == true && p.IsVerified == true)
                 .Select(x => new SelectListItem
                 {
                     Text = x.Name,
                     Value = x.Number
                 }).ToList();
            return result;
        }

        async Task<IceKloudResponse> IRTSEDSAManager.RequestVendAsync(RechargeMeterModel model)
        {
            IceKloudResponse response = new IceKloudResponse();
            IcekloudRequestmodel requestModel = new IcekloudRequestmodel();    
            HttpClient _httpClient = new HttpClient();
            var stringsResult = "";
            try
            {
                string url = (WebConfigurationManager.AppSettings["IsDevelopment"].ToString() == "1") ?
                         WebConfigurationManager.AppSettings["DevIcekloudURL"].ToString() :
                         WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

                requestModel = StackRequestModel(model);
                var req = JsonConvert.SerializeObject(requestModel, Formatting.Indented); 

                var icekloud_response = _httpClient.PostAsJsonAsync(url, req).Result;

                var strings_result = icekloud_response.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<IceKloudResponse>(strings_result);
                response.RequestModel = requestModel;
                return response;
            }
            catch (Exception)
            {
                try
                {
                    IceCloudErorResponse error_response = JsonConvert.DeserializeObject<IceCloudErorResponse>(stringsResult);

                    if (error_response.Status == "Error")
                    {
                        if (error_response.SystemError.ToLower() == "Unable to connect to the remote server".ToLower())
                        {
                            response.Status = "unsuccesful";
                            response.Content.Data.Error = error_response.SystemError;
                            response.RequestModel = requestModel;
                            return response;
                        }
                        if (error_response.SystemError.ToLower() == "The specified TransactionID already exists for this terminal.".ToLower())
                        {
                            model.TransactionId = model.TransactionId + 1;
                            return await RequestVendAsync(model);
                        }
                        response.Status = error_response?.Status;
                        response.Content.Data.Error = error_response?.Stack.ToArray()[0]?.Detail ?? error_response?.SystemError;
                        response.RequestModel = requestModel;
                        return response;
                    }
                }
                catch (Exception e) { throw e; }
                throw;
            }
        }

        private static IcekloudRequestmodel StackRequestModel(RechargeMeterModel model)
        {
            var username = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString();
            var password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString();
            return new IcekloudRequestmodel
            {
                Auth = new IcekloudAuth
                {
                    Password = password,
                    UserName = username
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                                     {
                        new
                        {
                            UserName = username,
                            Password = password,
                            System = "SL"
                        }, "apiV1_VendVoucher", "webapp", "0", "EDSA", $"{model.Amount}", $"{model.MeterNumber}", -1, "ver1.5", model.TransactionId
                       },
            };
        }

        async Task<IceKloudResponse> RequestVendAsync(RechargeMeterModel model)
        {
            IceKloudResponse response = new IceKloudResponse();
            IcekloudRequestmodel requestModel = new IcekloudRequestmodel();
            var stringsResult = "";
            try
            {
                string url = (WebConfigurationManager.AppSettings["IsDevelopment"].ToString() == "0") ?
                         WebConfigurationManager.AppSettings["DevIcekloudURL"].ToString() :
                         WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

                requestModel = StackRequestModel(model);
                var req = JsonConvert.SerializeObject(requestModel, Formatting.Indented);

                stringsResult = await SendHttpRequestAsync(url, HttpMethod.Post, req);
                response = JsonConvert.DeserializeObject<IceKloudResponse>(stringsResult);
                response.RequestModel = requestModel;
                return response;
            }
            catch (Exception)
            {
                try
                {
                    IceCloudErorResponse error_response = JsonConvert.DeserializeObject<IceCloudErorResponse>(stringsResult);

                    if (error_response.Status == "Error")
                    {
                        if (error_response.SystemError.ToLower() == "Unable to connect to the remote server".ToLower())
                        {
                            response.Status = "unsuccesful";
                            response.Content.Data.Error = error_response.SystemError;
                            response.RequestModel = requestModel;
                            return response;
                        }
                        if (error_response.SystemError.ToLower() == "The specified TransactionID already exists for this terminal.".ToLower())
                        {
                            model.TransactionId = model.TransactionId + 1;
                            return await (this as IRTSEDSAManager).RequestVendAsync(model);
                        }
                        response.Status = error_response?.Status;
                        response.Content.Data.Error = error_response?.Stack.ToArray()[0]?.Detail ?? error_response?.SystemError;
                        response.RequestModel = requestModel;
                        return response;
                    }
                }
                catch (Exception e) { throw e; }
                throw;
            }
        }


        async Task<Dictionary<string, IcekloudQueryResponse>> IRTSEDSAManager.QueryVendStatus(RechargeMeterModel model, TransactionDetail transDetail)
        {
            var response = new Dictionary<string, IcekloudQueryResponse>();

            var queryRequest = StackStatusRequestModel(model);
            var url = WebConfigurationManager.AppSettings["IcekloudURL"].ToString();
            var jsonRequest = JsonConvert.SerializeObject(queryRequest);
            var strings_result = await SendHttpRequestAsync(url, HttpMethod.Post, jsonRequest);

            var statusResponse = JsonConvert.DeserializeObject<IcekloudQueryResponse>(strings_result);
            var count = 0;
            if (!statusResponse.Content.Finalised)
            {
                count = count + 1;
                return await (this as IRTSEDSAManager).QueryVendStatus(model, transDetail);
            }
            else
            {
                transDetail.QueryStatusCount = count;
                if (string.IsNullOrEmpty(statusResponse.Content.VoucherPin))
                {
                    SaveChanges();
                    response.Add("failed", statusResponse);
                    return response;
                }
                else
                {
                    response.Add("success", statusResponse);
                    return response;
                }
            }
        }

        private static IcekloudRequestmodel StackStatusRequestModel(RechargeMeterModel model)
        {
            var username = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString();
            var password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString();
            return new IcekloudRequestmodel
            {
                Auth = new IcekloudAuth
                {
                    Password = password,
                    UserName = username
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                                     {
                        new
                        {
                            UserName = username,
                            Password = password,
                            System = "SL"
                        }, "apiV1_GetTransactionStatus", model.TransactionId
                       },
            };
        }
    }
}
