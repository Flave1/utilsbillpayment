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
                    IEnumerable<long> ids2 =(IEnumerable<long>) acquirerTerminals.POS.Select(x => x.POSId).Distinct();
                   // var ids = acquirerTerminals.Terminals; // Select(x => x.TerminalRef).Distinct();

                    if(acquirerTerminals != null && ids2.Count() >0)
                    {
                        var data = Context.Deposits.Where(d => d.UserId == AcquirerId).Join(Context.MeterRecharges, d =>d.UserId, m=>m.UserId,(d,m)=> new { 
                            UserId = d.UserId,DepositAmount = d.Amount, DepositPOSId= d.POSId,RechargeAmount = m.Amount,RechargePOSId = m.POSId,RechargeCreatedAt= m.CreatedAt
                        }).Where(t => ids2.Any(a => a == t.RechargePOSId))
                        .Where(m => m.RechargeCreatedAt != null).Where(m => m.RechargeCreatedAt.Year == DateTime.Now.Year).GroupBy(x => new { mdate = x.RechargeCreatedAt.Month })
                        .Select(x => new TransactionChartData
                        {
                            mdate = x.Key.mdate.ToString(),
                            deposit = x.Sum(y => y.DepositAmount).ToString(),
                            billpayment = x.Sum(y => y.RechargeAmount).ToString(),

                        }).DefaultIfEmpty();
                        if (data != null)
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


        public DashboardViewModel getDashboardData(long userId)
        {
            var user = Context.Users.Find(userId);
            var data = (from u in Context.Users join d in Context.Deposits  on u.UserId equals d.UserId
                        join s in Context.MeterRecharges on d.UserId equals s.UserId  where u.UserId == userId  select new 
                        { 
                            totalSales = s.Amount,
                            totalDeposit = d.Amount,
                           
                        } ).DefaultIfEmpty();

            List<TransactionChartData> tDatas = new List<TransactionChartData>();
            tDatas = getChartDataByAcquirer("", userId).OrderByDescending(a=>a.mdate).ToList();
            if (tDatas != null)
            {
                for (int x = 0; x < tDatas.Count; x++)
                {
                    tDatas[x].mdate = DateTime.Now.Year.ToString() + "-" + tDatas[x].mdate;//getAbbreviatedName(Int16.Parse(tDatas[x].mdate));
                }
            }
            if(data.FirstOrDefault() != null)
            {
                return new DashboardViewModel
                {
                    totalSales = data.Sum(d => d.totalSales),
                    totalDeposit = data.Sum(d => d.totalDeposit),
                    posCount = user.POS.Count,
                    walletBalance = _userManager.GetUserWalletBalance(userId),
                   transactionChartData =   tDatas
                };
            }
            else
            {
                return new DashboardViewModel
                {
                    totalSales = 0,
                    totalDeposit = 0,
                    posCount = user.POS.Count,
                    walletBalance = _userManager.GetUserWalletBalance(userId),
                    transactionChartData = tDatas
                };
            }
            
        }
    }
}
