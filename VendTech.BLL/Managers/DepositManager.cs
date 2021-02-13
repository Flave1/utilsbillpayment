using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Data.Entity;

namespace VendTech.BLL.Managers
{
    public class DepositManager : BaseManager, IDepositManager
    {

        string IDepositManager.GetWelcomeMessage()
        {
            return "Welcome To Base Project Demo";
        }

        PagingResult<DepositListingModel> IDepositManager.GetDepositPagedList(PagingModel model, bool getForRelease, long vendorId)
        {
            var result = new PagingResult<DepositListingModel>();

            // model.RecordsPerPage = 2;
            IQueryable<Deposit> query = Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending
            && p.POS.Enabled != false
            ).OrderBy(model.SortBy + " " + model.SortOrder);
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
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new DepositListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Deposits List";
            result.TotalCount = query.Count();
            return result;
        }
        PagingResult<DepositListingModel> IDepositManager.GetUserDepositList(int pageNo, int pageSize, long userId)
        {
            var result = new PagingResult<DepositListingModel>();
            var query = Context.Deposits.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt);
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

        PagingResult<DepositListingModel> IDepositManager.GetReportsPagedList(ReportSearchModel model, bool callFromAdmin)
        {
            model.RecordsPerPage = 10000000;
            IQueryable<DepositLog> query = null;
            var result = new PagingResult<DepositListingModel>();

            if (!model.IsInitialLoad)
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);
            else
                query = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released && DbFunctions.TruncateTime(p.Deposit.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

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
            var list = query
               .ToList().Select(x => new DepositListingModel(x.Deposit)).ToList();
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
                query = query.Where(p => p.Deposit.Amount.ToString().Contains(model.Amount.ToLower().Replace(",", "")));
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
                // query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);

                query = model.SortBy == "CreatedAt" ?
                    (model.SortOrder == "Desc" ?
                    query.OrderByDescending(p => p.Deposit.UpdatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage) :
                    query.OrderBy(p => p.Deposit.UpdatedAt).Skip((model.PageNo - 1)).Take(model.RecordsPerPage)) :
                    query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
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

        PagingResult<DepositListingModel> IDepositManager.GetReportsPagedHistoryList(ReportSearchModel model, bool callFromAdmin)
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
            var amt = Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.PercentageAmount != null).ToList().Sum(p => p.PercentageAmount).Value;

