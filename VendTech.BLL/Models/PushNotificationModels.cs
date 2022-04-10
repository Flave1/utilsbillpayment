using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class PushNotificationModel
    {
        public string DeviceToken { get; set; }
        public int DeviceType { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public NotificationTypeEnum NotificationType { get; set; }
        public long UserId { get; set; }
        public long Id { get; set; }
    }
    public class NotificationApiListingModel
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public string UserName { get; set; }
        public string SentOn { get; set; }
        public long Id { get; set; }
        public NotificationApiListingModel(Notification obj)
        {
            Message = obj.Text;
            UserName = obj.User.Name + " " + obj.User.SurName;
            Type = obj.Type.Value;
            SentOn = obj.SentOn.ToString();
            Title = obj.Title;
            Id = obj.RowId==null?0:obj.RowId.Value;
        }
    }
}
