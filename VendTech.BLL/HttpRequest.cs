using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL
{
    public static class Request
    {
        public static async Task<string> SendHttpRequestAsync(string requestUrl, HttpMethod httpMethod, string requestBody = null)
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
    }
}
