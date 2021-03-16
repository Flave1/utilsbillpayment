using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class ApplicationNotUsedSchedulerJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            using (var db = new VendTechEntities())
            {
                try
                {
                    int minutes = 3;
                    var record = db.AppSettings.FirstOrDefault(p => p.Name == AppSettings.LogoutTime);
                    if (record != null)
                        minutes = Convert.ToInt32(record.Value);
                    var logOutTime = DateTime.UtcNow.AddMinutes(-minutes);
                    var logOutUsers = db.Users.Where(p => p.AppLastUsed != null && logOutTime > p.AppLastUsed.Value).ToList();
                    foreach (var user in logOutUsers)
                    {
                        db.TokensManagers.RemoveRange(user.TokensManagers.ToList());
                        db.SaveChanges();
                    }
                }
                catch (Exception) { }
                
            }
        }
    }
}
