using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Common
{
    public static class PushNotification
    {
        public static string SendNotification(PushNotificationModel model)
        {
            try
            {
                string GoogleAppID = Config.GetWebApiKey;
                var SENDER_ID = Config.GetSenderId;

                WebRequest tRequest;
                tRequest = WebRequest.Create(Config.FCMUrl);
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

                tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            

                if (model.DeviceType == (int)AppTypeEnum.IOS)
                {
                    var payload = new
                    {
                        to = model.DeviceToken,
                        priority = "high",
                        content_available = true,
                        notification = new
                        {
                            title = model.Title,
                            body = model.Message,
                            type =(int) model.NotificationType,
                            sound = "default",
                            id=model.Id

                        }
                    };
                    string postData = JsonConvert.SerializeObject(payload).ToString();

                    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    tRequest.ContentLength = byteArray.Length;

                    Stream dataStream = tRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse tResponse = tRequest.GetResponse();

                    dataStream = tResponse.GetResponseStream();

                    StreamReader tReader = new StreamReader(dataStream);

                    String sResponseFromServer = tReader.ReadToEnd();

                    tReader.Close();
                    dataStream.Close();
                    tResponse.Close();
                    SaveNotificationToDB(model, (int)NotificationStatusEnum.Success);
                    return sResponseFromServer;
                }
                else
                {
                    var payload = new
                    {
                        to = model.DeviceToken,
                        priority = "high",
                        content_available = true,
                        data = new
                        {
                            title = model.Title,
                            body = model.Message,
                            id=model.Id,
                            type=(int)model.NotificationType
                        }
                    };
                    string postData = JsonConvert.SerializeObject(payload).ToString();

                    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    tRequest.ContentLength = byteArray.Length;

                    Stream dataStream = tRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse tResponse = tRequest.GetResponse();

                    dataStream = tResponse.GetResponseStream();

                    StreamReader tReader = new StreamReader(dataStream);
                    String sResponseFromServer = tReader.ReadToEnd();

                    tReader.Close();
                    dataStream.Close();
                    tResponse.Close();
                    SaveNotificationToDB(model, (int)NotificationStatusEnum.Success);
                    return sResponseFromServer;
                }

            }
            catch (Exception ex)
            {
                SaveNotificationToDB(model, (int)NotificationStatusEnum.Failed);
                return ex.Message;
            }
        }

        public static bool SaveNotificationToDB(PushNotificationModel model, int status)
        {
            var db = new VendtechEntities();
            var dbNotification = new Notification();
            dbNotification.SentOn = DateTime.UtcNow;
            dbNotification.UserId = model.UserId;
            dbNotification.Title = model.Title;
            dbNotification.Text = model.Message;
            dbNotification.Type = (int)model.NotificationType;
            dbNotification.Status = status;
            dbNotification.RowId = model.Id;
            db.Notifications.Add(dbNotification);
            db.SaveChanges();
            return true;
        }
    }
}
