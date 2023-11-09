using Quartz;
using System.Web.Mvc;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.BLL.Jobs
{
    public class BalanceLowSheduleJob : IJob
    {
        public partial class UserScheduleDTO
        {
            public int Id { get; set; }
            public long UserId { get; set; }
            public int ScheduleType { get; set; }
            public int Status { get; set; }
            public System.DateTime CreatedAt { get; set; }
            public string Balance { get; set; }
        }
        public void Execute(IJobExecutionContext context)
        {
            var _errorManager = DependencyResolver.Current.GetService<IErrorLogManager>();
            var _posManager = DependencyResolver.Current.GetService<IPOSManager>();
            try
            {
                var lowOnBalance = _posManager.GetAllUserRunningLow();
                for (int i = 0; i < lowOnBalance.Count; i++)
                {
                    if (_posManager.BalanceLowMessageIsSent(lowOnBalance[i].UserId, UserScheduleTypes.LowBalnce))
                        continue;
                    else if (!_posManager.BalanceLowScheduleExist(lowOnBalance[i].UserId, UserScheduleTypes.LowBalnce))
                    {
                        try
                        {
                            _posManager.SaveUserSchedule(lowOnBalance[i].UserId, lowOnBalance[i].Balance);
                        }
                        catch (System.Exception ex)
                        {
                            _errorManager.LogExceptionToDatabase(new System.Exception("balancelow checking to save", ex));
                        }
                    }
                    else
                        continue;
                }
            }
            catch (System.Exception ex)
            {
                _errorManager.LogExceptionToDatabase(new System.Exception("balancelow checking to fetch", ex));
            }


            var lowOnBalanceSchedule = _posManager.GetUserSchedule();
            for (int i = 0; i < lowOnBalanceSchedule.Count; i++)
            {
                if (_posManager.IsWalletFunded(lowOnBalanceSchedule[i].UserId))
                {
                    try
                    {
                        _posManager.RemoveFromSchedule(lowOnBalanceSchedule[i].UserId);
                        continue;
                    }
                    catch (System.Exception ex)
                    {
                        _errorManager.LogExceptionToDatabase(new System.Exception("balancelow checking to remove", ex));
                    }
                }
                else if (lowOnBalanceSchedule[i].Status == (int)UserScheduleStatus.NotSent)
                {
                    try
                    {
                        var _emailManager = DependencyResolver.Current.GetService<IEmailTemplateManager>();
                        var emailTemplate = _emailManager.GetEmailTemplateByTemplateType(TemplateTypes.BalanceLowReminder);
                        if (emailTemplate.TemplateStatus)
                        {
                            _posManager.UpdateUserSchedule(lowOnBalanceSchedule[i].UserId, UserScheduleStatus.Pending);
                            var _userManager = DependencyResolver.Current.GetService<IUserManager>();
                            var userAcount = _userManager.GetUserDetailsByUserId(lowOnBalanceSchedule[i].UserId);
                            string body = emailTemplate.TemplateContent;
                            body = body.Replace("%customer%", userAcount.Vendor);
                            Utilities.SendEmail("favouremmanuel433@gmail.com", emailTemplate.EmailSubject, body);
                            Utilities.SendEmail(userAcount.Email, emailTemplate.EmailSubject, body);
                            _posManager.UpdateUserSchedule(lowOnBalanceSchedule[i].UserId, UserScheduleStatus.Sent);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _errorManager.LogExceptionToDatabase(new System.Exception("balancelow checking to send", ex));
                    }
                }



            }





        }
    }
}
