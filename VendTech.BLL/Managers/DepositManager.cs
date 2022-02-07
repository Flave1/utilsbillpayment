using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class DepositManager : BaseManager, IDepositManager
    {

        string IDepositManager.GetWelcomeMessage()
        {
            return "Welcome To Base Project Demo";
        }

        PagingResult<DepositListingModel> IDepositManager.GetAllPendingDepositPagedList(PagingModel model, bool getForRelease, long vendorId, string status)
        {
            var result = new PagingResult<DepositListingModel>();

            // model.RecordsPerPage = 2;
            IQueryable<PendingDeposit> query = Context.PendingDeposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending
            && p.POS.Enabled != false && p.IsDeleted == false).OrderBy(model.SortBy + " " + model.SortOrder);
            {
                if (vendorId == 0)
                {
                    //var vendor = Context.POS.Where(p => p.VendorId != null && !p.IsDeleted).FirstOrDefault();
                    //if (vendor != null)
                    //    vendorId = vendor.POSId;
                    // this is New
                    //query = query.Where(p=>p.POSId>0 && !p.POS.IsDeleted);
                    // model.PageNo = 1;
                    //model.RecordsPerPage = 2;
                }
                else
                {
                    query = query.Where(p => p.POSId == vendorId);
                }

            }
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()) || z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.Amount.ToString().Contains(model.Search) || ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
                //if (model.SearchField.Equals("VENDOR"))
                //    query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("USER"))
                    query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("POS"))
                    query = query.Where(z => z.POS.SerialNumber.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("PAYMENT"))
                    query = query.Where(z => ((DepositPaymentTypeEnum)z.PaymentType).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("CHEQUE"))
                    query = query.Where(z => z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("AMOUNT"))
                    query = query.Where(z => z.Amount.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("%"))
                    query = query.Where(z => z.PercentageAmount != null && z.PercentageAmount.Value.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("STATUS"))
                    query = query.Where(z => ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
            }
            else if (!string.IsNullOrEmpty(status))
                query = query.Where(z => ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(status.ToLower()));

            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new DepositListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposits List";
            result.TotalCount = query.Count();
            return result;
        }
        PagingResult<DepositListingModel> IDepositManager.GetDepositPagedList(PagingModel model, bool getForRelease, long vendorId, string status)
        {
            var result = new PagingResult<DepositListingModel>();

            // model.RecordsPerPage = 2;
            IQueryable<Deposit> query = Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending
            && p.POS.Enabled != false && p.IsDeleted == false).OrderBy(model.SortBy + " " + model.SortOrder);
            {
                if (vendorId == 0)
                {
                    //var vendor = Context.POS.Where(p => p.VendorId != null && !p.IsDeleted).FirstOrDefault();
                    //if (vendor != null)
                    //    vendorId = vendor.POSId;
                    // this is New
                    //query = query.Where(p=>p.POSId>0 && !p.POS.IsDeleted);
                    // model.PageNo = 1;
                    //model.RecordsPerPage = 2;
                }
                else
                {
                    query = query.Where(p => p.POSId == vendorId);
                }

            }
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()) || z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.Amount.ToString().Contains(model.Search) || ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
                //if (model.SearchField.Equals("VENDOR"))
                //    query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("USER"))
                    query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("POS"))
                    query = query.Where(z => z.POS.SerialNumber.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("PAYMENT"))
                    query = query.Where(z => ((DepositPaymentTypeEnum)z.PaymentType).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("CHEQUE"))
                    query = query.Where(z => z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("AMOUNT"))
                    query = query.Where(z => z.Amount.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("%"))
                    query = query.Where(z => z.PercentageAmount != null && z.PercentageAmount.Value.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("STATUS"))
                    query = query.Where(z => ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
            }
            else if (!string.IsNullOrEmpty(status))
                query = query.Where(z => ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(status.ToLower()));

            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new DepositListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposits List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositListingModel> IDepositManager.GetReleasedDepositPagedList(PagingModel model, bool getForRelease, long vendorId)
        {
            var result = new PagingResult<DepositListingModel>();

            // model.RecordsPerPage = 2;
            IQueryable<Deposit> query = Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Released || p.Status == (int)DepositPaymentStatusEnum.Reversed && p.POS.Enabled != false
            && p.IsDeleted == false).OrderBy(model.SortBy + " " + model.SortOrder);
            if (!getForRelease)
            {
                if (vendorId == 0)
                {
                    //var vendor = Context.POS.Where(p => p.VendorId != null && !p.IsDeleted).FirstOrDefault();
                    //if (vendor != null)
                    //    vendorId = vendor.POSId;

                    // this is New
                    //query = query.Where(p=>p.POSId>0 && !p.POS.IsDeleted);
                    // model.PageNo = 1;
                    //model.RecordsPerPage = 2;
                }
                else
                {
                    query = query.Where(p => p.POSId == vendorId);
                }

            }
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()) || z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.Amount.ToString().Contains(model.Search) || ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
                //if (model.SearchField.Equals("VENDOR"))
                //    query = query.Where(z => z.User1.Name.ToLower().Contains(model.Search.ToLower()) || z.User1.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("USER"))
                    query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("POS"))
                    query = query.Where(z => z.POS.SerialNumber.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("PAYMENT"))
                    query = query.Where(z => ((DepositPaymentTypeEnum)z.PaymentType).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("CHEQUE"))
                    query = query.Where(z => z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()) || z.CheckNumberOrSlipId.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("AMOUNT"))
                    query = query.Where(z => z.Amount.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("%"))
                    query = query.Where(z => z.PercentageAmount != null && z.PercentageAmount.Value.ToString().Contains(model.Search));
                else if (model.SearchField.Equals("STATUS"))
                    query = query.Where(z => ((DepositPaymentStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
            }
            var list = query.Skip(model.PageNo - 1).Take(model.RecordsPerPage).OrderBy(d => d.Status).ToList().Select(x => new DepositListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposits List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositListingModel> IDepositManager.GetUserDepositList(int pageNo, int pageSize, long userId)
        {
            var result = new PagingResult<DepositListingModel>();
            var query = Context.Deposits.Where(p => p.UserId == userId && p.IsDeleted == false).OrderByDescending(p => p.CreatedAt);
            result.TotalCount = query.Count();
            var list = query
               .Skip((pageNo - 1) * pageSize).Take(pageSize)
               .ToList().Select(x => new DepositListingModel(x, true)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositLogListingModel> IDepositManager.GetDepositLogsPagedList(PagingModel model)
        {
            var result = new PagingResult<DepositLogListingModel>();
            var query = Context.DepositLogs.OrderBy(model.SortBy + " " + model.SortOrder);
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()) || z.Deposit.User.Name.ToLower().Contains(model.Search.ToLower()) || z.Deposit.User.SurName.ToLower().Contains(model.Search.ToLower()) || z.Deposit.Amount.ToString().Contains(model.Search) || ((DepositPaymentStatusEnum)z.PreviousStatus).ToString().ToLower().Contains(model.Search.ToLower()) || ((DepositPaymentStatusEnum)z.NewStatus).ToString().ToLower().Contains(model.Search.ToLower()));

                if (model.SearchField.Equals("USER"))
                    query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()) || z.User.SurName.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("REQUESTED"))
                    query = query.Where(z => z.Deposit.User.Name.ToLower().Contains(model.Search.ToLower()) || z.Deposit.User.SurName.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("AMOUNT"))
                    query = query.Where(z => z.Deposit.Amount.ToString().Contains(model.Search));

                else if (model.SearchField.Equals("OLD"))
                    query = query.Where(z => ((DepositPaymentStatusEnum)z.PreviousStatus).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("NEW"))
                    query = query.Where(z => ((DepositPaymentStatusEnum)z.NewStatus).ToString().ToLower().Contains(model.Search.ToLower()));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new DepositLogListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositListingModel> IDepositManager.GetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 10000000;
            IQueryable<DepositLog> query = null;
            var result = new PagingResult<DepositListingModel>();

            if (!model.IsInitialLoad)
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released || p.NewStatus == (int)DepositPaymentStatusEnum.Reversed);
            else
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt)
                    .Where(p => (p.NewStatus == (int)DepositPaymentStatusEnum.Released
                    || p.NewStatus == (int)DepositPaymentStatusEnum.Reversed)
                    && DbFunctions.TruncateTime(p.Deposit.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));
            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            //if (!string.IsNullOrEmpty(model.Meter))
            //{
            //    query = query.Where(p => p.Deposit.m);
            //}

            var totalrecoed = query.ToList().Count();
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "TransactionId" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                // query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                if (model.SortBy == "CreatedAt")
                {
                    if (model.SortOrder == "Desc")
                    {
                        query = query.OrderByDescending(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                    else
                    {
                        query = query.OrderBy(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                }
                else
                {
                    query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                }
            }
            var list = query.ToList().Select(x => new DepositListingModel(x.Deposit)).ToList();
            if (model.SortBy == "CreatedAt" || model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
                if (model.SortBy == "UserName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "TransactionId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PercentageAmount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "BANK")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Bank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Bank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "Status")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "NewBalance")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.NewBalance).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.NewBalance).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }
            result.List = list;


            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = totalrecoed;
            return result;

            //var result = new PagingResult<DepositListingModel>();
            //var query = new List<DepositLog>(); //Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released).ToList();


            //if (model.VendorId.HasValue && model.VendorId > 0)
            //{
            //    var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
            //    var posIds = new List<long>();
            //    if (callFromAdmin)
            //    {
            //        query = Context.DepositLogs.Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released).ToList();
            //        posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
            //    } 
            //    else
            //    {
            //        posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
            //        query = Context.DepositLogs.Where(p => posIds.Contains(p.Deposit.POSId) && p.NewStatus == (int)DepositPaymentStatusEnum.Released).OrderByDescending(p => p.Deposit.CreatedAt).ToList();
            //    } 
            //}
            //else
            //{
            //    query = Context.DepositLogs.Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released).ToList();
            //}

            //if (model.From != null) 
            //    query = query.Where(p => p.Deposit.CreatedAt.Date >= model.From.Value.Date).ToList();  
            //if (model.To != null) 
            //    query = query.Where(p => p.Deposit.CreatedAt.Date <= model.To.Value.Date).ToList();  
            //if (model.PosId.HasValue && model.PosId > 0) 
            //    query = query.Where(p => p.Deposit.POSId == model.PosId).ToList(); 
            //if (model.Bank.HasValue && model.Bank > 0) 
            //    query = query.Where(p => p.Deposit.BankAccountId == model.Bank).ToList(); 
            //if (model.DepositType.HasValue && model.DepositType > 0) 
            //    query = query.Where(p => p.Deposit.PaymentType == model.DepositType).ToList(); 
            //if (!string.IsNullOrEmpty(model.RefNumber)) 
            //    query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower())).ToList(); 
            //if (!string.IsNullOrEmpty(model.TransactionId)) 
            //    query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower())).ToList();


            //var ordered = query.Select(x => new DepositListingModel(x.Deposit)).ToList();

            //result.List = (from a in ordered    
            //               orderby a.CreatedAt
            //               descending
            //               select a).ToList(); 

            //result.Status = ActionStatus.Successfull;
            //result.Message = "Deposit Logs List";
            //result.TotalCount = query.Count();
            //return result;
        }

        PagingResult<AgentRevenueListingModel> IDepositManager.GetAgentRevenueReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 10000000;
            IQueryable<DepositLog> query = null;
            var result = new PagingResult<AgentRevenueListingModel>();

            if (!model.IsInitialLoad)
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released || p.NewStatus == (int)DepositPaymentStatusEnum.Reversed);
            else
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt)
                    .Where(p => (p.NewStatus == (int)DepositPaymentStatusEnum.Released
                    || p.NewStatus == (int)DepositPaymentStatusEnum.Reversed)
                    && DbFunctions.TruncateTime(p.Deposit.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));
            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }

            if(model.AgencyId.HasValue && model.AgencyId > 0)
            {
                query = query.Where(p => p.Deposit.User.AgentId == model.AgencyId);
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            //if (!string.IsNullOrEmpty(model.Meter))
            //{
            //    query = query.Where(p => p.Deposit.m);
            //}

            var totalrecoed = query.ToList().Count();
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "TransactionId" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                // query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                if (model.SortBy == "CreatedAt")
                {
                    if (model.SortOrder == "Desc")
                    {
                        query = query.OrderByDescending(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                    else
                    {
                        query = query.OrderBy(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                }
                else
                {
                    query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                }
            }
            var list = query.ToList().Select(x => new AgentRevenueListingModel(x.Deposit)).ToList();
            if (model.SortBy == "CreatedAt" || model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
                if (model.SortBy == "UserName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }  
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }
            result.List = list;


            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = totalrecoed;
            return result;
 
        }


        PagingResult<DepositAuditModel> IDepositManager.GetAuditReportsPagedList(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<DepositAuditModel>();
            var query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));
            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.Amount))
            {
                var amt = Convert.ToDecimal(model.Amount.ToLower().Replace(",", "")+".00");
                query = query.Where(p => p.Deposit.Amount == amt);
            }
            if (!string.IsNullOrEmpty(model.IssuingBank))
            {
                query = query.Where(p => p.Deposit.ChequeBankName.ToLower().Contains(model.IssuingBank.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.Payer))
            {
                query = query.Where(p => p.Deposit.NameOnCheque.ToLower().Contains(model.Payer.ToLower()));
            }
            if (model.IsAudit)
            {
                query = query.Where(p => p.Deposit.isAudit == true);
            }
            if (!model.IsAudit && string.IsNullOrEmpty(model.Status))
            {
                query = query.Where(p => p.Deposit.isAudit == false);
            }

            var totalrecoed = query.ToList().Count();
            if (model.SortBy != "DEPOSITBY" && model.SortBy != "AMOUNT" || model.SortBy != "POS" && model.SortBy != "GTBANK" && model.SortBy != "DEPOSITREF" && model.SortBy != "PAYER" && model.SortBy != "ISSUINGBANK")
            {
                // query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                if (model.SortBy == "CreatedAt")
                {
                    if (model.SortOrder == "Desc")
                    {
                        query = query.OrderByDescending(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                    else
                    {
                        query = query.OrderBy(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                }
            }
            var list = query
           .ToList().Select(x => new DepositAuditModel(x.Deposit)).ToList();

            if (model.SortBy == "CREATEDAT" || model.SortBy == "DEPOSITBY" || model.SortBy == "AMOUNT" || model.SortBy == "POS" || model.SortBy == "GTBANK" || model.SortBy == "DEPOSITREF" || model.SortBy == "PAYER" || model.SortBy == "ISSUINGBANK" || model.SortBy.ToUpper() == "DEPOSITTYPE")
            {
                if (model.SortBy == "CREATEDAT")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "DEPOSITBY")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.DepositBy).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.DepositBy).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "GTBANK")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "DEPOSITREF")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.DepositRef).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.DepositRef).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "AMOUNT")
                {

                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "POS")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.PosId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.PosId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                else if (model.SortBy == "PAYER")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Payer).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.Payer).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy == "ISSUINGBANK")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.IssuingBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.IssuingBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "DEPOSITTYPE")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
            }
            result.List = list;


            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = totalrecoed;
            return result;
        }

        PagingResult<DepositAuditModel> IDepositManager.GetDepositAuditReports(ReportSearchModel model, bool callFromAdmin = false)
        {
            var result = new PagingResult<DepositAuditModel>();
            var query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);

            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }
            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            if (!model.IsAudit && string.IsNullOrEmpty(model.Status))
                query = query.Where(p => p.Deposit.isAudit == false);
            if (model.IsAudit)
                query = query.Where(p => p.Deposit.isAudit == true);

            var totalrecord = query.ToList().Count();
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                 query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1));

                //query = model.SortBy == "CreatedAt" ?
                //    (model.SortOrder == "Desc" ?
                //    query.OrderByDescending(p => p.Deposit.UpdatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage) :
                //    query.OrderBy(p => p.Deposit.UpdatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage)) :
                //    query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            }

            var list = query
               .ToList().Select(x => new DepositAuditModel(x.Deposit)).ToList();
            if (model.SortBy.ToUpper() == "DEPOSITBY" || model.SortBy.ToUpper() == "AMOUNT" || model.SortBy.ToUpper() == "POS" || model.SortBy.ToUpper() == "PAYMENTTYPE" || model.SortBy.ToUpper() == "GTBANK" || model.SortBy.ToUpper() == "DEPOSITREF" || model.SortBy.ToUpper() == "STATUS" || model.SortBy.ToUpper() == "GTBANK" || model.SortBy.ToUpper() == "PAYER" || model.SortBy.ToUpper() == "ISSUINGBANK" || model.SortBy.ToUpper() == "DEPOSITTYPE")
            {
                if (model.SortBy == "CREATEDAT")
                {
                    list = model.SortOrder == "Asc" ? list.OrderBy(p => p.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy.ToUpper() == "DEPOSITBY")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.DepositBy).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.DepositBy).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "PAYMENTTYPE")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "GTBANK")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "DEPOSITREF")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.DepositRef).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.DepositRef).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "STATUS")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "AMOUNT")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                    list.OrderByDescending(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "POS")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.PosId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.PosId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "GTBANK")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.GTBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "PAYER")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Payer).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.Payer).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "ISSUINGBANK")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.IssuingBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.IssuingBank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                else if (model.SortBy.ToUpper() == "DEPOSITTYPE")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
            }
            result.List = list;


            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Audit Logs List";
            result.TotalCount = totalrecord;
            return result;
        }
        PagingResult<DepositExcelReportModel> IDepositManager.GetReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<DepositExcelReportModel>();
            var query = Context.DepositLogs.Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }
            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            //if (!string.IsNullOrEmpty(model.Meter))
            //{
            //    query = query.Where(p => p.Deposit.m);
            //}
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            }



            var list = query
               .ToList().Select(x => new DepositExcelReportModel(x.Deposit)).ToList();
            if (model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
                if (model.SortBy == "UserName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.USERNAME).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.USERNAME).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PercentageAmount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PERCENT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PERCENT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_TYPE).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_TYPE).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "BANK")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.BANK).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.BANK).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_REF_NO).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_REF_NO).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }

                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.AMOUNT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.AMOUNT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }

             
            // if data will not available so pass blank single data
            if (list.Count == 0)
            {
                var testdata = new DepositExcelReportModel();
                list = new List<DepositExcelReportModel>();
                list.Add(testdata);
            }

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<AgencyRevenueExcelReportModel> IDepositManager.GetAgentRevenueReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<AgencyRevenueExcelReportModel>();
            var query = Context.DepositLogs.Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }
            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            //if (!string.IsNullOrEmpty(model.Meter))
            //{
            //    query = query.Where(p => p.Deposit.m);
            //}
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            }



            var list = query
               .ToList().Select(x => new AgencyRevenueExcelReportModel(x.Deposit)).ToList();
            if (model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
              
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_TYPE).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_TYPE).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                } 
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_REF_NO).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_REF_NO).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }

                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.AMOUNT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.AMOUNT).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }


            // if data will not available so pass blank single data
            if (list.Count == 0)
            {
                var testdata = new AgencyRevenueExcelReportModel();
                list = new List<AgencyRevenueExcelReportModel>();
                list.Add(testdata);
            }

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositListingModel> IDepositManager.GetReportsPagedHistoryList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            var result = new PagingResult<DepositListingModel>();

            var query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);

            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));
            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.Deposit.POSId));
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }

            var totalrecoed = query.ToList().Count();
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                if (model.SortBy == "CreatedAt")
                {
                    if (model.SortOrder == "Desc")
                    {
                        query = query.OrderByDescending(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                    else
                    {
                        query = query.OrderBy(p => p.Deposit.CreatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                }
                else
                {
                    query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                }
            }

            var list = query
               .ToList().Select(x => new DepositListingModel(x.Deposit)).ToList();
            if (model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
                if (model.SortBy == "UserName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.UserName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PercentageAmount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PercentageAmount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Type).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "BANK")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Bank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Bank).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.ChkNoOrSlipId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "Status")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Status).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.Amount).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "NewBalance")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.NewBalance).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.NewBalance).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.PosNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }
            result.List = list.Take(15).ToList();


            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = totalrecoed;
            return result;


        }

        PagingResult<DepositExcelReportModel> IDepositManager.GetReportExcelData(ReportSearchModel model)
        {
            var result = new PagingResult<DepositExcelReportModel>();
            var query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));

            }
            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posList = new List<long>();
                posList = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.UserId)).AsEnumerable().Select(p => p.POSId).ToList();
                query = query.Where(p => posList.Contains(p.Deposit.POSId));
            }
            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            //if (!string.IsNullOrEmpty(model.Meter))
            //{
            //    query = query.Where(p => p.Deposit.m);
            //}
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder);
            }
            var list = query.OrderByDescending(p => p.Deposit.CreatedAt)
               .ToList().Select(x => new DepositExcelReportModel(x.Deposit)).ToList();
            if (model.SortBy == "UserName" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance")
            {
                if (model.SortBy == "UserName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.USERNAME).ToList();
                    else
                        list = list.OrderByDescending(p => p.USERNAME).ToList();
                }
                if (model.SortBy == "PercentageAmount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.PERCENT).ToList();
                    else
                        list = list.OrderByDescending(p => p.PERCENT).ToList();
                }
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_TYPE).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_TYPE).ToList();
                }
                if (model.SortBy == "BANK")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.BANK).ToList();
                    else
                        list = list.OrderByDescending(p => p.BANK).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_REF_NO).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_REF_NO).ToList();
                }

                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.AMOUNT).ToList();
                    else
                        list = list.OrderByDescending(p => p.AMOUNT).ToList();
                }
                if (model.SortBy == "NewBalance")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.NEW_BALANCE).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.NEW_BALANCE).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.POSID).ToList();
                    else
                        list = list.OrderByDescending(p => p.POSID).ToList();
                }
            }


            if (list.Count == 0)
            {
                var testdata = new DepositExcelReportModel();
                list = new List<DepositExcelReportModel>();
                list.Add(testdata);
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<DepositAuditExcelReportModel> IDepositManager.GetAuditReportExcelData(ReportSearchModel model)
        {
            var result = new PagingResult<DepositAuditExcelReportModel>();
            var query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(model.To));

            }
            if (model.To == null && model.From == null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.Deposit.CreatedAt) <= DbFunctions.TruncateTime(DateTime.Now));

            }
            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posList = new List<long>();
                posList = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.UserId)).AsEnumerable().Select(p => p.POSId).ToList();
                query = query.Where(p => posList.Contains(p.Deposit.POSId));
            }
            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.Deposit.POSId == model.PosId);
            }
            if (model.Bank.HasValue && model.Bank > 0)
            {
                query = query.Where(p => p.Deposit.BankAccountId == model.Bank);
            }
            if (model.DepositType.HasValue && model.DepositType > 0)
            {
                query = query.Where(p => p.Deposit.PaymentType == model.DepositType);
            }
            if (!string.IsNullOrEmpty(model.RefNumber))
            {
                query = query.Where(p => p.Deposit.CheckNumberOrSlipId.ToLower().Contains(model.RefNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.Deposit.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy == "NewBalance" && model.SortBy != "PAYER" && model.SortBy != "ISSUINGBANK")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder);
            }
            if (!string.IsNullOrEmpty(model.Amount))
            {
                query = query.Where(p => p.Deposit.Amount.ToString().Contains(model.Amount.Replace(",", "")));
            }
            if (!string.IsNullOrEmpty(model.IssuingBank))
            {
                query = query.Where(p => p.Deposit.ChequeBankName.ToLower().Contains(model.IssuingBank.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.Payer))
            {
                query = query.Where(p => p.Deposit.NameOnCheque.ToLower().Contains(model.Payer.ToLower()));
            }
            if (model.IsAudit)
            {
                query = query.Where(p => p.Deposit.isAudit == true);
            }
            if (!model.IsAudit && string.IsNullOrEmpty(model.Status))
            {
                query = query.Where(p => p.Deposit.isAudit == false);
            }
            var list = query.OrderByDescending(p => p.Deposit.CreatedAt)
               .ToList().Select(x => new DepositAuditExcelReportModel(x.Deposit)).ToList();
            if (model.SortBy == "DepositBy" || model.SortBy == "Amount" || model.SortBy == "POS" || model.SortBy == "PercentageAmount" || model.SortBy == "PaymentType" || model.SortBy == "BANK" || model.SortBy == "CheckNumberOrSlipId" || model.SortBy == "Status" || model.SortBy == "NewBalance" || model.SortBy != "PAYER" || model.SortBy != "ISSUINGBANK")
            {
                if (model.SortBy == "DepositBy")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_BY).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_BY).ToList();
                }
                if (model.SortBy == "PaymentType")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_TYPE).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_TYPE).ToList();
                }
                if (model.SortBy == "GTBANK")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.GTBANK).ToList();
                    else
                        list = list.OrderByDescending(p => p.GTBANK).ToList();
                }
                if (model.SortBy == "CheckNumberOrSlipId")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.DEPOSIT_REF_NO).ToList();
                    else
                        list = list.OrderByDescending(p => p.DEPOSIT_REF_NO).ToList();
                }

                if (model.SortBy == "Amount")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.AMOUNT).ToList();
                    else
                        list = list.OrderByDescending(p => p.AMOUNT).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.POSID).ToList();
                    else
                        list = list.OrderByDescending(p => p.POSID).ToList();
                }
                if (model.SortBy == "PAYER")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.PAYER).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.PAYER).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
                if (model.SortBy == "ISSUINGBANK")
                {
                    list = (model.SortOrder == "Asc" ? list.OrderBy(p => p.ISSUINGBANK).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList() :
                        list.OrderByDescending(p => p.ISSUINGBANK).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList());
                }
            }


            if (list.Count == 0)
            {
                var testdata = new DepositAuditExcelReportModel();
                list = new List<DepositAuditExcelReportModel>();
                list.Add(testdata);
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposit Logs List";
            result.TotalCount = query.Count();
            return result;
        }

        decimal IDepositManager.GetPendingDepositTotal()
        {
            var amt = Context.PendingDeposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.PercentageAmount != null && p.IsDeleted == false).ToList().Sum(p => p.PercentageAmount).Value;

            amt += Context.PendingDeposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.PercentageAmount == null && p.IsDeleted == false).ToList().Sum(p => p.Amount);
            return amt;
        }

        ActionOutput IDepositManager.ChangeDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId)
        {
            Deposit dbDeposit = new Deposit();
            var dbpendingDeposit = Context.PendingDeposits.FirstOrDefault(p => p.PendingDepositId == depositId) ?? null;
            if (dbpendingDeposit == null)
                return ReturnError("Deposit not exist.");
            else
            {
                if (status == DepositPaymentStatusEnum.Released)
                {
                    dbDeposit = (this as IDepositManager).SaveApprovedDeposit(dbpendingDeposit);
                    //Creating Log entry in deposit logs table
                    var dbDepositLog = new DepositLog();
                    dbDepositLog.UserId = currentUserId;
                    dbDepositLog.DepositId = dbDeposit.DepositId;
                    dbDepositLog.PreviousStatus = dbDeposit.Status;
                    dbDepositLog.NewStatus = (int)status;
                    dbDepositLog.CreatedAt = DateTime.UtcNow;
                    Context.DepositLogs.Add(dbDepositLog);
                    dbDeposit.Status = (int)status;
                    dbDeposit.IsDeleted = true;
                    dbDeposit.POS = Context.POS.FirstOrDefault(d => d.POSId == dbDeposit.POSId);
                    if (dbDeposit.POS != null && status == DepositPaymentStatusEnum.Released)
                    {
                        //dbDeposit.AgencyCommission = (this as IDepositManager).TakeCommisionsAndReturnAgentsCommision(dbDeposit.POSId, dbDeposit.Amount);
                         
                        if (dbDeposit.POS?.CommissionPercentage != null)
                        {
                            var percentage = dbDeposit.Amount * dbDeposit.POS.Commission.Percentage / 100;
                            dbDeposit.POS.Balance = dbDeposit.POS.Balance + percentage + dbDeposit.Amount;
                        }
                        else
                        {
                            dbDeposit.POS.Balance = dbDeposit.POS.Balance + dbDeposit.Amount;
                        }

                        if (dbDeposit.POS.User.Agency != null)
                        {
                            var agentPos = Context.POS.FirstOrDefault(a => a.VendorId == dbDeposit.POS.User.Agency.Representative);
                            if (agentPos != null)
                            {
                                var percentage = (dbDeposit.Amount * dbDeposit.POS.User.Agency.Commission.Percentage) / 100;
                                agentPos.Balance = agentPos.Balance == null ? percentage : agentPos.Balance + percentage;
                                dbDeposit.AgencyCommission = percentage;
                            }
                        }
                        dbDeposit.NewBalance = dbDeposit.POS.Balance; 

                        dbDeposit.TransactionId = Utilities.GetLastDepositTrabsactionId();
                        dbDeposit.IsDeleted = false;

                       
                       
                    }
                    dbpendingDeposit.ApprovedDepId = dbDeposit.DepositId; 
                    Context.SaveChanges();
                     
                    //Send push to all devices where this user logged in when admin released deposit
                    var deviceTokens = dbDeposit.User.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct();
                    var obj = new PushNotificationModel();
                    obj.UserId = dbDeposit.UserId;
                    obj.Id = dbDeposit.DepositId;
                    var notyAmount = string.Format("{0:N0}", dbDeposit.Amount);
                    if (dbDeposit.Status == (int)DepositPaymentStatusEnum.Rejected || dbDeposit.Status == (int)DepositPaymentStatusEnum.RejectedByAccountant)
                    {
                        obj.Title = "Deposit request rejected";
                        obj.Message = "Your deposit request has been rejected of SLL " + notyAmount;
                    }
                    else if (dbDeposit.Status == (int)DepositPaymentStatusEnum.Released)
                    {
                        obj.Title = "Wallet updated successfully";
                        obj.Message = "Your wallet has been updated with SLL " + notyAmount;
                    }
                    else if (dbDeposit.Status == (int)DepositPaymentStatusEnum.ApprovedByAccountant)
                    {
                        obj.Title = "Deposit request in progress";
                        obj.Message = "Your deposit request has been in processed of SLL " + notyAmount;
                    }
                    obj.NotificationType = NotificationTypeEnum.DepositStatusChange;
                    foreach (var item in deviceTokens)
                    {
                        obj.DeviceToken = item.DeviceToken;
                        obj.DeviceType = item.AppType.Value;
                        PushNotification.SendNotification(obj);
                    }

                }
            }

            return ReturnSuccess(dbDeposit.User.UserId, "Deposit status changed successfully.");
        }

        ActionOutput IDepositManager.ReverseDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId)
        {
            var dbDeposit = Context.Deposits.FirstOrDefault(p => p.DepositId == depositId);
            if (dbDeposit == null)
                return ReturnError("Deposit not exist.");


            //Creating a new entry here

            var reversedDeposit = new Deposit
            {
                Amount = Decimal.Negate(dbDeposit.Amount),
                DepositLogs = null,
                BankAccount = dbDeposit.BankAccount,
                BankAccountId = dbDeposit.BankAccountId,
                CheckNumberOrSlipId = dbDeposit.CheckNumberOrSlipId,
                ChequeBankName = dbDeposit.ChequeBankName,
                Comments = dbDeposit.Comments,
                CreatedAt = DateTime.UtcNow,
                isAudit = dbDeposit.isAudit,
                NameOnCheque = dbDeposit.NameOnCheque,
                NewBalance = dbDeposit.NewBalance,
                PaymentType = dbDeposit.PaymentType,
                PercentageAmount = Decimal.Negate(dbDeposit.PercentageAmount ?? new decimal()),
                POS = dbDeposit.POS,
                POSId = dbDeposit.POSId,
                Status = dbDeposit.Status,
                TransactionId = dbDeposit.TransactionId,
                UpdatedAt = dbDeposit.UpdatedAt,
                User = dbDeposit.User,
                UserId = dbDeposit.UserId,
                ValueDate = dbDeposit.ValueDate
            };

            Context.Deposits.Add(reversedDeposit);
            Context.SaveChanges();

            //Creating Log entry in deposit logs table
            var dbDepositLog = new DepositLog();
            dbDepositLog.UserId = currentUserId;
            dbDepositLog.DepositId = reversedDeposit.DepositId;
            dbDepositLog.PreviousStatus = reversedDeposit.Status;
            dbDepositLog.NewStatus = (int)status;
            dbDepositLog.CreatedAt = DateTime.UtcNow;
            Context.DepositLogs.Add(dbDepositLog);


            reversedDeposit.Status = (int)status;
            if (dbDeposit.POS != null && status == DepositPaymentStatusEnum.Reversed)
            {
                var lastPosReleaseDeposit = Context.Deposits.Where(p => p.POSId == dbDeposit.POSId).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (lastPosReleaseDeposit != null && lastPosReleaseDeposit.NewBalance != null)
                {
                    reversedDeposit.NewBalance = lastPosReleaseDeposit.NewBalance.Value - dbDeposit.PercentageAmount;
                    reversedDeposit.NewBalance = reversedDeposit.NewBalance;
                }
                else
                {
                    reversedDeposit.POS.Balance = dbDeposit.POS.Balance == null ? 
                        (0 + (dbDeposit.PercentageAmount == null || dbDeposit.PercentageAmount == 0 ? dbDeposit.Amount : dbDeposit.PercentageAmount))
                        : (dbDeposit.POS.Balance - (dbDeposit.PercentageAmount == null || dbDeposit.PercentageAmount == 0 ? dbDeposit.Amount : dbDeposit.PercentageAmount));
                }

                reversedDeposit.NewBalance = dbDeposit.POS.Balance;
            }
            Context.SaveChanges();
            //Send push to all devices where this user logged in when admin released deposit
            var deviceTokens = dbDeposit.User.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct();
            var obj = new PushNotificationModel();
            obj.UserId = dbDeposit.UserId;
            obj.Id = dbDeposit.DepositId;
            var notyAmount = string.Format("{0:N0}", dbDeposit.Amount);
            obj.Title = "Wallet updated successfully";
            obj.Message = "Your wallet has been updated with SLL " + notyAmount;
            obj.NotificationType = NotificationTypeEnum.DepositStatusChange;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                PushNotification.SendNotification(obj);
            }
            return ReturnSuccess("Deposit status changed successfully.");
        }

        ActionOutput<List<long>> IDepositManager.ChangeMultipleDepositStatus(ReleaseDepositModel model, long userId)
        {
            List<long> userIds = new List<long>();
            try
            {

                if (!(Context.DepositOTPs.Any(p => p.OTP == model.OTP && !p.IsUsed)))
                    return ReturnError<List<long>>("WRONG OTP ENTERED");

                if (model.CancelDepositIds != null)
                {
                    foreach (var depositId in model.CancelDepositIds)
                    {
                        //userIds.Add((this as IDepositManager).ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Rejected, userId).ID);
                        var pendingDepoosits = Context.PendingDeposits.Where(d => model.CancelDepositIds.Contains(d.PendingDepositId)).ToList();
                        if (pendingDepoosits.Any())
                        {
                            Context.PendingDeposits.RemoveRange(pendingDepoosits);
                            Context.SaveChanges();
                        }
                    }
                }
                if (model.ReleaseDepositIds != null)
                {
                    foreach (var depositId in model.ReleaseDepositIds)
                    {
                        (this as IDepositManager).ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Released, userId);
                    }
                }
                return ReturnSuccess(userIds, "Deposit updated successfully.");
            }
            catch (Exception ex)
            {
                return ReturnError<List<long>>("Error occured while updating entries.");
            }
        }

        ActionOutput IDepositManager.ChangeMultipleDepositStatusOnReverse(ReverseDepositModel model, long userId)
        {
            try
            {
                if (!(Context.DepositOTPs.Any(p => p.OTP == model.OTP && !p.IsUsed)))
                    return ReturnError("Invalid OTP");

                if (model.ReverseDepositIds != null)
                {
                    for (int i = 0; i < model.ReverseDepositIds.Count; i++)
                    {
                        (this as IDepositManager).ReverseDepositStatus(model.ReverseDepositIds[i], DepositPaymentStatusEnum.Reversed, userId);
                    }
                }
                return ReturnSuccess("Deposit status updated successfully.");
            }
            catch (Exception ed)
            {
                return ReturnError("Error occured while updating entries.");
            }

        }

        ActionOutput<string> IDepositManager.SendOTP()
        {
            try
            {
                var otp = Utilities.GenerateRandomNo();
                var dbDepositOTP = new DepositOTP();
                dbDepositOTP.CreatedAt = DateTime.UtcNow;
                dbDepositOTP.IsUsed = false;
                dbDepositOTP.OTP = otp.ToString();
                Context.DepositOTPs.ToList().ForEach(p => p.IsUsed = true);
                Context.DepositOTPs.Add(dbDepositOTP);
                Context.SaveChanges();
                return ReturnSuccess<string>(otp.ToString(), "OTP sent successfully.");
            }
            catch (Exception)
            {

                return ReturnError<string>("Error occured in sending OTP.");
            }
        }

        ActionOutput<DepositListingModel> IDepositManager.GetDepositDetail(long depositId)
        {
            var dbDeposit = Context.Deposits.FirstOrDefault(p => p.DepositId == depositId);

            var thisDepositNotification = Context.Notifications.FirstOrDefault(d => d.Type == (int)NotificationTypeEnum.DepositStatusChange && d.RowId == depositId);
            if (thisDepositNotification != null)
            {
                thisDepositNotification.MarkAsRead = true;
                Context.SaveChanges();
            }
            if (dbDeposit == null)
                return ReturnError<DepositListingModel>("Deposit not exist.");
            var data = new DepositListingModel(dbDeposit, true);
            return ReturnSuccess<DepositListingModel>(data, "Deposit detail fetched successfully.");
        }

        decimal IDepositManager.ReturnPendingDepositsTotalAmount(DepositModel model)
        {
            var deposits = Context.PendingDeposits.Where(d => d.Status == (int)DepositPaymentStatusEnum.Pending && d.POSId == model.PosId).Select(d => d.Amount);
            if (deposits.Any())
            {
                return deposits.Sum();
            }
            return 0;
        }

        decimal IDepositManager.TakeCommisionsAndReturnAgentsCommision(long posId, decimal amt)
        {
            decimal agentsCommission = 0;
            var pos = Context.POS.FirstOrDefault(d => d.POSId == posId);
            if (pos?.CommissionPercentage != null)
            {
                var percentage = amt * pos.Commission.Percentage / 100;
                pos.Balance = pos.Balance + percentage;
            }

            if (pos.User.Agency != null)
            {
                var agentPos = Context.POS.FirstOrDefault(a => a.VendorId == pos.User.Agency.Representative);
                if (agentPos != null)
                {
                    var percentage = (amt * pos.User.Agency.Commission.Percentage) / 100;
                    agentPos.Balance = agentPos.Balance == null ? percentage : agentPos.Balance + percentage;
                    agentsCommission = percentage;
                }
            }
            return agentsCommission;
        }

        ActionOutput<PendingDeposit> IDepositManager.SaveDepositRequest(DepositModel model)
        {
            var userAssignedPos = new POS();
          

            userAssignedPos = Context.POS.FirstOrDefault(d => d.POSId == model.PosId) ?? null;
            if (userAssignedPos == null)
            {
                throw new ArgumentException("POS ID NOT FOUND");
            }
            model.PosId = userAssignedPos.POSId;
            model.UserId = userAssignedPos.VendorId ?? 0;

            var dbDeposit = new PendingDeposit();
            dbDeposit.Amount = model.Amount;
            dbDeposit.UserId = model.UserId;
            dbDeposit.POSId = model.PosId;
            dbDeposit.IsDeleted = false;
            dbDeposit.PaymentType = (int)model.DepositType;
            dbDeposit.ChequeBankName = model.ChkBankName;
            dbDeposit.NameOnCheque = model.NameOnCheque;
            dbDeposit.PendingBankAccountId = model.BankAccountId;
            dbDeposit.CheckNumberOrSlipId = model.ChkOrSlipNo;
            dbDeposit.Comments = model.Comments;
            var percentage = dbDeposit.Amount * userAssignedPos.Commission.Percentage / 100;
            dbDeposit.PercentageAmount = dbDeposit.Amount + percentage;
            dbDeposit.TransactionId = "0";
            dbDeposit.CreatedAt = DateTime.UtcNow;
            dbDeposit.Status = (int)DepositPaymentStatusEnum.Pending;
            dbDeposit.ValueDate = model.ValueDate+ " 12:00";//.ToString("dd/MM/yyyy hh:mm");
            dbDeposit.NextReminderDate = DateTime.UtcNow.AddDays(15);
            Context.PendingDeposits.Add(dbDeposit);
            Context.SaveChanges();
            return ReturnSuccess<PendingDeposit>(dbDeposit, "Deposit request saved successfully.");//PLEASE DO NOT CHANGE STRING VALUE
        }

        Deposit IDepositManager.SaveApprovedDeposit(PendingDeposit model)
        {
            if (model.POSId == 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
                var userAssignedPos = new POS();
                if (user.UserRole.Role == UserRoles.Vendor)
                    userAssignedPos = user.POS.FirstOrDefault();
                else if (user.UserRole.Role == UserRoles.AppUser && user.User1 != null)
                    userAssignedPos = user.User1.POS.FirstOrDefault();
                if (userAssignedPos != null)
                    model.POSId = userAssignedPos.POSId;
            }

            var dbDeposit = new Deposit();
            dbDeposit.Amount = model.Amount;
            dbDeposit.UserId = model.UserId;
            dbDeposit.POSId = model.POSId;
            dbDeposit.IsDeleted = false;
            dbDeposit.PaymentType = (int)model.PaymentType;
            dbDeposit.ChequeBankName = model.ChequeBankName;
            dbDeposit.NameOnCheque = model.NameOnCheque;
            dbDeposit.BankAccountId = model.PendingBankAccountId;
            dbDeposit.CheckNumberOrSlipId = model.CheckNumberOrSlipId;
            dbDeposit.Comments = model.Comments;
            dbDeposit.PercentageAmount = model.PercentageAmount;
            dbDeposit.TransactionId = "0";
            dbDeposit.CreatedAt = model.CreatedAt;
            dbDeposit.Status = (int)DepositPaymentStatusEnum.Pending;
            dbDeposit.ValueDate = model.ValueDate;// + //" 12:00";//.ToString("dd/MM/yyyy hh:mm");
            dbDeposit.NextReminderDate = DateTime.UtcNow.AddDays(15);
            Context.Deposits.Add(dbDeposit);
            Context.SaveChanges(); 
            return dbDeposit;
        }

        DepositAuditModel IDepositManager.UpdateDepositAuditRequest(DepositAuditModel depositAuditModel)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var posId = new POS();
            //.Include(x => x.POS) .Include(x => x.User).Include(x => x.BankAccount)
            var dbDeposit = Context.Deposits
                .FirstOrDefault(x => x.DepositId == depositAuditModel.DepositId);

            posId = Context.POS.FirstOrDefault(x => x.POSId == depositAuditModel.PosId);
            dbDeposit.Amount = Convert.ToDecimal(depositAuditModel.Amount.ToString().Replace(",", ""));
            dbDeposit.UserId = depositAuditModel.UserId;
            dbDeposit.POSId = posId != null ? posId.POSId : dbDeposit.POSId;
            dbDeposit.ChequeBankName = depositAuditModel.IssuingBank != null ? depositAuditModel.IssuingBank : "";
            dbDeposit.NameOnCheque = depositAuditModel.Payer != null ? depositAuditModel.Payer : "";
            dbDeposit.CheckNumberOrSlipId = depositAuditModel.DepositRef != null ? depositAuditModel.DepositRef : "";
            dbDeposit.UpdatedAt = DateTime.UtcNow;
            if (!depositAuditModel.isAudit)
            {

                dbDeposit.NextReminderDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(15));
            }
            else
            {
                if (DateTime.UtcNow.Date < dbDeposit.NextReminderDate.Value.Date)
                {
                    dbDeposit.NextReminderDate = DateTime.UtcNow.AddDays(15);
                }
            }
            
            dbDeposit.ValueDate = DateTime.ParseExact(depositAuditModel.ValueDateModel, "dd/MM/yyyy hh:mm", provider).ToString("dd/MM/yyyy hh:mm");
            //dbDeposit.isAudit = depositAuditModel.isAudit;
            dbDeposit.PaymentType = depositAuditModel.Type != null ? (int)Enum.Parse(typeof(DepositPaymentTypeEnum), depositAuditModel.Type) : (int)DepositPaymentTypeEnum.Cash;
            dbDeposit.BankAccount.BankName = depositAuditModel.GTBank != null ? depositAuditModel.GTBank : "";
            dbDeposit.Comments = depositAuditModel.Comment;
            Context.SaveChanges();


            depositAuditModel.DateTime = dbDeposit.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            depositAuditModel.DepositBy = dbDeposit.POS.User.Vendor;
            depositAuditModel.IssuingBank = dbDeposit.ChequeBankName != null ?
            dbDeposit.ChequeBankName + '-' + dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty)
            .Substring(dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : "";
            depositAuditModel.Payer = dbDeposit.NameOnCheque;

            //depositAuditModel.isAudit = !depositAuditModel.isAudit;

            //Get Description filed of enum 
            if (dbDeposit.PaymentType == (int)DepositPaymentTypeEnum.PurchaseOrder)
            {
                var fieldInfo = DepositPaymentTypeEnum.PurchaseOrder.GetType().GetField(DepositPaymentTypeEnum.PurchaseOrder.ToString());

                var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                depositAuditModel.Type = descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : DepositPaymentTypeEnum.PurchaseOrder.ToString();
            }
            else
                depositAuditModel.Type = ((DepositPaymentTypeEnum)dbDeposit.PaymentType).ToString();

            depositAuditModel.DepositId = dbDeposit.DepositId;
            depositAuditModel.Price = Convert.ToString(Convert.ToDecimal(depositAuditModel.Amount));
            depositAuditModel.PosId = Convert.ToInt64(posId.SerialNumber);
            depositAuditModel.ValueDateModel = DateTime.ParseExact(dbDeposit.ValueDate, "dd/MM/yyyy hh:mm",
                CultureInfo.InvariantCulture).ToString("dd/MM/yyyy hh:mm");
            return depositAuditModel;
        }
        DepositAuditModel IDepositManager.SaveDepositAuditRequest(DepositAuditModel depositAuditModel)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            var posId = new POS();
            //.Include(x => x.POS) .Include(x => x.User).Include(x => x.BankAccount)
            var dbDeposit = Context.Deposits
                .FirstOrDefault(x => x.DepositId == depositAuditModel.DepositId);

            posId = Context.POS.FirstOrDefault(x => x.POSId == depositAuditModel.PosId);
            dbDeposit.Amount = Convert.ToDecimal(depositAuditModel.Amount.ToString().Replace(",", ""));
            dbDeposit.UserId = depositAuditModel.UserId;
            dbDeposit.POSId = posId != null ? posId.POSId : dbDeposit.POSId;
            dbDeposit.ChequeBankName = depositAuditModel.IssuingBank != null ? depositAuditModel.IssuingBank : "";
            dbDeposit.NameOnCheque = depositAuditModel.Payer != null ? depositAuditModel.Payer : "";
            dbDeposit.CheckNumberOrSlipId = depositAuditModel.DepositRef != null ? depositAuditModel.DepositRef : "";
            dbDeposit.UpdatedAt = DateTime.UtcNow;
            dbDeposit.ValueDate = DateTime.ParseExact(depositAuditModel.ValueDateModel, "dd/MM/yyyy hh:mm", provider).ToString("dd/MM/yyyy hh:mm");

            var currentDate = DateTime.UtcNow;

            if (depositAuditModel.isAudit)
            {
                dbDeposit.NextReminderDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(15));     
            }
            else
            {
                   dbDeposit.NextReminderDate = DateTime.UtcNow;
            }
               

            dbDeposit.isAudit = depositAuditModel.isAudit;
            dbDeposit.PaymentType = depositAuditModel.Type != null ? (int)Enum.Parse(typeof(DepositPaymentTypeEnum), depositAuditModel.Type) : (int)DepositPaymentTypeEnum.Cash;
            dbDeposit.BankAccount.BankName = depositAuditModel.GTBank != null ? depositAuditModel.GTBank : "";
            dbDeposit.Comments = depositAuditModel.Comment;
            Context.SaveChanges();
            

            depositAuditModel.DateTime = dbDeposit.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            depositAuditModel.DepositBy = dbDeposit.POS.User.Vendor;
            depositAuditModel.IssuingBank = dbDeposit.ChequeBankName != null ?
            dbDeposit.ChequeBankName + '-' + dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty)
            .Substring(dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : "";
            depositAuditModel.Payer = dbDeposit.NameOnCheque;


            //Get Description filed of enum 
            if (dbDeposit.PaymentType == (int)DepositPaymentTypeEnum.PurchaseOrder)
            {
                var fieldInfo = DepositPaymentTypeEnum.PurchaseOrder.GetType().GetField(DepositPaymentTypeEnum.PurchaseOrder.ToString());

                var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                depositAuditModel.Type = descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : DepositPaymentTypeEnum.PurchaseOrder.ToString();
            }
            else
                depositAuditModel.Type = ((DepositPaymentTypeEnum)dbDeposit.PaymentType).ToString();

            depositAuditModel.Comment = dbDeposit.Comments;
            depositAuditModel.DepositId = dbDeposit.DepositId;
            depositAuditModel.Price = Convert.ToString(Convert.ToDecimal(depositAuditModel.Amount));
            depositAuditModel.PosId = Convert.ToInt64(posId.SerialNumber);
            depositAuditModel.ValueDateModel = DateTime.ParseExact(dbDeposit.ValueDate, "dd/MM/yyyy hh:mm",
                CultureInfo.InvariantCulture).ToString("dd/MM/yyyy hh:mm");
            return depositAuditModel;
        }
        
        List<Deposit> IDepositManager.GetUnclearedDeposits()
        {
            var currentDate = DateTime.UtcNow;
           var result = Context.Deposits.Where(d => d.isAudit == false 
           && d.NextReminderDate != null
           && currentDate >=  d.NextReminderDate && d.IsDeleted == false).ToList();
            return result;
        }

        void IDepositManager.UpdateNextReminderDate(Deposit deposit)
        {
            deposit.NextReminderDate = DateTime.UtcNow.AddDays(15);
            Context.SaveChanges();
        }

        List<PendingDeposit> IDepositManager.GetListOfDeposits(List<long> depositIds)
        {
            return Context.PendingDeposits.Where(d => depositIds.Contains(d.PendingDepositId)).ToList() ?? new List<PendingDeposit>();
        }

        void IDepositManager.DeletePendingDeposits(List<PendingDeposit> deposits)
        {
            Context.PendingDeposits.RemoveRange(deposits);
            Context.SaveChanges();
        }



        IQueryable<BalanceSheetListingModel> IDepositManager.GetBalanceSheetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 999999999;
            IQueryable<BalanceSheetListingModel> query = null;


            if (model.IsInitialLoad)
            {
                query = from a in Context.Deposits
                        where DbFunctions.TruncateTime(a.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow)
                        select new BalanceSheetListingModel
                        {
                            DateTime = a.CreatedAt,
                            Reference = a.CheckNumberOrSlipId,
                            TransactionId = a.TransactionId,
                            TransactionType = "Deposit",
                            DepositAmount = a.PercentageAmount ?? 0,
                            SaleAmount = 0,
                            Balance = 0,
                            POSId = a.POSId
                        };
            }
            else
            {
                query = from a in Context.Deposits
                        select new BalanceSheetListingModel
                        {
                            DateTime = a.CreatedAt,
                            Reference = a.CheckNumberOrSlipId,
                            TransactionId = a.TransactionId,
                            TransactionType = "Deposit",
                            DepositAmount = a.PercentageAmount ?? 0,
                            SaleAmount = 0,
                            Balance = 0,
                            POSId = a.POSId
                        };
            }

            if (model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        //posIds = Context.POS.Where(p => p.VendorId != null && p.VendorId == model.VendorId || p.User.AgentId == agentId).Select(p => p.POSId).ToList();
                        posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = Context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
                    }
                }
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }

           
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.DateTime) >= DbFunctions.TruncateTime(model.From));
            }

            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.DateTime) <= DbFunctions.TruncateTime(model.To));
            }

            if (model.VendorId.HasValue && model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId??0));
            }

            if (model.PosId.HasValue && model.PosId > 0)
            {
                query = query.Where(p => p.POSId == model.PosId);
            }
        
            if (model.SortBy != "UserName" && model.SortBy != "POS" && model.SortBy != "TransactionId" && model.SortBy != "Amount" && model.SortBy != "PercentageAmount" && model.SortBy != "PaymentType" && model.SortBy != "BANK" && model.SortBy != "CheckNumberOrSlipId" && model.SortBy != "Status" && model.SortBy != "NewBalance")
            {
                if (model.SortBy == "CreatedAt")
                {
                    if (model.SortOrder == "Desc")
                    {
                        query = query.OrderByDescending(p => p.DateTime).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                    else
                    {
                        query = query.OrderBy(p => p.DateTime).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                    }
                }
                else
                {
                    query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
                }
            } 
            return query; 
        }

        IQueryable<DashboardBalanceSheetModel> IDepositManager.GetDashboardBalanceSheetReports()
        {
            return Context.Deposits.GroupBy(f => f.UserId).Select(f => new DashboardBalanceSheetModel
            {
                SaleAmount = 0,
                Vendor = f.FirstOrDefault().User.Vendor,
                UserId = f.FirstOrDefault().UserId,
                Balance = 0,
                DepositAmount = f.Sum(d => d.PercentageAmount ?? 0),
                Status = "",
                POSBalance = f.OrderByDescending(a => a.POS.Balance).FirstOrDefault().POS.Balance ?? 0
            });
        }
    }
}