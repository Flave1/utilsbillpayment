using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Data.Entity;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using System.Net.Http;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace VendTech.BLL.Managers
{
    public class MeterManager : BaseManager, IMeterManager
    {

        ActionOutput IMeterManager.SaveMeter(MeterModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId);
                if (dbMeter == null)
                    return ReturnError("Meter not exist.");
            }
            else
            {
                if (Context.Meters.Any(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId))
                    return ReturnError("Same meter number already exist for you.");
            }
            dbMeter.Name = model.Name;
            dbMeter.Number = model.Number;
            dbMeter.MeterMake = model.MeterMake;
            dbMeter.Address = model.Address;
            dbMeter.Allias = model.Allias;
            dbMeter.IsVerified = model.MeterId > 0 ? model.isVerified : false;
            if (model.MeterId == 0)
            {
                dbMeter.UserId = model.UserId;
                dbMeter.CreatedAt = DateTime.UtcNow;
                Context.Meters.Add(dbMeter);
            }
            Context.SaveChanges();
            return ReturnSuccess("Meter details saved successfully.");
        }
        MeterModel IMeterManager.GetMeterDetail(long meterId)
        {
            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == meterId);
            if (dbMeter == null)
                return null;
            return new MeterModel
            {
                Address = dbMeter.Address,
                MeterId = dbMeter.MeterId,
                MeterMake = dbMeter.MeterMake,
                Name = dbMeter.Name,
                Number = dbMeter.Number,
                isVerified = (bool)dbMeter.IsVerified,
                Allias = dbMeter.Allias
            };
        }
        ActionOutput IMeterManager.DeleteMeter(long meterId, long userId)
        {

            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == meterId && p.UserId == userId);
            if (dbMeter == null)
                return ReturnError("Meter not exist.");
            dbMeter.IsDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Meter deleted successfully.");
        }
        List<SelectListItem> IMeterManager.GetMetersDropDown(long userID)
        {
            return Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID).Select(p => new SelectListItem { Text = p.Number, Value = p.MeterId.ToString() }).ToList();
        }

        PagingResult<MeterAPIListingModel> IMeterManager.GetMeters(long userID, int pageNo, int pageSize)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID).ToList();
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterAPIListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meters fetched successfully.";
            return result;
        }
        PagingResult<SalesReportExcelModel> IMeterManager.GetSalesExcelReportData(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<SalesReportExcelModel>();

            var query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null);
            //            if (model.SortBy == "UserName" )
            //            {
            //                query = query.OrderBy(p =>"Name" + " " + model.SortOrder);
            //            }
            //else if(model.SortBy == "MeterNumber"){
            //    query = query.OrderBy(p => model.SortBy + " " + ( p.MeterNumber));

            //}
            //            else
            //                query = query.OrderBy(model.SortBy + " " + model.SortOrder);
            if (model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) >= DbFunctions.TruncateTime(model.From));

            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) <= DbFunctions.TruncateTime(model.To));

            }
            if (model.PosId != null)
            {
                query = query.Where(p => p.POSId == model.PosId);

            }
            if (!string.IsNullOrEmpty(model.Meter))
            {
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber != null && p.MeterNumber.Contains(model.Meter)));

            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            result.TotalCount = query.Count();


            if (model.SortBy != "VendorName" && model.SortBy != "MeterNumber" && model.SortBy != "POS")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder);
            }
            var list = query.ToList().Select(x => new SalesReportExcelModel
            {
                Date_TIME = x.CreatedAt.ToString("dd/MM/yyyy HH:mm"),//ToString("dd/MM/yyyy HH:mm"),
                PRODUCT_TYPE = x?.Platform?.ShortName,
                PIN = x.MeterToken1,
                AMOUNT = string.Format("{0:N0}", x.Amount),
                TRANSACTIONID = x.TransactionId,
                METER_NO = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                VENDORNAME = x.POS.User == null ? "" : x.POS.User.Vendor,
                POSID = x.POSId == null ? "" : x.POS.SerialNumber,
            }).ToList();
            if (model.SortBy == "VendorName" || model.SortBy == "MeterNumber" || model.SortBy == "POS")
            {
                if (model.SortBy == "VendorName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.VENDORNAME).ToList();
                    else
                        list = list.OrderByDescending(p => p.VENDORNAME).ToList();
                }
                if (model.SortBy == "MeterNumber")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.METER_NO).ToList();
                    else
                        list = list.OrderByDescending(p => p.METER_NO).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.POSID).ToList();
                    else
                        list = list.OrderByDescending(p => p.POSID).ToList();
                }
            }


            // if data will not available so pass blank single data
            if (list.Count() == 0)
            {
                var testdata = new SalesReportExcelModel();
                list = new List<SalesReportExcelModel>();
                list.Add(testdata);
            }

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;
        }
        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesReport(ReportSearchModel model, bool callFromAdmin)
        {
            model.RecordsPerPage = 10000000;
             var result = new PagingResult<MeterRechargeApiListingModel>();

            IQueryable<TransactionDetail> query = null;
            if (!model.IsInitialLoad)
                query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
            else
                query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));
 
            if (model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) >= DbFunctions.TruncateTime(model.From)); 
            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) <= DbFunctions.TruncateTime(model.To)); 
            }
            if (model.PosId >0)
            {
                query = query.Where(p => p.POSId == model.PosId); 
            }
            if (!string.IsNullOrEmpty(model.Meter))
            {
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber != null && p.MeterNumber.Contains(model.Meter))); 
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            result.TotalCount = query.Count();


            if (model.SortBy != "VendorName" && model.SortBy != "MeterNumber" && model.SortBy != "POS")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            }
 

            var list = query.ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                ProductShortName = x.Platform?.ShortName ==null ? "" : x.Platform.ShortName,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.TransactionDetailsId,
                RechargeId = x.TransactionDetailsId,
                UserName = x.User?.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
                VendorName = x.POS.User == null ? "" : x.POS.User.Vendor,
                RechargePin = x?.MeterToken1
            }).ToList();
            
            if (model.SortBy == "VendorName" || model.SortBy == "MeterNumber" || model.SortBy == "POS")
            {
                if (model.SortBy == "VendorName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.VendorName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.VendorName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "MeterNumber")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.POSId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.POSId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;
            //var result = new PagingResult<MeterRechargeApiListingModel>();

            //var query = new List<MeterRecharge>(); 

            //if (model.VendorId > 0)
            //{
            //    var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
            //    var posIds = new List<long>();
            //    if (callFromAdmin)
            //    {
            //        posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
            //        query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null).ToList();
            //    } 
            //    else
            //    {
            //        posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
            //        query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null && posIds.Contains(p.POSId.Value)).ToList();
            //    }
            //}
            //else
            //{
            //    query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null).ToList();
            //}

            //if (model.From != null) 
            //    query = query.Where(p => p.CreatedAt.Date >= model.From.Value.Date).ToList();  
            //if (model.To != null) 
            //    query = query.Where(p => p.CreatedAt.Date <= model.To.Value.Date).ToList();  
            //if (model.PosId >0) 
            //    query = query.Where(p => p.POSId == model.PosId).ToList();  
            //if (!string.IsNullOrEmpty(model.Meter)) 
            //    query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber != null && p.MeterNumber.Contains(model.Meter))).ToList(); 
            //if (!string.IsNullOrEmpty(model.TransactionId)) 
            //    query = query.Where(p => p.TransactionId.ToLower().Contains(model.TransactionId.ToLower())).ToList(); 
             
             
            //var list = query.Select(x => new MeterRechargeApiListingModel
            //{
            //    Amount = x.Amount,
            //    ProductShortName = x.Platform.ShortName ==null ? "" : x.Platform.ShortName,
            //    RechargePin =x.MeterToken,
            //    CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),
            //    MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
            //    POSId = x.POSId == null ? "" : x.POS.SerialNumber,
            //    Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
            //    TransactionId = x.TransactionId,
            //    MeterRechargeId = x.MeterRechargeId,
            //    RechargeId = x.MeterRechargeId,
            //    UserName = x.User.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
            //    VendorName = x.POS.User == null ? "" : x.POS.User.Vendor,
            //}).OrderByDescending(w => w.CreatedAt).ToList();
             

            //result.List = (from a in list
            //               orderby a.CreatedAt
            //               descending
            //               select a).ToList();

            ////result.List = list;
            //result.Status = ActionStatus.Successfull;
            //result.Message = "Meter recharges fetched successfully.";
            //return result;
        }

        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesHistory(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null); 
            if (model.VendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = Context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }
            if (model.From != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) >= DbFunctions.TruncateTime(model.From));
            }
            if (model.To != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.CreatedAt) <= DbFunctions.TruncateTime(model.To));
            }
            if (model.PosId > 0)
            {
                query = query.Where(p => p.POSId == model.PosId);
            }
            if (!string.IsNullOrEmpty(model.Meter))
            {
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber != null && p.MeterNumber.Contains(model.Meter)));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            result.TotalCount = query.Count();


            if (model.SortBy != "VendorName" && model.SortBy != "MeterNumber" && model.SortBy != "POS")
            {
                query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            }

            /*var innerJoin = from r in query // outer sequence
                            join p in Context.Platforms //inner sequence 
                            on r.PlatFormId equals p.PlatformId // key selector 
                            select new MeterRechargeApiListingModel
                            { // result selector 
                                
                            };*/


            var list = query.ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                ProductShortName = x.Platform?.ShortName == null ? "" : x.Platform.ShortName,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.TransactionDetailsId,
                RechargeId = x.TransactionDetailsId,
                UserName = x.User?.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
                VendorName = x.POS.User == null ? "" : x.POS.User.Vendor,
            }).ToList();

            if (model.SortBy == "VendorName" || model.SortBy == "MeterNumber" || model.SortBy == "POS")
            {
                if (model.SortBy == "VendorName")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.VendorName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.VendorName).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "MeterNumber")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
                if (model.SortBy == "POS")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.POSId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.POSId).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                }
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result; 
        }

        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRecharges(long userID, int pageNo, int pageSize)
        {
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.UserId == userID && p.Finalised == true);
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.TransactionDetailsId,
                ProductShortName = x.Platform.ShortName == null ? "" : x.Platform.ShortName,
                RechargePin = x.MeterToken1,
                RechargeId = x.TransactionDetailsId
            }).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;
        }
        ActionOutput IMeterManager.RechargeMeter(RechargeMeterModel model)
        {

            var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            if (user == null)
                return ReturnError("User not exist.");
            var pos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId);
            if (pos.Balance == null || pos.Balance.Value < model.Amount)
                return ReturnError("INSUFFICIENT BALANCE FOR THIS TRANSACTION.");
            var dbMeterRecharge = new TransactionDetail
            {
                UserId = model.UserId,
                MeterId = model.MeterId,
                POSId = model.POSId,
                MeterNumber = model.MeterNumber,
                MeterToken1 = Utilities.GetNumbersFromGuid(),
                Amount = model.Amount,
                PlatFormId = 1, /**TODO temporary hard codded */
                TransactionId = Utilities.GetLastMeterRechardeId(),//Utilities.GenerateUniqueId(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = DateTime.UtcNow
            };


            pos.Balance = pos.Balance.Value - model.Amount;
            Context.TransactionDetails.Add(dbMeterRecharge);
            Context.SaveChanges();
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = dbMeterRecharge.TransactionDetailsId;
            obj.Title = "Meter recharged successfully";
            obj.Message = "Your meter has successfully recharged with SLL " + string.Format("{0:N0}", model.Amount) +" PIN: "+dbMeterRecharge.MeterToken1;
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                PushNotification.SendNotification(obj);
            }

            return  ReturnSuccess("Meter recharged successfully.");

            //return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = dbMeterRecharge, Count = retailSchedDetail.NoOfBags });

        }

        async Task<ReceiptModel> IMeterManager.RechargeMeterReturn(RechargeMeterModel model)
        {
            var response = new ReceiptModel { ReceiptStatus = new ReceiptStatus() };
            var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            if (user == null)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "User does not exist";
                return response;
            } 
            var pos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId);
            if (pos.Balance == null || pos.Balance.Value < model.Amount)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
                return response;
            } 
            if(model.MeterId != null)
            {
                var met = Context.Meters.Find(model.MeterId);
                model.MeterNumber = met.Number;
            }
            Platform platf = new Platform();
            if (model.PlatformId == null)
            {
                platf = Context.Platforms.Find(1);
                model.PlatformId = platf.PlatformId;
            }
            else
                platf = Context.Platforms.Find(model.PlatformId);

           

            IceCloudResponse icekloud_response = new IceCloudResponse();
            IcekloudQueryResponse query_response = new IcekloudQueryResponse();
            TransactionDetail db_transaction_detail = new TransactionDetail();
          
         

            model.TransactionId = Convert.ToInt64(Utilities.GetLastMeterRechardeId());
            icekloud_response =  Utilities.Make_recharge_request_from_icekloud(model);

            if (icekloud_response.Content.Data.Error == "Unable to connect to the remote server")
            { 
                query_response = Utilities.Query_vend_status(Utilities.Buid_vend_query_object(model));
                if (!query_response.Content.Finalised)
                {
                    db_transaction_detail = Build_db_transaction_detail_from_Query_response(query_response, model);
                    db_transaction_detail.PlatFormId = platf.PlatformId;
                    db_transaction_detail.Platform = platf;
                    try
                    {
                        Context.TransactionDetails.Add(db_transaction_detail);
                        Context.SaveChanges();
                    }
                    catch (SqlException ex)
                    {
                        response.ReceiptStatus.Status = "unsuccessful";
                        response.ReceiptStatus.Message = ex.ToString();
                        return response;
                    }
                    
                    response.ReceiptStatus.Status = "unsuccessful";
                    response.ReceiptStatus.Message = query_response.Content.StatusDescription;
                    return response;
                }
            }
            else if (icekloud_response.Status.ToLower() != "success")
            {  
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = icekloud_response.Content.Data.Error;
                return response;
            }
        
            var response_data = icekloud_response.Content.Data.DataData.FirstOrDefault();

            
            if (response_data != null)
                db_transaction_detail = Build_db_transaction_detail_from_Icekloud_response(response_data, model);
            else if (query_response.Content.Finalised)
                db_transaction_detail = Build_db_transaction_detail_from_Query_response(query_response, model);
            else
                db_transaction_detail = Build_db_transaction_detail_response(model, user);

            db_transaction_detail.PlatFormId = platf.PlatformId;
            db_transaction_detail.Platform = platf;
            db_transaction_detail.TransactionId = model.TransactionId.ToString();
            try
            {
                pos.Balance = pos.Balance.Value - model.Amount;
                Context.TransactionDetails.Add(db_transaction_detail);
                Context.SaveChanges();

                Push_notification_to_user(user, model, db_transaction_detail.TransactionDetailsId);

                var receipt = Build_receipt_model_from_dbtransaction_detail(db_transaction_detail);
                receipt.ShouldShowSmsButton = (bool)db_transaction_detail.POS.SMSNotificationDeposit;
                return receipt;
            }
            catch (IndexOutOfRangeException e)
            {

                throw e;
            }
            
        }

        ReceiptModel Build_receipt_model_from_dbtransaction_detail(TransactionDetail model)
        {
            var receipt = new ReceiptModel();
            receipt.AccountNo = model?.AccountNumber;
            receipt.POS = model?.POS?.SerialNumber;
            receipt.CustomerName = model?.Customer;
            receipt.ReceiptNo = model?.ReceiptNumber;
            receipt.Address = model?.CustomerAddress;
            receipt.Tarrif = Convert.ToDouble(model?.Tariff);
            receipt.DeviceNumber = model?.MeterNumber;
            receipt.DebitRecovery = 0.00;
            receipt.Amount = Convert.ToDouble(model?.TenderedAmount);
            receipt.Charges = Convert.ToDouble(model?.ServiceCharge);
            receipt.Commission = 0.00;
            receipt.Unit = Convert.ToDouble(model?.Units);
            receipt.UnitCost = 0.00.ToString();
            receipt.SerialNo = model?.SerialNumber;
            receipt.Pin1 = Utilities.FormatThisToken(model?.MeterToken1);
            receipt.Pin2 = Utilities.FormatThisToken(model?.MeterToken2);
            receipt.Pin3 = Utilities.FormatThisToken(model?.MeterToken3);
            receipt.Discount = 0;
            receipt.VendorId = model.UserId.ToString();
            return receipt;
        }

        void Push_notification_to_user(User user, RechargeMeterModel model, long MeterRechargeId)
        {
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = MeterRechargeId;
            obj.Title = "Meter recharged successfully";
            obj.Message = $"Your meter has successfully recharged with SLL { string.Format("{0:N0}", model.Amount) } PIN: {model.MeterToken1}{model.MeterToken2}{model.MeterToken3}";
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                PushNotification.SendNotification(obj);
            }
        }
        TransactionDetail Build_db_transaction_detail_from_Query_response(IcekloudQueryResponse response_data, RechargeMeterModel model)
        {
            var trans = new TransactionDetail();
            trans.UserId = model.UserId;
            trans.MeterId = model.MeterId;
            trans.POSId = model.POSId;
            trans.MeterNumber = model.MeterNumber;
            trans.TransactionDetailsId = (long)model.MeterId;
            trans.MeterToken1 = response_data.Content?.VoucherPin?.ToString()??string.Empty;
            trans.Amount = model.Amount;
            trans.TransactionId = model.TransactionId.ToString();
            trans.IsDeleted = false;
            trans.Status = response_data.Content.Finalised ? (int)RechargeMeterStatusEnum.Success : 0;
            trans.CreatedAt = DateTime.UtcNow;
            trans.AccountNumber = response_data.Content?.CustomerAccNo?.ToString()??string.Empty;
            trans.CurrentDealerBalance = 0;
            trans.Customer = response_data.Content?.Customer?.ToString()??string.Empty;
            trans.ReceiptNumber = response_data.Content?.VoucherSerialNumber?.ToString()??string.Empty;
            trans.RequestDate = Convert.ToDateTime(response_data.Content.DateAndTimeCreated.Date).Date;
            trans.RTSUniqueID = 00;
            trans.SerialNumber = response_data.Content?.SerialNumber.ToString()??string.Empty;
            trans.ServiceCharge = response_data.Content?.ServiceCharge?.ToString()??string.Empty;
            trans.Tariff = response_data.Content?.Tariff?.ToString()??string.Empty;
            trans.TaxCharge = Convert.ToDecimal(response_data.Content.TaxCharge);
            trans.TenderedAmount = Convert.ToDecimal(response_data.Content.Denomination);
            trans.TransactionAmount = Convert.ToDecimal(response_data.Content.Denomination);
            trans.Units = Convert.ToDecimal(response_data.Content.Units);
            trans.VProvider = response_data?.Content?.Provider?.ToString() ?? string.Empty;
            trans.Finalised = response_data?.Content?.Finalised;
            trans.StatusRequestCount = Convert.ToInt16(response_data?.Content?.StatusRequestCount);
            trans.Sold = response_data.Content.Sold;
            //trans.DateAndTimeSold =  response_data.Content?.DateAndTimeSold?.ToString();
            //trans.DateAndTimeFinalised =  response_data.Content?.DateAndTimeFinalised?.ToString();
            //trans.DateAndTimeLinked = response_data.Content?.DateAndTimeLinked?.ToString();
            trans.VoucherSerialNumber = response_data.Content?.VoucherSerialNumber?.ToString();
            trans.VendStatus = response_data.Content?.Status?.ToString();
            trans.VendStatusDescription = response_data.Content?.StatusDescription?.ToString();
            return trans;
        }

        TransactionDetail Build_db_transaction_detail_from_Icekloud_response(Datum response_data, RechargeMeterModel model)
        {
            return new TransactionDetail
            {
                UserId = model.UserId,
                MeterId = model.MeterId,
                POSId = model.POSId,
                MeterNumber = model.MeterNumber,
                TransactionDetailsId = (long)model.MeterId,
                MeterToken1 = response_data.PinNumber,
                MeterToken2 = response_data.PinNumber2,
                MeterToken3 = response_data.PinNumber3,
                Amount = model.Amount, 
                TransactionId = model.TransactionId.ToString(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = DateTime.UtcNow,
                AccountNumber = response_data.PowerHubVoucher.AccountNumber,
                CurrentDealerBalance = response_data.DealerBalance,
                Customer = response_data.PowerHubVoucher.Customer,
                ReceiptNumber = response_data.PowerHubVoucher.ReceiptNumber,
                RequestDate = Convert.ToDateTime(response_data.DateAndTime),
                RTSUniqueID = response_data.PowerHubVoucher.RtsUniqueId,
                SerialNumber = response_data.SerialNumber,
                ServiceCharge = response_data.PowerHubVoucher.ServiceCharge.ToString(),
                Tariff = response_data.PowerHubVoucher.Tariff.ToString(),
                TaxCharge = Convert.ToDecimal(response_data.PowerHubVoucher.TaxCharge),
                TenderedAmount = response_data.PowerHubVoucher.TenderedAmount,
                TransactionAmount = response_data.PowerHubVoucher.TransactionAmount,
                Units = response_data.PowerHubVoucher.Units,

            };
        } 
        TransactionDetail Build_db_transaction_detail_response(RechargeMeterModel model, User user)
        {
            return new TransactionDetail
            {
                UserId = user.UserId,
                MeterId = model.MeterId,
                POSId = model.POSId, 
                MeterNumber = model.MeterNumber,
                TransactionDetailsId = (long)model.MeterId,
                MeterToken1 = Utilities.GetNumbersFromGuid(),
                MeterToken2 = string.Empty,
                MeterToken3 = string.Empty,
                Amount = model.Amount,
                TransactionId = model.TransactionId.ToString(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = DateTime.UtcNow,
                AccountNumber = string.Empty,
                CurrentDealerBalance = 0,
                Customer = user.Name + " " + user.SurName,
                ReceiptNumber = Utilities.GetUniqueKey(),
                RequestDate = DateTime.UtcNow,
                RTSUniqueID = 11111,
                SerialNumber = model.TransactionId.ToString().PadLeft(2, '0'),
                ServiceCharge = 0.ToString(),
                Tariff = 0.ToString(),
                TaxCharge = 0,
                TenderedAmount = model.Amount,
                TransactionAmount = model.Amount,
                Units = 0,
                CostOfUnits = 0,
                CustomerAddress = user.Address,
                DebitRecovery = 0, 
            };
        }

        ActionOutput<MeterRechargeApiListingModel> IMeterManager.GetRechargeDetail(long rechargeId)
        {
            var recharge = Context.TransactionDetails.FirstOrDefault(p => p.TransactionDetailsId == rechargeId);
            if (recharge == null)
                return ReturnError<MeterRechargeApiListingModel>("Recharge not exist.");
            var data = new MeterRechargeApiListingModel();
            data.Amount = recharge.Amount;
            data.CreatedAt = recharge.CreatedAt.ToString();
            data.MeterNumber = recharge.Meter == null ? recharge.MeterNumber : recharge.Meter.Number;
            data.Status = ((RechargeMeterStatusEnum)recharge.Status).ToString();
            data.RechargeId = recharge.TransactionDetailsId;
            data.VendorName = recharge.POS == null || recharge.POS.User == null ? "" : recharge.POS.User.Vendor;
            data.VendorId = recharge.POS == null || recharge.POS.User == null ? 0 : recharge.POS.VendorId.Value;
            data.RechargePin = recharge.MeterToken1;
            data.TransactionId = recharge.TransactionId;
            return ReturnSuccess<MeterRechargeApiListingModel>(data, "Recharge detail fetched successfully.");

        }

        RechargeDetailPDFData IMeterManager.GetRechargePDFData(long rechargeId)
        {
            var recharge = Context.TransactionDetails.FirstOrDefault(p => p.TransactionDetailsId == rechargeId);
            if (recharge == null)
                return null;
            return new RechargeDetailPDFData
            {
                Amount = recharge.Amount,
                CreatedAt = recharge.CreatedAt.ToString(),
                MeterNumber = recharge.Meter.Number,
                Status = ((RechargeMeterStatusEnum)recharge.Status).ToString(),
                TransactionId = recharge.TransactionId,
                UserName = recharge.User.Name + " " + recharge.User.SurName

            };
        }
         
         
    }

     

}
