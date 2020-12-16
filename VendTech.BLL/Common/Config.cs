using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Common
{
    public static class Config
    {
        public static bool LogExceptionInDatabase
        {
            get { return Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["LogExceptionInDB"].ToString()); }
        }
        public static string GetWebApiKey
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ApiKeyForPush"].ToString(); }
        }
        public static string GetSenderId
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["SenderIdForPush"].ToString(); }
        }

        public static string FCMUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["FCMUrl"].ToString(); }
        }
    }
}
