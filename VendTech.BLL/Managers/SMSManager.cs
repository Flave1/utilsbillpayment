using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.BLL.Managers
{
    public class SMSManager : BaseManager, ISMSManager
    {
        async Task<ActionOutput> ISMSManager.SendSmsAsync(SendSMSRequest model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri(WebConfigurationManager.AppSettings["SMSAPI"].ToString()); 
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await client.SendAsync(httpRequest);
                var stringResult = res.Content.ReadAsStringAsync().Result;
                return new ActionOutput { Message = "SMS Sent Successfully", Status = ActionStatus.Successfull };
            }
            catch (HttpRequestException e)
            {
                return new ActionOutput { Message = e.Message, Status = ActionStatus.Error };
            }
            catch (Exception x)
            {
                return new ActionOutput { Message = x.Message, Status = ActionStatus.Error };
            }

        }
    }
}
