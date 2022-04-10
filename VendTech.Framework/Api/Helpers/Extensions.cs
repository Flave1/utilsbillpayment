using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VendTech.Framework.Api
{
    public static class Extensions
    {
        public static string GetClientIP(this HttpRequestMessage request)
        {
            return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
        }

        public static Dictionary<string, string> ToDictionary(this string keyValue)
        {
            return keyValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(part => part.Split('='))
                          .ToDictionary(split => split[0], split => split[1]);
        }

        public static HttpResponseMessage ConvertToHttpResponseOK(this JsonContent content)
        {
            return content.ConvertToHttpResponse(HttpStatusCode.OK);
        }

        public static HttpResponseMessage ConvertToHttpResponseISE(this JsonContent content)
        {
            return content.ConvertToHttpResponse(HttpStatusCode.InternalServerError);
        }

        public static HttpResponseMessage ConvertToHttpResponseNAUTH(this JsonContent content)
        {
            return content.ConvertToHttpResponse(HttpStatusCode.NonAuthoritativeInformation);
        }

        public static HttpResponseMessage ConvertToHttpResponse(this JsonContent content, HttpStatusCode status)
        {
            return new HttpResponseMessage()
            {
                StatusCode = status,
                Content = content
            };
        }

        public static string RemoveSpecialChars(this string value)
        {
            return value.Replace("%20", " ");
        }

        public static T ToModel<T>(this NameValueCollection model)
        {
            T instance = (T)Activator.CreateInstance(typeof(T));

            var allProps = instance.GetType().GetProperties();
            var keys = model.AllKeys;
            foreach (var key in keys)
            {
                var value = model[key];
                var prop = allProps.FirstOrDefault(p => p.Name.ToLower() == key.ToLower());
                if (prop != null && !string.IsNullOrEmpty(value))
                {
                    value = HttpUtility.UrlDecode(value);
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(instance, value);
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    {
                        prop.SetValue(instance, Convert.ToInt32(value));
                    }
                    else if (prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?))
                    {
                        prop.SetValue(instance, Convert.ToInt64(value));
                    }
                    else if (prop.PropertyType == typeof(short) || prop.PropertyType == typeof(short?))
                    {
                        prop.SetValue(instance, Convert.ToInt16(value));
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        prop.SetValue(instance, Convert.ToBoolean(value));
                    }
                    else if (prop.PropertyType == typeof(byte))
                    {
                        prop.SetValue(instance, Convert.ToByte(value));
                    }
                    else if (prop.PropertyType == typeof(decimal))
                    {
                        prop.SetValue(instance, Convert.ToDecimal(value));
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        prop.SetValue(instance, Convert.ToDouble(value));
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        prop.SetValue(instance, Convert.ToDateTime(value));
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        prop.SetValue(instance, Enum.Parse(prop.PropertyType, value, true));
                    }
                    else if (prop.PropertyType.IsArray)
                    {
                        var typeOfArray = prop.PropertyType.GetElementType();
                        var values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var arr = Array.CreateInstance(typeOfArray, values.Count());
                        for (int i = 0; i < arr.Length; i++)
                        {
                            arr.SetValue(Convert.ChangeType(values[i], typeOfArray), i);
                        }
                        prop.SetValue(instance, arr);
                    }
                    else
                    {
                        prop.SetValue(instance, value);
                    }
                }

            }
            return instance;
        }
    }
}
