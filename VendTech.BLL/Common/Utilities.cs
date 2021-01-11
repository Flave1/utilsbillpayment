using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using VendTech.DAL;
using MimeKit;
using System.Net.Mail;

namespace VendTech.BLL.Common
{
    public static class Utilities
    {
        public static decimal MinimumDepositAmount = 50;
        public static decimal MaximumDepositAmount = 500;
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GenerateUniqueId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        public static string GetLastMeterRechardeId()
        {
            //VendTechEntities context = new VendTechEntities();
            //long max = context.MeterRecharges.Max(p => p.MeterRechargeId);
            return GenerateTransStanNo();
        }

        private static string GenerateTransStanNo()
        {
            string transRef = "";
            VendTechEntities context = new VendTechEntities();
            StanTable stanTable = null;
            stanTable = context.StanTables.FirstOrDefault();
            if (stanTable == null)
            {
                stanTable = new StanTable();
                stanTable.Stan = 1;
                context.StanTables.Add(stanTable);
                context.SaveChanges();
            }

             stanTable.Stan += 1;// stanValue;
            int stanValue = stanTable.Stan;
            //context.StanTables.Add(stanTable);
            context.SaveChanges();
            transRef = Convert.ToString(stanValue).PadLeft(11, '0');
            return transRef;
        }


        public static int GetUserRoleIntValue(string role)
        {
            var db=new VendTechEntities();
            var record = db.UserRoles.FirstOrDefault(p => p.Role == role);
            if (record == null)
                return 0;
            return record.RoleId;
        }
        public static string FormatBankAccount(string bankAccount)
        {
            var length = (bankAccount.Length)-4;
            string formattedString = "";
            for (int i = 0; i < length; i++)
            {
                formattedString += "x";
            }
            return String.Format("{0}{1}", formattedString, bankAccount.Substring(bankAccount.Length - 4));
        }
        public static List<SelectListItem> EnumToList(Type en)
        {
            var itemValues = en.GetEnumValues();
            var list = new List<SelectListItem>();

            foreach (var value in itemValues)
            {
                var name = en.GetEnumName(value);
                var member = en.GetMember(name).Single();
                var desc = ((DescriptionAttribute)member.GetCustomAttributes(typeof(DescriptionAttribute), false).Single()).Description;
                list.Add(new SelectListItem { Text = desc.ToUpper(), Value = ((int)value).ToString().ToUpper() });
            }
            return list;
        }

        public static string DomainUrl
        {
            get
            {
                var domain = string.Empty;
                Uri url = System.Web.HttpContext.Current.Request.Url;
                domain = url.AbsoluteUri.Replace(url.PathAndQuery, string.Empty);
                return domain;
            }
        }

        public static string GetDescription(Type en, object value, bool getText = false)
        {
            try
            {
                var name = en.GetEnumName(value);
                var member = en.GetMember(name).Single();
                var desc = getText ? name : value.ToString();
                var descAttr = (DescriptionAttribute)member.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault();
                if (descAttr != null) desc = descAttr.Description;
                return desc;
            }
            catch { return ""; };
        }

        public static string EncryptPassword(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        public static string DecryptPassword(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }
        public static int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string GetUniqueKey(int maxSize = 15)
        {
            char[] chars = new char[62];
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public static bool SendEmailOld(string to, string sub, string body)
        {
            try
            {
                MimeMessage mail = new MimeMessage();
                mail.From.Add(new MailboxAddress(WebConfigurationManager.AppSettings["SMTPFrom"].ToString(), WebConfigurationManager.AppSettings["SMTPDisplayName"].ToString()));
                mail.To.Add(new MailboxAddress(to, to));
                mail.Subject = sub;
                mail.Body = new TextPart("html")
                {
                    Text = body
                }; 

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                    client.Connect(WebConfigurationManager.AppSettings["SMTPHost"].ToString(), Convert.ToInt32(WebConfigurationManager.AppSettings["SMTPPort"]), false); 
                    client.AuthenticationMechanisms.Remove("XOAUTH2"); 
                    client.Authenticate("favouremmanuel433@gmail.com", "85236580Go"); 
                    client.Send(mail); 
                    client.Disconnect(true);
                }

                
                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient(WebConfigurationManager.AppSettings["SMTPHost"].ToString());

                //mail.From = new MailAddress(WebConfigurationManager.AppSettings["SMTPFrom"].ToString(), WebConfigurationManager.AppSettings["SMTPDisplayName"].ToString());
                //mail.To.Add(to);
                //mail.Subject = sub;
                //mail.Body = body;
                ////SmtpServer.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["SMTPPort"]); 
                //SmtpServer.Port = 587;
                //SmtpServer.UseDefaultCredentials = true;
                //SmtpServer.Credentials = new System.Net.NetworkCredential("favouremmanuel433@gmail.com", "85236580Gm");//WebConfigurationManager.AppSettings["SMTPUsername"].ToString(), WebConfigurationManager.AppSettings["SMTPPassword"].ToString());
                //SmtpServer.EnableSsl = true;
                //mail.IsBodyHtml = true;
                //SmtpServer.Send(mail);
                return true;
            }
            catch (Exception x)
            { throw x;    }
            
        }
        public static bool SendEmail(string to, string sub, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                System.Net.Mail.SmtpClient SmtpServer = new System.Net.Mail.SmtpClient();

                mail.From = new MailAddress(WebConfigurationManager.AppSettings["SMTPFrom"].ToString(), WebConfigurationManager.AppSettings["SMTPDisplayName"].ToString());
                mail.To.Add(to);
                mail.Subject = sub;
                mail.Body = body;
                //SmtpServer.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["SMTPPort"]); 
                //SmtpServer.Port = 587;
                //SmtpServer.UseDefaultCredentials = false;
                //SmtpServer.Credentials = new System.Net.NetworkCredential("favouremmanuel433@gmail.com", "85236580Gm");//WebConfigurationManager.AppSettings["SMTPUsername"].ToString(), WebConfigurationManager.AppSettings["SMTPPassword"].ToString());
                //SmtpServer.EnableSsl = true;
                mail.IsBodyHtml = true;
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception x)
            { return true;    }
            
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static DateTime GetLocalDateTime()
        {
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            DateTime utcTime = DateTime.UtcNow;
            DateTime servertimetotest = DateTime.Now;
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(curTimeZone.StandardName);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi); // con
            return localTime;
        }

        public static string GetNumbersFromGuid() {
            var guidstring = Guid.NewGuid().ToString("N");
            var getNumbers = (from t in guidstring
                              where char.IsDigit(t)
                              select t).ToArray();
            return string.Join("", getNumbers);// getNumbers.Split(',').Select(Int32.Parse).ToList();// getNumbers;
        } 
    }
}
