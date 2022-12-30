using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.PlatformApi;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class PlatformApiLogModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("transactionId")]
        public long TransactionId { get; set; }
        [JsonProperty("apiLog")]
        public string ApiLog { get; set; }
        [JsonProperty("logType")]
        public int LogType { get; set; }
        [JsonProperty("logDate")]
        public DateTime LogDate { get; set; }
        [JsonProperty("apiLogJson")]
        public ExecutionResponse ApiLogJson
        { 
            get {
                return JsonConvert.DeserializeObject<ExecutionResponse>(ApiLog);
            } 
        }

        public string LogDateStr
        {
            get
            {
                return LogDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }

        public static PlatformApiLogModel From(PlatformApiLog log)
        {
            return new PlatformApiLogModel
            {
                Id = log.Id,
                TransactionId = log.TransactionId,
                ApiLog = log.ApiLog,
                LogType = log.LogType,
                LogDate = log.LogDate
            };
        }

        public static Expression<Func<VendTech.DAL.PlatformApiLog, PlatformApiLogModel>> Projection
        {
            get
            {
                
                return log => new PlatformApiLogModel()
                {
                    Id = log.Id,
                    TransactionId = log.TransactionId,
                    ApiLog = log.ApiLog,
                    LogType = log.LogType,
                    LogDate = log.LogDate,
                };
            }
        }
    }
}
