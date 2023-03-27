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
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class RTSEDSAManager : BaseManager, IRTSEDSAManager
    {

        async Task<PagingResult<RtsedsaTransaction>> IRTSEDSAManager.GetTransactionsAsync(UnixDateRequest model)
        {
            var result = new PagingResult<RtsedsaTransaction>();

            RtsedsaTransactionRequest requestObj = new RtsedsaTransactionRequest();
            requestObj.Header = new Header
            {
                System = "SL",
                UserName = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString(),
                Password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString(),
            };

            requestObj.Date = Convert.ToInt64(model.Date); //1678917600000;//  

            var req = JsonConvert.SerializeObject(requestObj, Formatting.Indented);

            string url = WebConfigurationManager.AppSettings["RTSEDSATransaction"].ToString();
            var resp = await SendHttpRequestAsync(url, HttpMethod.Post, req);

            var list = JsonConvert.DeserializeObject<RtsedsaTransactionResp>(resp);

            result.List = list.Data;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;

        }

        public static async Task<string> SendHttpRequestAsync(string requestUrl, HttpMethod httpMethod, string requestBody = null)
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

                    var response = httpClient.SendAsync(request).Result;
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();

                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
