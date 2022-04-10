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
    public class SMSManager : ISMSManager
    {
        async Task<bool> ISMSManager.SendSmsAsync(SendSMSRequest model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri("https://kwiktalk.io");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await client.SendAsync(request);
                return true;
            }
            catch (HttpRequestException e)
            {
                throw e;
            }
            catch (Exception x)
            {
                throw x;
            }

        }
    }
}
