using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using MimeKit;
using System.Net.Mail;
using System.Net;
using VendTech.DAL;
using System.Globalization;
using iTextSharp.text;
//using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using Patagames.Pdf.Net;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models.CurrencyModel;
using iTextSharp.tool.xml.html;
using System.Xml;
using HtmlAgilityPack;
using MailKit.Net.Smtp;
using System.Web.Mail;
using VendTech.BLL.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;

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
            VendtechEntities context = new VendtechEntities();
            var lastRecord = context.TransactionDetails.OrderByDescending(d =>  d.TransactionDetailsId).FirstOrDefault();
            var trId = lastRecord != null ? Convert.ToInt64(lastRecord.TransactionId) + 1 : 1;
            return trId.ToString();
        }

        //public static string GetLastDepositTransactionId()
        //{
        //    VendtechEntities context = new VendtechEntities();
        //    var existing_details = context.Deposits.Where(p => p.IsDeleted == false).AsEnumerable();
        //    long max = existing_details.Any() ? existing_details.Max(p => Convert.ToInt64(p.TransactionId)) : 1;
        //    max = max + 1;
        //    return max.ToString();
        //}

        public static string GetLastDepositTransactionId()
        {
            using (VendtechEntities context = new VendtechEntities())
            {
                var tranx = context.Deposits
                 .OrderByDescending(p => p.DepositId)
                 .FirstOrDefault();

                if (tranx != null)
                {
                    long maxTransactionId = Convert.ToInt64(tranx.TransactionId) + 1;
                    return maxTransactionId.ToString();
                }
                return "1";
            }
        }

        private static string GenerateTransStanNo()
        {
            string transRef = "";
            VendtechEntities context = new VendtechEntities();
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
            transRef = Convert.ToString(stanValue); ;// PadLeft(11, '0');
            return transRef;
        }

        public static long VENDTECH
        {
            get{  return 20; }
        }

        public static string EMAILFOOTERTEMPLATE
        {
            get {
                return $"VENDTECH MANAGEMENT" + 
                        $"</br></br>" +
                        $"Phone: +232 79 990990" +
                        $"</br>" +
                        $"Email: support@vendtechsl.com" +
                        $"</br>" +
                        $"Website: www.vendtechsl.com" +
                        $"{Environment.NewLine}" +
                        $"Address: 186 Wilkinson Rd, Freetown - Sierra Leone";
            }
        }


        public static int GetUserRoleIntValue(string role)
        {
            var db = new VendtechEntities();
            var record = db.UserRoles.FirstOrDefault(p => p.Role == role);
            if (record == null)
                return 0;
            return record.RoleId;
        }
        public static string FormatBankAccount(string bankAccount)
        {
            var length = (bankAccount.Length) - 4;
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


        public static CountryDTO2 GetCountry()
        {
            var _countryManager = DependencyResolver.Current.GetService<ICurrencyManager>();
            return _countryManager.RetrieveDomainCountry(DomainUrl);
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

        public static string EncryptText(string text)
        {
            try
            {
                if (!String.IsNullOrEmpty(text))
                {
                    byte[] encData_byte = new byte[text.Length];
                    encData_byte = System.Text.Encoding.UTF8.GetBytes(text);
                    string encodedData = Convert.ToBase64String(encData_byte);
                    return encodedData;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        public static string DecryptText(string encodedText)
        {
            if (!String.IsNullOrEmpty(encodedText))
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecode_byte = Convert.FromBase64String(encodedText);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            return "";
        }

        public static string GenerateByAnyLength(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rn = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            return $"vtsl{rn}";
        }

        public static int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public static int GenerateFiveRandomNo()
        {
            var result = 0;
            int _min = 10000;
            int _max = 99999;
            Random _rdm = new Random();
            result = _rdm.Next(_min, _max);
            var db = new VendtechEntities();
            if (db.POS.Any(e => e.PassCode == result.ToString())) 
                GenerateFiveRandomNo(); 
            return result;
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
                    client.Authenticate("Favouremmanuel433@gmail.com", "85236580Go");
                    client.Send(mail);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception x)
            { throw x; }

        }

        public static string LogExceptionToDatabase(Exception exc)
        {
            var context = new VendtechEntities();
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = exc.Message;
            errorObj.StackTrace = exc.StackTrace;
            errorObj.InnerException = exc.InnerException == null ? "" : exc.InnerException.Message;
            errorObj.LoggedInDetails = "";
            errorObj.LoggedAt = DateTime.UtcNow;
            errorObj.UserId = 0;
            context.ErrorLogs.Add(errorObj);
            // To do
            context.SaveChanges();
            return errorObj.ErrorLogID.ToString();
        }

        public static void SendEmail(string to, string sub, string body)
        {
            string from = WebConfigurationManager.AppSettings["SMTPFromtest"].ToString();
            string password = "Supt*VT&ch"; //WebConfigurationManager.AppSettings["SMTPPassword"].ToString();
            string displayName = WebConfigurationManager.AppSettings["SMTPDisplayName"].ToString();
            string smtp = "smtp.office365.com"; //"smtp.gmail.com";
            int port = 587;//465;
            try
            {
               
                var mimeMsg = new MimeMessage();
                var frms = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, from),
                };
                var tos = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, to),
                };
                mimeMsg.From.AddRange(frms);
                mimeMsg.To.AddRange(tos);
                mimeMsg.Subject = sub;
                
                mimeMsg.Body = new TextPart("html")
                {
                    Text = body
                };
                
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                   
                    client.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    
                    client.Connect(smtp, port);
                    
                    client.Authenticate(from, password);
                   
                    client.Send(mimeMsg);
                   
                    client.Disconnect(true);
                    
                }
            }
            catch (Exception x)
            {
                LogExceptionToDatabase(new Exception("SendEmail err 1", x));
                //LogExceptionToDatabase(x);
                //return true;
            }

        }

        public static bool SendSms(SendSMSRequest request)
        {
            //request.Recipient = null;
            var json = JsonConvert.SerializeObject(request);
            string baseUrl = WebConfigurationManager.AppSettings["SMSAPI"].ToString();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = client.SendAsync(httpRequest).Result;
            //var stringResult = res.Content.ReadAsStringAsync().Result;
            return res.IsSuccessStatusCode;
        }
        public static void SendPDFEmail(string to, string sub, string body, string file = "", string name = "")
        {
            string from = WebConfigurationManager.AppSettings["SMTPFromtest"].ToString();
            string password = "Supt*VT&ch"; //WebConfigurationManager.AppSettings["SMTPPassword"].ToString();
            string displayName = WebConfigurationManager.AppSettings["SMTPDisplayName"].ToString();
            string smtp = "smtp.office365.com"; //"smtp.gmail.com";
            int port = 587;
            try
            {

                var mimeMsg = new MimeMessage();
                var frms = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, from),
                };
                var tos = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, to),
                };
                mimeMsg.From.AddRange(frms);
                mimeMsg.To.AddRange(tos);
                mimeMsg.Subject = sub;

                var multipart = new Multipart("mixed");
                var content = new TextPart("html")
                {
                    Text = body
                };

                multipart.Add(content);

                if (!string.IsNullOrEmpty(file))
                {
                    var attachment = new MimePart("application", "pdf")
                    {
                        Content = new MimeContent(File.OpenRead(file)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(name)
                    };
                    multipart.Add(attachment);
                }

                mimeMsg.Body = multipart;

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.Connect(smtp, port);

                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(from, password);

                    client.Send(mimeMsg);

                    client.Disconnect(true);
                }
            }
            catch (Exception x)
            {
                //LogExceptionToDatabase(x);
                //return true;
            }

        }


    

        public static void LogProcessToDatabase(string Message, object data)
        {
            var context = new VendtechEntities();
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = Message;
            errorObj.StackTrace = typeof(Utilities).ToString();
            errorObj.InnerException = "";
            errorObj.LoggedInDetails = "";
            errorObj.LoggedAt = DateTime.UtcNow;
            context.ErrorLogs.Add(errorObj);
            // To do
            context.SaveChanges();
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
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

        public static string GetNumbersFromGuid()
        {
            var guidstring = Guid.NewGuid().ToString("N");
            var getNumbers = (from t in guidstring
                              where char.IsDigit(t)
                              select t).ToArray();
            return string.Join("", getNumbers.Take(20));// getNumbers.Split(',').Select(Int32.Parse).ToList();// getNumbers;
        }
          
        public static string FormatThisToken(string token_item)
        {
            if (token_item != null && token_item.Length >= 2 && token_item.Length <= 12)
                token_item = token_item.Insert(4, " ").Insert(9, " ");
            else if (token_item != null && token_item.Length >= 12 && token_item.Length <= 16)
                token_item = token_item.Insert(4, " ").Insert(9, " ").Insert(14, " ");
            else if (token_item != null && token_item.Length >= 16 && token_item.Length <= 21)
                token_item = token_item.Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " "); 


            return token_item;
        }
       
        //public async static Task<bool> Execute(string email, string subject, string message)
        //{
        //    try
        //    {
        //        string toEmail = email;

        //        MailMessage mail = new MailMessage()
        //        {
        //            From = new MailAddress(WebConfigurationManager.AppSettings["SMTPFrom"].ToString(), "VendTech")
        //        };
        //        mail.To.Add(toEmail);

        //        //mail.To.Add(new MailAddress(toEmail));
        //        //mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

        //        mail.Subject = subject;
        //        mail.Body = message;
        //        mail.IsBodyHtml = true;
        //        mail.Priority = MailPriority.High;

        //        using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(WebConfigurationManager.AppSettings["SMTPHost"].ToString(), Convert.ToInt32(WebConfigurationManager.AppSettings["SMTPPort"].ToString())))
        //        {
        //            smtp.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SMTPUserName"].ToString(), WebConfigurationManager.AppSettings["SMTPPassword"].ToString());
        //            smtp.EnableSsl = true;
        //            await smtp.SendMailAsync(mail);
        //        }
        //        return true;
        //    }

        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public static string SHA256(string randomString)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }


        public static string FormatAmount(decimal? amt)
        {
            if (amt.ToString().Contains('.'))
            {
                var splitedAmt = amt.ToString().Split('.');
                var d =  "." + splitedAmt[1];
                var result = amt == null ? "0" : string.Format("{0:N0}", Convert.ToDecimal(splitedAmt[0])) + "" + d;
                return result;
            }
            else
            {
                return amt == null ? "0" : string.Format("{0:N0}", amt) + "";
            }
        }

        public static int WeekOfYearISO8601(DateTime date)
        {
            var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static List<int> WeekOfYearISO8601(List<DateTime> dates)
        {
            List<int> weeks = new List<int>();
            for (int i = 0; i < dates.Count(); i++)
            {
                var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(dates[i]);
                weeks.Add(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dates[i].AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
            }
            return weeks.Distinct().ToList();
        }

        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string ReplaceWhitespace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }

        public static Dictionary<string, string> CreatePdf(string content, string transctionId)
        {

            // create a new PDF document
            Document document = new Document();
            try
            {
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fileName =  transctionId;
                string path = rootDirectory + "/Receipts/" + fileName;
                var relativePath = DomainUrl + "/Receipts/" + fileName;
                // create a PDF writer to write the document to a file
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));

                // open the document
                document.Open();

                // create a new XML parser
                XMLWorkerHelper parser = XMLWorkerHelper.GetInstance();

                // create a string with the HTML content to be converted to PDF
                string htmlContent = content;


                // convert the HTML content to PDF and add it to the document
                parser.ParseXHtml(writer, document, new StringReader(content));

                // close the document
                document.Close();

                var result = new Dictionary<string, string>();
                result.Add(relativePath, path);
                return result;

            }
            catch (Exception ex)
            {
                document.Close();
                throw new ArgumentException("Unable to create PDF");
            }
        }
        public static string CreateImage(string pdfFilePath)
        {
            string fileName = "receipt.jpeg";
            string outputImagePath1 = DomainUrl + "/Receipts/" + fileName;
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string outputImagePath12 = rootDirectory + "/Receipts/" + fileName;
            try
            {
                // Open the PDF document
                using (var pdfDocument = PdfDocument.Load(pdfFilePath))
                {
                    // Get the first page
                    var pdfPage = pdfDocument.Pages[0];

                    // Create a Bitmap to render the page
                    var bitmap = new Bitmap((int)pdfPage.Width, (int)pdfPage.Height);

                    // Set up the Graphics object for rendering
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        // Render the PDF page to the Graphics object
                        pdfPage.Render(graphics, 96, 96, (int)pdfPage.Width, (int)pdfPage.Height, 0, 0);
                    }

                    // Save the bitmap as an image file
                    bitmap.Save(outputImagePath12, ImageFormat.Jpeg);
                  
                }
                return outputImagePath1;
            }
            catch (Exception ex)
            {
                // Handle error.
                Console.WriteLine("Error: " + ex.Message);
                return outputImagePath1;
            }
        }

        public static void DeleteFileFromDirectory(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException ex)
            {
                throw;
            }
            
        }
        public static DateTime ConvertEpochTimeToDate(long epochTime)
        {
            //long epochTime = 1622697228; // Unix epoch time in seconds
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                    .AddSeconds(epochTime);
            return dateTime;
        }

        public static string ConvertDateToEpochDate(string dateString)
        {
            var splited = dateString.Split('/').Select(int.Parse).ToList();
            DateTime date = new DateTime(splited[2], splited[1], splited[0]); // Corrected day, month, year order
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Set the epoch date
            TimeSpan timeSpan = date - epoch; // Get the difference between the two dates
            long epochTime = (long)timeSpan.TotalSeconds; // Convert the difference to seconds and cast to long
            return epochTime.ToString();

            //DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //TimeSpan timeSpan = date - epoch;
            //long epochTimeInSeconds = (long)timeSpan.TotalSeconds;
            //return epochTimeInSeconds.ToString();
        }

        public static long ToUnixTimestamp(DateTime value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("DateTime to convert to Unix Timestamp cannot be null");
            }

            return (long)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        public static string ReaFromTemplateFile(string fileName)
        {

            var _errorManager = DependencyResolver.Current.GetService<IErrorLogManager>();
            // Get the current application directory
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return null;
            // Combine the directory and file name to get the full path
            //string filePath = Path.Combine(currentDirectory, "Templates/"+fileName);
            string filePath = @"C:\Inetpub\vhosts\vendtechsl.com\httpdocs\Templates\DepositPDF.html";
            filePath = filePath.Replace("\\", "/");

            try
            {
                // Read the file content
                string content = File.ReadAllText(filePath);
                return content; 
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"The file '{fileName}' was not found in the current applicatio   n directory.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return null;
        }

        public static string RemoveRow(string htmlContent)
        {

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            HtmlNode rowToRemove = FindRowById(htmlDoc, "commission");
            //HtmlNode rowToRemove = FindRowByStyle(htmlDoc, "display: none");
            if (rowToRemove != null)
            {
                rowToRemove.Remove();
                string modifiedHtml = htmlDoc.DocumentNode.OuterHtml;
                return modifiedHtml;
            }
            else
            {
                return htmlContent;
            }
        }

        public static HtmlNode FindRowByStyle(HtmlDocument htmlDoc, string style)
        {
            return htmlDoc.DocumentNode.SelectSingleNode($"//tr[@style='{style}']");
        }
        public static HtmlNode FindRowById(HtmlDocument htmlDoc, string id)
        {
            return htmlDoc.DocumentNode.SelectSingleNode($"//div[@id='{id}']");
        }

        public static string RemoveTableRow(string htmlContent, int rowIndexToRemove)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Select the table
            var tableNode = doc.DocumentNode.SelectSingleNode("//table");

            if (tableNode != null)
            {
                // Select all rows in the table
                var rows = tableNode.SelectNodes(".//tr");

                if (rows != null && rowIndexToRemove >= 0 && rowIndexToRemove < rows.Count)
                {
                    // Remove the specified row
                    rows[rowIndexToRemove].Remove();
                }
            }

            // Get the modified HTML content
            return doc.DocumentNode.OuterHtml;
        }

        public static string TrimLeadingZeros(string input)
        {
            if (!string.IsNullOrEmpty(input) && input != "0")
            {
                input = input.TrimStart('0');

                if (input == "")
                {
                    input = "0";
                }
            }

            return input;
        }
    }
}

