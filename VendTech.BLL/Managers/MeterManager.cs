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

            var query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null);
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
                PRODUCT_TYPE = x.Platform.ShortName,
                PIN = x.MeterToken,
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
            if (list.Count == 0)
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
            // var query =  Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null);
            IQueryable<MeterRecharge> query = null;
            if (model.From != null)
                query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null);
            else
                query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

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

            /*var innerJoin = from r in query // outer sequence
                            join p in Context.Platforms //inner sequence 
                            on r.PlatFormId equals p.PlatformId // key selector 
                            select new MeterRechargeApiListingModel
                            { // result selector 
                                
                            };*/


            var list = query.ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                ProductShortName = x.Platform.ShortName ==null ? "" : x.Platform.ShortName,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.MeterRechargeId,
                RechargeId = x.MeterRechargeId,
                UserName = x.User.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
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
            var query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.POSId != null); 
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
                ProductShortName = x.Platform.ShortName == null ? "" : x.Platform.ShortName,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.MeterRechargeId,
                RechargeId = x.MeterRechargeId,
                UserName = x.User.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
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
            var query = Context.MeterRecharges.Where(p => !p.IsDeleted && p.UserId == userID);
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber : x.Meter.Number,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                MeterRechargeId = x.MeterRechargeId,
                ProductShortName = x.Platform.ShortName == null ? "" : x.Platform.ShortName,
                RechargePin = x.MeterToken,
                RechargeId = x.MeterRechargeId
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
            var dbMeterRecharge = new MeterRecharge
            {
                UserId = model.UserId,
                MeterId = model.MeterId,
                POSId = model.POSId,
                MeterNumber = model.MeterNumber,
                MeterToken = Utilities.GetNumbersFromGuid(),
                Amount = model.Amount,
                PlatFormId = 1, /**TODO temporary hard codded */
                TransactionId = Utilities.GetLastMeterRechardeId(),//Utilities.GenerateUniqueId(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = DateTime.UtcNow
            };

            pos.Balance = pos.Balance.Value - model.Amount;
            Context.MeterRecharges.Add(dbMeterRecharge);
            Context.SaveChanges();
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = dbMeterRecharge.MeterRechargeId;
            obj.Title = "Meter recharged successfully";
            obj.Message = "Your meter has successfully recharged with SLL " + string.Format("{0:N0}", model.Amount) +" PIN: "+dbMeterRecharge.MeterToken;
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

        ReceiptModel IMeterManager.RechargeMeterReturn(RechargeMeterModel model)
        { 
            var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            if (user == null)
                return null; // ReturnError<RechargeMeterModel>("User not exist.");
            var pos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId);
            if (pos.Balance == null || pos.Balance.Value < model.Amount)
                return null;// ReturnError<RechargeMeterModel>("INSUFFICIENT BALANCE FOR THIS TRANSACTION.");
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
            {
                platf = Context.Platforms.Find(model.PlatformId);
            }
            var dbMeterRecharge = new MeterRecharge
            {
                UserId = model.UserId,
                MeterId = model.MeterId,
                POSId = model.POSId,
                MeterNumber = model.MeterNumber,
                MeterRechargeId = Convert.ToInt64(model.MeterId),
                MeterToken = Utilities.GetNumbersFromGuid(),
                Amount = model.Amount,
                PlatFormId =platf.PlatformId,
                Platform = platf,
                TransactionId = Utilities.GetLastMeterRechardeId(),//Utilities.GenerateUniqueId(),
                IsDeleted = false,
                Status = (int)RechargeMeterStatusEnum.Success,
                CreatedAt = DateTime.UtcNow
            };

            pos.Balance = pos.Balance.Value - model.Amount;
            Context.MeterRecharges.Add(dbMeterRecharge);
            try
            {
                Context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            { 
                throw e;
            }
            try
            {
                var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
                var obj = new PushNotificationModel();
                obj.UserId = model.UserId;
                obj.Id = dbMeterRecharge.MeterRechargeId;
                obj.Title = "Meter recharged successfully";
                obj.Message = "Your meter has successfully recharged with SLL " + string.Format("{0:N0}", model.Amount) + " PIN: " + dbMeterRecharge.MeterToken;
                obj.NotificationType = NotificationTypeEnum.MeterRecharge;
                foreach (var item in deviceTokens)
                {
                    obj.DeviceToken = item.DeviceToken;
                    obj.DeviceType = item.AppType.Value;
                    PushNotification.SendNotification(obj);
                }
                model.MeterToken = dbMeterRecharge.MeterToken;
                var receipt = new ReceiptModel();

                receipt.AccountNo = user.UserId.ToString();
                receipt.CustomerName = user.SurName + " " + user.Name;
                receipt.ReceiptNo = dbMeterRecharge.TransactionId;
                receipt.Address = user.Address;
                receipt.DeviceNumber = dbMeterRecharge.MeterNumber;

                if (dbMeterRecharge.MeterToken != null && dbMeterRecharge.MeterToken.Length >= 2 && dbMeterRecharge.MeterToken.Length <= 12)
                    receipt.RechargeToken = dbMeterRecharge.MeterToken.Insert(4, " ").Insert(9, " "); 
                else if (dbMeterRecharge.MeterToken != null && dbMeterRecharge.MeterToken.Length >= 12 && dbMeterRecharge.MeterToken.Length <= 16)
                    receipt.RechargeToken = dbMeterRecharge.MeterToken.Insert(4, " ").Insert(9, " ").Insert(14, " "); 
                else if (dbMeterRecharge.MeterToken != null && dbMeterRecharge.MeterToken.Length >= 16 && dbMeterRecharge.MeterToken.Length <= 21)
                    receipt.RechargeToken = dbMeterRecharge.MeterToken.Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " ");
                else 
                    receipt.RechargeToken = dbMeterRecharge.MeterToken; 

                //receipt.RechargeToken = dbMeterRecharge.MeterToken.Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " ").Insert(24, " ");
                receipt.Amount = Convert.ToDouble(dbMeterRecharge.Amount);
                receipt.Charges = 0;
                receipt.Commission = 0;
                receipt.Unit = 56;
                receipt.UnitCost = 100;
                receipt.TerminalID = "0000000000007";
                receipt.SerialNo = dbMeterRecharge.MeterRechargeId.ToString();

                return receipt;
            }
            catch (IndexOutOfRangeException e)
            {

                throw e;
            }
           
            //return ReturnSuccess(model, "Deposit detail fetched successfully.");
        }

        

        ActionOutput<MeterRechargeApiListingModel> IMeterManager.GetRechargeDetail(long rechargeId)
        {
            var recharge = Context.MeterRecharges.FirstOrDefault(p => p.MeterRechargeId == rechargeId);
            if (recharge == null)
                return ReturnError<MeterRechargeApiListingModel>("Recharge not exist.");
            var data = new MeterRechargeApiListingModel();
            data.Amount = recharge.Amount;
            data.CreatedAt = recharge.CreatedAt.ToString();
            data.MeterNumber = recharge.Meter == null ? recharge.MeterNumber : recharge.Meter.Number;
            data.Status = ((RechargeMeterStatusEnum)recharge.Status).ToString();
            data.RechargeId = recharge.MeterRechargeId;
            data.VendorName = recharge.POS == null || recharge.POS.User == null ? "" : recharge.POS.User.Vendor;
            data.VendorId = recharge.POS == null || recharge.POS.User == null ? 0 : recharge.POS.VendorId.Value;
            data.RechargePin = recharge.MeterToken;
            data.TransactionId = recharge.TransactionId;
            return ReturnSuccess<MeterRechargeApiListingModel>(data, "Recharge detail fetched successfully.");

        }

        RechargeDetailPDFData IMeterManager.GetRechargePDFData(long rechargeId)
        {
            var recharge = Context.MeterRecharges.FirstOrDefault(p => p.MeterRechargeId == rechargeId);
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