            amt += Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.PercentageAmount == null).ToList().Sum(p => p.Amount);
            return amt;
        }
        ActionOutput IDepositManager.ChangeDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId)
        {
            var dbDeposit = Context.Deposits.FirstOrDefault(p => p.DepositId == depositId);
            if (dbDeposit == null)
                return ReturnError("Deposit not exist.");
            //Creating Log entry in deposit logs table
            var dbDepositLog = new DepositLog();
            dbDepositLog.UserId = currentUserId;
            dbDepositLog.DepositId = depositId;
            dbDepositLog.PreviousStatus = dbDeposit.Status;
            dbDepositLog.NewStatus = (int)status;
            dbDepositLog.CreatedAt = DateTime.UtcNow;
            Context.DepositLogs.Add(dbDepositLog);
            dbDeposit.Status = (int)status;
            if (dbDeposit.POS != null && status == DepositPaymentStatusEnum.Released)
            {
                var lastPosReleaseDeposit = Context.Deposits.Where(p => p.POSId == dbDeposit.POSId).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (lastPosReleaseDeposit != null && lastPosReleaseDeposit.NewBalance != null)
                    dbDeposit.NewBalance = lastPosReleaseDeposit.NewBalance.Value + dbDeposit.PercentageAmount;
                else
                    // new balance same as current POS balance
                    // dbDeposit.NewBalance = dbDeposit.POS.Balance == null ? (0 + dbDeposit.Amount) : dbDeposit.POS.Balance.Value + dbDeposit.PercentageAmount;
                    dbDeposit.POS.Balance = dbDeposit.POS.Balance == null ? (0 + (dbDeposit.PercentageAmount == null || dbDeposit.PercentageAmount == 0 ? dbDeposit.Amount : dbDeposit.PercentageAmount)) : (dbDeposit.POS.Balance + (dbDeposit.PercentageAmount == null || dbDeposit.PercentageAmount == 0 ? dbDeposit.Amount : dbDeposit.PercentageAmount));
                dbDeposit.NewBalance = dbDeposit.POS.Balance;
            }
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
            return ReturnSuccess("Deposit status changed successfully.");
        }
        ActionOutput IDepositManager.ChangeMultipleDepositStatus(ReleaseDepositModel model, long userId)
        {
            try
            {
                if (!(Context.DepositOTPs.Any(p => p.OTP == model.OTP && !p.IsUsed)))
                    return ReturnError("Invalid OTP");
                if (model.CancelDepositIds != null)
                {
                    for (int i = 0; i < model.CancelDepositIds.Count; i++)
                    {
                        (this as IDepositManager).ChangeDepositStatus(model.CancelDepositIds[i], DepositPaymentStatusEnum.Rejected, userId);
                    }
                }
                if (model.ReleaseDepositIds != null)
                {
                    for (int i = 0; i < model.ReleaseDepositIds.Count; i++)
                    {
                        (this as IDepositManager).ChangeDepositStatus(model.ReleaseDepositIds[i], DepositPaymentStatusEnum.Released, userId);
                    }
                }
                return ReturnSuccess("Deposit status updated successfully.");
            }
            catch (Exception)
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
            if (dbDeposit == null)
                return ReturnError<DepositListingModel>("Deposit not exist.");
            var data = new DepositListingModel(dbDeposit, true);
            return ReturnSuccess<DepositListingModel>(data, "Deposit detail fetched successfully.");
        }

        ActionOutput IDepositManager.SaveDepositRequest(DepositModel model)
        {
            //if (model.Amount < Utilities.MinimumDepositAmount || model.Amount > Utilities.MaximumDepositAmount)
            //    return ReturnError("Deposit amount must be between 50 to 500.");
            if (model.PosId == 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
                var userAssignedPos = new POS();
                if (user.UserRole.Role == UserRoles.Vendor)
                    userAssignedPos = user.POS.FirstOrDefault();
                else if (user.UserRole.Role == UserRoles.AppUser && user.User1 != null)
                    userAssignedPos = user.User1.POS.FirstOrDefault();
                if (userAssignedPos != null)
                    model.PosId = userAssignedPos.POSId;
            }
            var dbDeposit = new Deposit();
            dbDeposit.Amount = model.Amount;
            dbDeposit.UserId = model.UserId;
            dbDeposit.POSId = model.PosId;
            dbDeposit.PaymentType = (int)model.DepositType;
            dbDeposit.ChequeBankName = model.ChkBankName;
            dbDeposit.NameOnCheque = model.NameOnCheque;
            dbDeposit.BankAccountId = model.BankAccountId;
            dbDeposit.CheckNumberOrSlipId = model.ChkOrSlipNo;
            dbDeposit.Comments = model.Comments;
            dbDeposit.PercentageAmount = model.TotalAmountWithPercentage;
            dbDeposit.TransactionId = Utilities.GenerateUniqueId();
            dbDeposit.CreatedAt = DateTime.UtcNow;
            dbDeposit.Status = (int)DepositPaymentStatusEnum.Pending;
            Context.Deposits.Add(dbDeposit);
            Context.SaveChanges();
            return ReturnSuccess("Deposit request saved successfully.");
        }

        DepositAuditModel IDepositManager.SaveDepositAuditRequest(DepositAuditModel depositAuditModel)
        {
            var dbDeposit = Context.Deposits.Include(x => x.User).Include(x => x.BankAccount).FirstOrDefault(x => x.DepositId == depositAuditModel.DepositId);
            var posId = Context.POS.FirstOrDefault(x => x.POSId == depositAuditModel.PosId);
            dbDeposit.Amount = Convert.ToDecimal(depositAuditModel.Amount.ToString().Replace(",", ""));
            dbDeposit.POSId = depositAuditModel.PosId;
            dbDeposit.ChequeBankName = depositAuditModel.IssuingBank != null ? depositAuditModel.IssuingBank : "";
            dbDeposit.NameOnCheque = depositAuditModel.Payer != null ? depositAuditModel.Payer : "";
            dbDeposit.CheckNumberOrSlipId = depositAuditModel.DepositRef != null ? depositAuditModel.DepositRef : "";
            dbDeposit.UpdatedAt = DateTime.UtcNow;
            dbDeposit.isAudit = depositAuditModel.isAudit;
            dbDeposit.BankAccount.BankName = depositAuditModel.GTBank != null ? depositAuditModel.GTBank.Substring(0, depositAuditModel.GTBank.LastIndexOf("-")) : "";
            dbDeposit.User.Name = !(string.IsNullOrEmpty(depositAuditModel.DepositBy)) ? depositAuditModel.DepositBy.Substring(0, depositAuditModel.DepositBy.IndexOf(" ") != -1 ? depositAuditModel.DepositBy.IndexOf(" ") : depositAuditModel.DepositBy.Length) : dbDeposit.User.Name;
            dbDeposit.User.SurName = !(string.IsNullOrEmpty(depositAuditModel.DepositBy)) ? depositAuditModel.DepositBy.Substring(depositAuditModel.DepositBy.IndexOf(" ") != -1 ? depositAuditModel.DepositBy.IndexOf(" ") : 0) : dbDeposit.User.SurName;

            Context.Deposits.Add(dbDeposit);
            Context.Entry(dbDeposit).State = EntityState.Modified;
            Context.SaveChanges();
            depositAuditModel.DateTime = Convert.ToString(dbDeposit.CreatedAt);
            depositAuditModel.DepositBy = dbDeposit.User.Name + " " + dbDeposit.User.SurName;
            depositAuditModel.IssuingBank = dbDeposit.ChequeBankName != null ?
                dbDeposit.ChequeBankName + '-' + dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(dbDeposit.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : "";
            depositAuditModel.Payer = dbDeposit.NameOnCheque;
            depositAuditModel.Type = ((DepositPaymentTypeEnum)dbDeposit.PaymentType).ToString();
            depositAuditModel.DepositId = dbDeposit.DepositId;
            depositAuditModel.Price = Convert.ToString(Convert.ToDecimal(depositAuditModel.Amount));
            depositAuditModel.PosId = Convert.ToInt64(posId.SerialNumber);
            return depositAuditModel;
        }
    }
}
