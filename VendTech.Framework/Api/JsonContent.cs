using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Common;

namespace VendTech.Framework.Api
{
    public class JsonContent : HttpContent
    {
        private readonly JToken _value;
        public JsonContent(object Data = null)
        {
            Response st = new Response { result = Data };

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _value = JObject.Parse(JsonConvert.SerializeObject(st, jsonSerializerSettings));
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
        public JsonContent(string message, Status status, object data = null, string token = null, char? verified = null, int? statusCode = null)
        {
            Response st = new Response { Message = message, Status = Utilities.GetDescription(typeof(Status), status), result = data, StatusCode = statusCode };

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _value = JObject.Parse(JsonConvert.SerializeObject(st, jsonSerializerSettings));
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
        public JsonContent(long totalCount, string message, Status status, object data = null)
        {
            Response st = new Response { Message = message, Status = Utilities.GetDescription(typeof(Status), status), result = data, TotalCount = totalCount };

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _value = JObject.Parse(JsonConvert.SerializeObject(st, jsonSerializerSettings));
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        protected override Task SerializeToStreamAsync(Stream stream,
            TransportContext context)
        {
            var jw = new JsonTextWriter(new StreamWriter(stream))
            {
                Formatting = Formatting.Indented
            };
            _value.WriteTo(jw);
            jw.Flush();
            return Task.FromResult<object>(null);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
