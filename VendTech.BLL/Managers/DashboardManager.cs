using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class DashboardManager :  BaseManager, IDashboardManager
    {
        private IUserManager _userManager = new UserManager();


        
        public decimal GetDepositTotal()
        {
            throw new NotImplementedException();
        }

        public decimal GetSalesTotal()
        {
            throw new NotImplementedException();
        }

        public int GetPOSCount()
        {
            throw new NotImplementedException();
        }

        public decimal GetWalletBalance()
        {
            throw new NotImplementedException();
        }

        public List<TransactionChartData> getChartDataByAcquirer(string frequency, long AcquirerId)
        {
            List<TransactionChartData> transactionChartData = new List<TransactionChartData>();
            try
            {
               // using (_db = new Context)
                {
                    var acquirerTerminals = Context.Users.Where(a => a.UserId == AcquirerId).FirstOrDefault();
                    IEnumerable<long> ids2 =(IEnumerable<long>) acquirerTerminals.POS.Select(x => x.POSId).Distinct().ToList();
                   // var ids = acquirerTerminals.Terminals; // Select(x => x.TerminalRef).Distinct();

                    if(acquirerTerminals != null && ids2.Count() >0)
                    {
                        var data = Context.Deposits.Where(d => d.UserId == AcquirerId && d.IsDeleted == false).Join(Context.TransactionDetails, d =>d.UserId, m=>m.UserId,(d,m)=> new { 
                            UserId = d.UserId,DepositAmount = d.Amount, DepositPOSId= d.POSId,RechargeAmount = m.Amount,RechargePOSId = m.POSId,RechargeCreatedAt= m.CreatedAt
                        }).Where(t => ids2.Any(a => a == t.RechargePOSId))
                        .Where(m => m.RechargeCreatedAt != null).Where(m => m.RechargeCreatedAt.Year == DateTime.Now.Year).GroupBy(x => new { mdate = x.RechargeCreatedAt.Month })
                        .Select(x => new TransactionChartData
                        {
                            mdate = x.Key.mdate.ToString(),
                            deposit = x.Sum(y => y.DepositAmount).ToString(),
                            billpayment = x.Sum(y => y.RechargeAmount).ToString(),

                        }).DefaultIfEmpty().ToList();
                           if (data.Any() && data[0] != null)
                        {
                            return transactionChartData = data.Where(q => q.mdate != "").ToList();
                        }
                    

                    /*var data = Context.MeterRecharges.Where(t => ids2.Any(a=>a==t.POSId))
                        .Where(m => m.CreatedAt != null).Where(m => m.CreatedAt.Year == DateTime.Now.Year).GroupBy(x => new { mdate = x.CreatedAt.Month })
                        .Select(x => new TransactionChartData
                        {
                            mdate = x.Key.mdate.ToString(),
                            deposit = x.Sum(y => y.Amount).ToString(),
                            billpayment = x.Sum(y => y.Amount).ToString(),

                        }).DefaultIfEmpty();
                        if (data != null)
                        {
                            return transactionChartData = data.Where(q => q.mdate != "").ToList();
                        }*/
                    }
                    

                }
                return transactionChartData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }


        public List<TransactionChartData> getChartDataByAdmin(string frequency)
        {
            List<TransactionChartData> transactionChartData = new List<TransactionChartData>();
            try
            {
                // using (_db = new Context)
                {
                   // var acquirerTerminals = Context.Users.Where(a => a.UserId == AcquirerId).FirstOrDefault();
                    //IEnumerable<long> ids2 = (IEnumerable<long>)acquirerTerminals.POS.Select(x => x.POSId).Distinct();
                    // var ids = acquirerTerminals.Terminals; // Select(x => x.TerminalRef).Distinct();

                   // if (acquirerTerminals != null && ids2.Count() > 0)
                    {
                        var data = Context.Deposits.Where(p => p.IsDeleted == false).Join(Context.TransactionDetails, d => d.UserId, m => m.UserId, (d, m) => new {
                            UserId = d.UserId,
                            DepositAmount = d.Amount,
                            DepositPOSId = d.POSId,
                            RechargeAmount = m.Amount,
                            RechargePOSId = m.POSId,
                            RechargeCreatedAt = m.CreatedAt
                        })
                        .Where(m => m.RechargeCreatedAt != null).Where(m => m.RechargeCreatedAt.Year == DateTime.Now.Year).GroupBy(x => new { mdate = x.RechargeCreatedAt.Month })
                        .Select(x => new TransactionChartData
                        {
                            mdate = x.Key.mdate.ToString(),
                            deposit = x.Sum(y => y.DepositAmount).ToString(),
                            billpayment = x.Sum(y => y.RechargeAmount).ToString(),

                        }).DefaultIfEmpty().ToList();
                        if (data.Any() && data[0] != null)
                        {
                            return transactionChartData = data.Where(q => q.mdate != "").ToList();
                        }
                    }
                }
                return transactionChartData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }


        public DashboardViewModel getDashboardData(long userId, long agentId)
        {
            try
            {
                if (userId == 0) return new DashboardViewModel();
                var user = Context.Users.Find(userId);
                if(user == null) return new DashboardViewModel();
                List<TransactionChartData> tDatas = new List<TransactionChartData>();
                var total_deposits = new decimal();
                var total_sales = new decimal();

                var current_user_pos_ids = Context.POS.Where(p => !p.IsDeleted && p.User.UserId == userId).Select(e => e.POSId).ToList();

                total_deposits = Context.Deposits.ToList().Where(e => current_user_pos_ids.Contains(e.POSId) && e.IsDeleted == false && e.CreatedAt.Date == DateTime.UtcNow.Date && e.Status == (int)DepositPaymentStatusEnum.Released).Sum(s => s.Amount);
                total_sales = Context.TransactionDetails.ToList().Where(e => current_user_pos_ids.Contains(e.POSId ?? 0) && e.CreatedAt.Date == DateTime.UtcNow.Date && e.Status == (int)RechargeMeterStatusEnum.Success).Sum(s => s.Amount);

                if (user.UserRole.Role == UserRoles.AppUser || user.UserRole.Role == UserRoles.Vendor) //user.UserRole.Role == UserRoles.Vendor ||
                {
                    tDatas = getChartDataByAcquirer("", userId).OrderByDescending(a => a?.mdate).ToList();
                    if (tDatas.Any())
                    {
                        for (int x = 0; x < tDatas.Count; x++)
                        {
                            tDatas[x].mdate = DateTime.Now.Year.ToString() + "-" + tDatas[x].mdate;//getAbbreviatedName(Int16.Parse(tDatas[x].mdate));
                        }
                    }

                    return new DashboardViewModel
                    {
                        totalSales = total_sales,
                        totalDeposit = total_deposits,
                        posCount = agentId > 0 ? Context.POS.Where(p => !p.IsDeleted && p.Enabled == true && p.User.AgentId == agentId).ToList().Count:  user.POS.Where(p => !p.IsDeleted && p.Enabled == true).ToList().Count,
                        walletBalance = _userManager.GetUserWalletBalance(userId),
                        transactionChartData = tDatas
                    };

                }
                else if (user.UserRole.Role != UserRoles.AppUser)
                {
                    total_deposits = new decimal();
                    total_sales = new decimal();
                    var date = DateTime.UtcNow.Date;
                    total_deposits = Context.Deposits.ToList().Where(d => d.CreatedAt.Date == DateTime.UtcNow.Date && d.Status == (int)DepositPaymentStatusEnum.Released && d.IsDeleted == false).Sum(s => s.Amount);
                    total_sales = Context.TransactionDetails.ToList().Where(d => d.CreatedAt.Date == DateTime.UtcNow.Date && d.Status == (int)RechargeMeterStatusEnum.Success).Sum(s => s.Amount);

                    tDatas = getChartDataByAdmin("").OrderByDescending(a => a?.mdate).ToList();
                    if (tDatas.Any())
                    {
                        for (int x = 0; x < tDatas.Count; x++)
                        {
                            tDatas[x].mdate = DateTime.Now.Year.ToString() + "-" + tDatas[x].mdate;//getAbbreviatedName(Int16.Parse(tDatas[x].mdate));
                        }
                    }
                    return new DashboardViewModel
                    {
                        totalSales = total_sales,
                        totalDeposit = total_deposits,
                        userCount = Context.Users.Where(u => u.UserRole.Role == UserRoles.AppUser || u.UserRole.Role == UserRoles.Vendor && u.Status == (int)UserStatusEnum.Active).Count(),
                        posCount = Context.POS.Where(p => !p.IsDeleted && p.Enabled == true).Count(),
                        walletBalance = _userManager.GetUserWalletBalance(userId),
                        transactionChartData = tDatas
                    };
                }
                else
                {

                    return new DashboardViewModel
                    {
                        totalSales = total_sales,
                        totalDeposit = total_deposits,
                        posCount = user.POS.Where(p => !p.IsDeleted).ToList().Count,
                        walletBalance = _userManager.GetUserWalletBalance(userId),
                        transactionChartData = tDatas
                    };
                }
            }
            catch (Exception e)
            { 
                return new DashboardViewModel();
            }
          
             
            
        }
    }
}
