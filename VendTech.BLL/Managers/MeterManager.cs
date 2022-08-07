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
using System.Web.Configuration;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Data.Entity.SqlServer;

namespace VendTech.BLL.Managers
{
    public class MeterManager : BaseManager, IMeterManager
    {

        ActionOutput IMeterManager.SaveMeter(MeterModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId && p.IsDeleted == false);
                if (dbMeter == null)
                    return ReturnError("Meter not exist.");
            }
            else
            {
                var met = Context.Meters.FirstOrDefault(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId && p.IsDeleted == false);
                if (met != null)
                    return ReturnError(met.MeterId, "Same meter number already exist for you.");
            }
            dbMeter.Name = model.Name;
            dbMeter.Number = model.Number;
            dbMeter.MeterMake = model.MeterMake;
            dbMeter.Address = model.Address;
            dbMeter.Allias = model.Allias;
            dbMeter.IsSaved = model.IsSaved;
            dbMeter.IsVerified = model.isVerified;
            if (model.MeterId == 0)
            {
                dbMeter.UserId = model.UserId;
                dbMeter.CreatedAt = DateTime.UtcNow;
                Context.Meters.Add(dbMeter);
            }
            Context.SaveChanges();
            return ReturnSuccess(dbMeter.MeterId, "Meter details saved successfully.");
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
            return Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsSaved == true && p.IsVerified == true)
                .Select(p => new SelectListItem
                {
                    Text = p.Number + " - " + p.Allias ?? string.Empty,
                    Value = p.MeterId.ToString()
                }).ToList();
        }

        PagingResult<MeterAPIListingModel> IMeterManager.GetMeters(long userID, int pageNo, int pageSize, bool isActive)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsSaved == true && p.IsVerified == isActive).ToList();
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterAPIListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meters fetched successfully.";
            return result;
        }
        PagingResult<SalesReportExcelModel> IMeterManager.GetSalesExcelReportData(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            var result = new PagingResult<SalesReportExcelModel>();

            var query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
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
                    // posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == model.AgencyId).Select(p => p.POSId).ToList();
                    posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }
            Console.WriteLine(query);
            //if(model.AgencyId > 0)
            //{
            //    query = query.Where(p => p.User.AgentId == agentId);
            //}
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
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber1 != null && p.MeterNumber1.Contains(model.Meter)));

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
                METER_NO = x.Meter == null ? x.MeterNumber1 : x.Meter.Number,
                VENDORNAME = x.POS.User == null ? "" : x.POS.User.Vendor,
                POSID = x.POSId == null ? "" : x.POS.SerialNumber,
                //Request = x?.Request,
                //Response = x?.Response
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
        
        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesReportAsync(ReportSearchModel model, bool callFromAdmin, long agentId)
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
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber1 != null && p.MeterNumber1.Contains(model.Meter)));
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

            var list = query.ToList().Select( x => new MeterRechargeApiListingModel(x, 1)).ToList();

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

        PagingResult<GSTRechargeApiListingModel> IMeterManager.GetUserGSTRechargesReport(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 10000000;
            var result = new PagingResult<GSTRechargeApiListingModel>();

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
                query = query.Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber1 != null && p.MeterNumber1.Contains(model.Meter)));
            }
            if (!string.IsNullOrEmpty(model.TransactionId))
            {
                query = query.Where(p => p.TransactionId.ToLower().Contains(model.TransactionId.ToLower()));
            }
            result.TotalCount = query.Count();


            //if (model.SortBy != "VendorName" && model.SortBy != "MeterNumber" && model.SortBy != "POS")
            //{
            //    query = query.OrderBy(model.SortBy + " " + model.SortOrder).Skip((model.PageNo - 1)).Take(model.RecordsPerPage);
            //}

            var list = query.ToList().Select(x => new GSTRechargeApiListingModel(x)).ToList();

            if (model.SortBy == "VendorName" || model.SortBy == "MeterNumber" || model.SortBy == "POS")
            { 
                if (model.SortBy == "MeterNumber")
                {
                    if (model.SortOrder == "Asc")
                        list = list.OrderBy(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                    else
                        list = list.OrderByDescending(p => p.MeterNumber).Skip((model.PageNo - 1)).Take(model.RecordsPerPage).ToList();
                } 
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;

        }
        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesHistory(ReportSearchModel model, bool callFromAdmin)
        {
            if(model.RecordsPerPage != 20)
            {
                model.RecordsPerPage = 10;
            }
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = Context.TransactionDetails.OrderByDescending(d => d.CreatedAt).Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null);
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


            var list = query.Take(model.RecordsPerPage).ToList().Select(x => new MeterRechargeApiListingModel(x)).ToList();

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
                MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number,
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
                MeterNumber1 = model.MeterNumber,
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
            obj.Message = "Your meter has successfully recharged with SLL " + string.Format("{0:N0}", model.Amount) + " PIN: " + dbMeterRecharge.MeterToken1;
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                PushNotification.SendNotification(obj);
            }

            return ReturnSuccess("Meter recharged successfully.");

            //return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = dbMeterRecharge, Count = retailSchedDetail.NoOfBags });

        }

        async Task<ReceiptModel> IMeterManager.RechargeMeterReturn(RechargeMeterModel model)
        { 
            var response = new ReceiptModel { ReceiptStatus = new ReceiptStatus() };

            Platform platf = new Platform();
            if (model.PlatformId == null)
            {
                platf = Context.Platforms.Find(1);
                model.PlatformId = platf.PlatformId;
            }
            else
                platf = Context.Platforms.Find(model.PlatformId);

            if (platf.DisablePlatform)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = platf.DisabledPlatformMessage;
                return response;
            }  

            var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            if (user == null)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "User does not exist";
                return response;
            }
            var pos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId); 

            if (pos == null)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "POS NOT FOUND!! Please Contact Administrator.";
                return response;
            }
            if (pos.Balance == null)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
                return response;
            }


            if (model.Amount > pos.Balance || pos.Balance.Value < model.Amount)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
                return response;
            }
 
            if (model.MeterId != null)
            {
                var met = Context.Meters.Find(model.MeterId);
                model.MeterNumber = met.Number;
            }
            else
            {
                model.IsSaved = false;
            }
            

            IceKloudResponse icekloud_response = new IceKloudResponse();
            IcekloudQueryResponse query_response = new IcekloudQueryResponse();
            TransactionDetail db_transaction_detail = new TransactionDetail();

            model.TransactionId = Convert.ToInt64(Utilities.GetLastMeterRechardeId());
            icekloud_response = Make_recharge_request_from_icekloud(model);

            var vend_request = JsonConvert.SerializeObject(icekloud_response.RequestModel);
            var vend_response = JsonConvert.SerializeObject(icekloud_response);
            var response_data = icekloud_response.Content.Data.Data.FirstOrDefault();

            if (icekloud_response.Content.Data.Error == "Unable to connect to the remote server")
            {
                var query_request = Buid_vend_query_object(model);
                query_response = Query_vend_status(query_request);
                if (!query_response.Content.Finalised)
                {
                    db_transaction_detail = Build_db_transaction_detail_from_Query_response(query_response, model);
                    db_transaction_detail.PlatFormId = platf.PlatformId;
                    db_transaction_detail.Platform = platf;

                    Context.TransactionDetails.Add(db_transaction_detail);
                    Context.SaveChanges();

                    response.ReceiptStatus.Status = "unsuccessful";
                    response.ReceiptStatus.Message = query_response.Content.StatusDescription;
                    return response;
                }
            }
            else if (icekloud_response.Status.ToLower() != "success")
            {
                //Will save to a different table
                db_transaction_detail = Build_db_transaction_detail_from_FAILED_response(icekloud_response, model);
                db_transaction_detail.PlatFormId = platf.PlatformId;
                db_transaction_detail.Platform = platf;
                Context.TransactionDetails.Add(db_transaction_detail);
                
                SaveSales();

                response.ReceiptStatus.Status = "unsuccessful";
                if("Input string was not in a correct format." == icekloud_response.Content.Data?.Error)
                {
                    response.ReceiptStatus.Message = "Amount tendered is too low";
                    return response;
                }
                response.ReceiptStatus.Message = icekloud_response.Content.Data?.Error;
                return response;
            }


            if (response_data != null)
            {
                db_transaction_detail = Build_db_transaction_detail_from_Icekloud_response(response_data, model, vend_request, vend_response);
            }
            else if (query_response.Content.Finalised)
            {
                db_transaction_detail = Build_db_transaction_detail_from_Query_response(query_response, model);
            }

            db_transaction_detail.PlatFormId = platf.PlatformId;
            db_transaction_detail.Platform = platf;
            db_transaction_detail.TransactionId = model.TransactionId.ToString();
            try
            {
                MeterModel newMeter = new MeterModel();
                long meterId = 0;
                if (model.SaveAsNewMeter)
                {
                    newMeter = StackNewMeterToDbObject(model);
                    newMeter.IsSaved = true;
                    meterId = (this as IMeterManager).SaveMeter(newMeter).ID;
                    db_transaction_detail.MeterId = meterId != 0 ? meterId : 0;

                }
                else
                {
                    db_transaction_detail.MeterId = model.MeterId;
                }

                pos.Balance = (pos.Balance - model.Amount);
                db_transaction_detail.CurrentVendorBalance = pos.Balance ?? 0;
                Context.TransactionDetails.Add(db_transaction_detail);
                SaveSales();

                Push_notification_to_user(user, model, db_transaction_detail.TransactionDetailsId);

                var receipt = Build_receipt_model_from_dbtransaction_detail(db_transaction_detail);
                receipt.ShouldShowSmsButton = (bool)db_transaction_detail.POS.WebSms;
                receipt.ShouldShowPrintButton = (bool)db_transaction_detail.POS.WebPrint;
                receipt.mobileShowSmsButton = (bool)db_transaction_detail.POS.PosSms;
                receipt.mobileShowPrintButton = (bool)db_transaction_detail.POS.PosPrint;
                receipt.CurrentBallance = db_transaction_detail?.POS?.Balance??0;
                return receipt;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void SaveSales()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
        }

        private MeterModel StackNewMeterToDbObject(RechargeMeterModel model)
        { 
            return new MeterModel
            {
                Address = "",
                Allias = "",
                isVerified = false,
                MeterId = 0,
                MeterMake = "",
                Name = "",
                UserId = model.UserId,
                Number = model.MeterNumber,
                IsSaved = model.IsSaved
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
            data.MeterNumber = recharge.Meter == null ? recharge.MeterNumber1 : recharge.Meter.Number;
            data.Status = ((RechargeMeterStatusEnum)recharge.Status).ToString();
            data.RechargeId = recharge.TransactionDetailsId;
            data.VendorName = recharge.POS == null || recharge.POS.User == null ? "" : recharge.POS.User.Vendor;
            data.VendorId = recharge.POS == null || recharge.POS.User == null ? 0 : recharge.POS.VendorId.Value;
            data.RechargePin = Utilities.FormatThisToken(recharge.MeterToken1);
            data.TransactionId = recharge.TransactionId;
            data.MeterId = recharge.MeterId;
            data.POSId = recharge.POS == null ? "" : recharge.POS.SerialNumber;
            var thisTransactionNotification = Context.Notifications.FirstOrDefault(d => d.Type == (int)NotificationTypeEnum.MeterRecharge && d.RowId == rechargeId);
            if(thisTransactionNotification != null)
            {
                thisTransactionNotification.MarkAsRead = true;
                Context.SaveChanges();
            }
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

        private static IceKloudResponse Make_recharge_request_from_icekloud(RechargeMeterModel model)
        {
            IceKloudResponse response = new IceKloudResponse();
            string strings_result = string.Empty;
            IcekloudRequestmodel request_model = new IcekloudRequestmodel();
            HttpResponseMessage icekloud_response = new HttpResponseMessage();
            HttpClient _http_client = new HttpClient();
            string url = (WebConfigurationManager.AppSettings["IsDevelopment"].ToString() == "1") ?
                         WebConfigurationManager.AppSettings["DevIcekloudURL"].ToString() :
                         WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

            try
            {
                request_model = Buid_new_request_object(model);

                icekloud_response = _http_client.PostAsJsonAsync(url, request_model).Result;

                strings_result = icekloud_response.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<IceKloudResponse>(strings_result);
                response.RequestModel = request_model;
                return response;
            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    Debug.WriteLine(errInner);
                }
                throw new AggregateException();
            }
            catch (Exception)
            {
                try
                {
                    IceCloudErorResponse error_response = JsonConvert.DeserializeObject<IceCloudErorResponse>(strings_result);

                    if (error_response.Status == "Error")
                    {
                        if (error_response.SystemError.ToLower() == "Unable to connect to the remote server".ToLower())
                        {
                            response.Status = "unsuccesful";
                            response.Content.Data.Error = error_response.SystemError;
                            response.RequestModel = request_model;
                            return response;
                        }
                        if (error_response.SystemError.ToLower() == "The specified TransactionID already exists for this terminal.".ToLower())
                        {
                            model.TransactionId = model.TransactionId + 1;
                            return Make_recharge_request_from_icekloud(model);
                        }

                        
                        response.Status = error_response?.Status;
                        response.Content.Data.Error = error_response?.Stack.ToArray()[0]?.Detail ?? error_response?.SystemError;
                        response.RequestModel = request_model;
                        return response;
                    }
                }
                catch (Exception e) { throw e; }
                throw;
            }
        }
        private static IcekloudRequestmodel Buid_new_request_object(RechargeMeterModel model)
        {
            var username = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString();
            var password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString();
            return new IcekloudRequestmodel
            {
                Auth = new IcekloudAuth
                {
                    Password = password,
                    UserName = username
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                                     {
                        new
                        {
                            UserName = username,
                            Password = password,
                            System = "SL"
                        }, "apiV1_VendVoucher", "webapp", "0", "EDSA", $"{model.Amount}", $"{model.MeterNumber}", -1, "ver1.5", model.TransactionId
                       },
            };
        }
        private static IcekloudQueryResponse Query_vend_status(IcekloudRequestmodel model)
        {
            IcekloudQueryResponse response = new IcekloudQueryResponse();
            string strings_result = string.Empty;
            HttpResponseMessage icekloud_response = new HttpResponseMessage();
            HttpClient _http_client = new HttpClient();
            var url = WebConfigurationManager.AppSettings["IcekloudURL"].ToString();
            try
            {
                icekloud_response = _http_client.PostAsJsonAsync(url, model).Result;
                strings_result = icekloud_response.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<IcekloudQueryResponse>(strings_result);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static IcekloudRequestmodel Buid_vend_query_object(RechargeMeterModel model)
        {
            var username = WebConfigurationManager.AppSettings["IcekloudUsername"].ToString();
            var password = WebConfigurationManager.AppSettings["IcekloudPassword"].ToString();
            return new IcekloudRequestmodel
            {
                Auth = new IcekloudAuth
                {
                    Password = password,
                    UserName = username
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                       {
                           new {
                                UserName = username,
                                Password = password,
                                System = "ATB"
                            }, "apiV1_GetTransactionStatus",  model.TransactionId
                       },
            };
        }
        private ReceiptModel Build_receipt_model_from_dbtransaction_detail(TransactionDetail model)
        {
            var receipt = new ReceiptModel();
            receipt.AccountNo = model?.AccountNumber;
            receipt.POS = model?.POS?.SerialNumber;
            receipt.CustomerName = model?.Customer;
            receipt.ReceiptNo = model?.ReceiptNumber;
            receipt.Address = model?.CustomerAddress;
            receipt.Tarrif = Utilities.FormatAmount(Convert.ToDecimal(model.Tariff));
            receipt.DeviceNumber = model?.MeterNumber1;
            receipt.DebitRecovery = Convert.ToDecimal(model.DebitRecovery);
            var amt = model?.TenderedAmount.ToString("N");
            receipt.Amount = amt.Contains('.') ? amt.TrimEnd('0').TrimEnd('.') : amt;
            receipt.Charges = Utilities.FormatAmount(Convert.ToDecimal(model.ServiceCharge));
            receipt.Commission = string.Format("{0:N0}", 0.00);
            receipt.Unit = Utilities.FormatAmount(Convert.ToDecimal(model.Units));
            receipt.UnitCost = Utilities.FormatAmount(Convert.ToDecimal(model.CostOfUnits));
            receipt.SerialNo = model?.SerialNumber;
            receipt.Pin1 = Utilities.FormatThisToken(model?.MeterToken1) ?? string.Empty;
            receipt.Pin2 = Utilities.FormatThisToken(model?.MeterToken2) ?? string.Empty;
            receipt.Pin3 = Utilities.FormatThisToken(model?.MeterToken3) ?? string.Empty;
            receipt.Discount = string.Format("{0:N0}", 0);
            receipt.Tax = Utilities.FormatAmount(Convert.ToDecimal(model.TaxCharge));
            receipt.TransactionDate = model.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            receipt.VendorId = model.User.Vendor;
            receipt.EDSASerial = model.SerialNumber;
            receipt.VTECHSerial = model.TransactionId;

            return receipt;
        }
        private void Push_notification_to_user(User user, RechargeMeterModel model, long MeterRechargeId)
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
        private TransactionDetail Build_db_transaction_detail_from_Query_response(IcekloudQueryResponse response_data, RechargeMeterModel model)
        {
            try
            {
                var trans = new TransactionDetail();
                trans.UserId = model.UserId;
                trans.MeterId = model.MeterId;
                trans.POSId = model.POSId;
                trans.MeterNumber1 = model.MeterNumber;
                trans.TransactionDetailsId = (long)model.MeterId;
                trans.MeterToken1 = response_data?.Content?.VoucherPin?.ToString() ?? string.Empty;
                trans.Amount = model?.Amount ?? new decimal();
                trans.TransactionId = response_data?.Content?.TransactionId.ToString(); //model?.TransactionId.ToString();
                trans.IsDeleted = false;
                trans.Status = response_data.Content.Finalised ? (int)RechargeMeterStatusEnum.Success : 0;
                trans.CreatedAt = DateTime.UtcNow;
                trans.AccountNumber = response_data.Content?.CustomerAccNo?.ToString() ?? string.Empty;
                trans.CurrentDealerBalance = 00;
                trans.Customer = response_data.Content?.Customer?.ToString() ?? string.Empty;
                trans.ReceiptNumber = response_data.Content?.VoucherSerialNumber?.ToString() ?? string.Empty;
                trans.RequestDate = Convert.ToDateTime(response_data?.Content?.DateAndTimeCreated.Date).Date;
                trans.RTSUniqueID = "00";
                trans.SerialNumber = response_data?.Content?.SerialNumber.ToString() ?? string.Empty;
                trans.ServiceCharge = response_data?.Content?.ServiceCharge.ToString();
                trans.Tariff = response_data.Content?.Tariff.ToString();
                trans.TaxCharge = response_data?.Content?.TaxCharge.ToString();
                trans.TenderedAmount = Convert.ToDecimal(response_data?.Content?.Denomination);
                trans.TransactionAmount = Convert.ToDecimal(response_data?.Content?.Denomination);
                trans.Units = response_data?.Content?.Units.ToString();
                trans.VProvider = response_data?.Content?.Provider?.ToString() ?? string.Empty;
                trans.Finalised = response_data?.Content?.Finalised;
                trans.StatusRequestCount = Convert.ToInt16(response_data?.Content?.StatusRequestCount);
                trans.Sold = response_data?.Content?.Sold;
                trans.DateAndTimeSold = response_data.Content?.DateAndTimeSold?.ToString();
                trans.DateAndTimeFinalised = response_data?.Content?.DateAndTimeFinalised?.ToString();
                trans.DateAndTimeLinked = response_data?.Content?.DateAndTimeLinked?.ToString();
                trans.VoucherSerialNumber = response_data?.Content?.VoucherSerialNumber?.ToString();
                trans.VendStatus = response_data.Content?.Status?.ToString();
                trans.VendStatusDescription = response_data?.Content?.StatusDescription?.ToString();
                trans.StatusResponse = JsonConvert.SerializeObject(response_data);
                trans.DebitRecovery = "0";
                return trans;
            }
            catch (Exception e)
            {

                throw;
            }

        }
        private TransactionDetail Build_db_transaction_detail_from_Icekloud_response(Datum response_data, RechargeMeterModel model, string vend_request, string vend_response)
        {
            try
            {
                var tran = new TransactionDetail();
                tran.UserId = model.UserId;
                tran.MeterId = model.MeterId != null ? model.MeterId : 0;
                tran.POSId = model.POSId;
                tran.MeterNumber1 = model?.MeterNumber;
                tran.TransactionDetailsId = model.MeterId != null ? (long)model.MeterId : 0;
                tran.MeterToken1 = response_data != null ? response_data?.PinNumber : string.Empty;
                tran.MeterToken2 = response_data != null ? response_data?.PinNumber2 : string.Empty;
                tran.MeterToken3 = response_data != null ? response_data?.PinNumber3 : string.Empty;
                tran.Amount = model.Amount;
                tran.TransactionId = model.TransactionId.ToString();
                tran.IsDeleted = false;
                tran.Status = (int)RechargeMeterStatusEnum.Success;
                tran.CreatedAt = DateTime.UtcNow;
                tran.AccountNumber = response_data != null ? response_data?.PowerHubVoucher?.AccountNumber : string.Empty;
                tran.CurrentDealerBalance = response_data != null ? (decimal)response_data?.DealerBalance : new decimal();
                tran.Customer = response_data != null ? response_data?.PowerHubVoucher?.Customer : string.Empty;
                tran.ReceiptNumber = response_data != null ? response_data?.PowerHubVoucher?.ReceiptNumber.ToString() : string.Empty;
                tran.RequestDate = response_data != null ? Convert.ToDateTime(response_data?.DateAndTime) : DateTime.UtcNow;
                tran.RTSUniqueID = response_data != null ? response_data?.PowerHubVoucher?.RtsUniqueId : "";
                tran.SerialNumber = response_data != null ? response_data?.SerialNumber.ToString() : string.Empty;
                tran.ServiceCharge = response_data != null ? response_data?.PowerHubVoucher?.ServiceCharge : "0";
                tran.Tariff = response_data != null ? response_data?.PowerHubVoucher?.Tariff.ToString():  "0";
                tran.TaxCharge = response_data != null ? response_data?.PowerHubVoucher?.TaxCharge : "0";
                tran.TenderedAmount = response_data != null ? Convert.ToDecimal(response_data?.PowerHubVoucher?.TenderedAmount) : 0;
                tran.TransactionAmount = response_data != null ? Convert.ToDecimal(response_data?.PowerHubVoucher?.TransactionAmount) : new decimal();
                var units = response_data != null ? response_data?.PowerHubVoucher?.Units.ToString() : "0";
                tran.Units = units;
                var costofunits = response_data != null ? response_data?.PowerHubVoucher?.CostOfUnits : string.Empty;
                tran.CostOfUnits = costofunits != null? costofunits: "0";
                tran.CustomerAddress = response_data != null ? response_data?.PowerHubVoucher?.CustAddress?.ToString() : string.Empty;
                tran.PlatFormId = Convert.ToInt16(model.PlatformId);
                tran.Request = vend_request;
                tran.Response = vend_response;
                tran.Finalised = true;
                tran.DebitRecovery = response_data != null ? response_data?.PowerHubVoucher?.DebtRecoveryAmt.ToString() ?? "0" : "0";
                return tran;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private TransactionDetail Build_db_transaction_detail_from_FAILED_response(IceKloudResponse response_data, RechargeMeterModel model)
        {
            try
            {
                var trans = new TransactionDetail();
                trans.UserId = model.UserId;
                trans.MeterId = model.MeterId;
                trans.POSId = model.POSId;
                trans.MeterNumber1 = model.MeterNumber;
                trans.MeterToken1 = string.Empty;
                trans.Amount = model?.Amount ?? new decimal();
                trans.TransactionId = model?.TransactionId.ToString();
                trans.IsDeleted = false;
                trans.Status = 0;
                trans.CreatedAt = DateTime.UtcNow;
                trans.AccountNumber = string.Empty;
                trans.CurrentDealerBalance = 00;
                trans.Customer = string.Empty;
                trans.ReceiptNumber = string.Empty;
                trans.RequestDate = DateTime.UtcNow;
                trans.RTSUniqueID = "00";
                trans.SerialNumber = string.Empty;
                trans.ServiceCharge = "0";
                trans.Tariff = "0";
                trans.TaxCharge = "0";
                trans.TenderedAmount = model.Amount;
                trans.TransactionAmount = model.Amount; ;
                trans.Units = "0";
                trans.VProvider = string.Empty;
                trans.Finalised = false;
                trans.StatusRequestCount = 1;
                trans.Sold = false;
                trans.DateAndTimeSold = string.Empty;
                trans.DateAndTimeFinalised = string.Empty;
                trans.DateAndTimeLinked = string.Empty;
                trans.VoucherSerialNumber = string.Empty;
                trans.DebitRecovery = "0";
                trans.CostOfUnits = "0.0";
                trans.VendStatus = response_data?.Content?.Data?.Error;
                trans.VendStatusDescription = response_data?.Content?.Data?.Error;
                trans.StatusResponse = JsonConvert.SerializeObject(response_data);
                return trans;
            }
            catch (Exception e)
            {

                throw;
            }

        }
        ReceiptModel IMeterManager.ReturnVoucherReceipt(string token)
        {
            var transaction_by_token = Context.TransactionDetails.Where(e => e.MeterToken1 == token).ToList().FirstOrDefault();
            if (transaction_by_token != null)
            {
                var receipt = Build_receipt_model_from_dbtransaction_detail(transaction_by_token);
                receipt.ShouldShowSmsButton = (bool)transaction_by_token.POS.WebSms;
                receipt.ShouldShowPrintButton = (bool)transaction_by_token.POS.WebPrint;
                receipt.mobileShowSmsButton = (bool)transaction_by_token.POS.PosSms;
                receipt.mobileShowPrintButton = (bool)transaction_by_token.POS.PosPrint;
                return receipt;
            }
            return new ReceiptModel { ReceiptStatus = new ReceiptStatus { Status = "unsuccessful", Message = "Unable to find voucher" } };
        }

        RequestResponse IMeterManager.ReturnRequestANDResponseJSON(string token)
        {
            var transaction_by_token = Context.TransactionDetails.Where(e => e.TransactionId == token).ToList().FirstOrDefault();
            if (transaction_by_token != null)
            {
                var receipt = new RequestResponse { Request = transaction_by_token.Request, Response = transaction_by_token.Response };
                return receipt;
            }
            return new RequestResponse { ReceiptStatus = new ReceiptStatus { Status = "unsuccessful", Message = "Unable to find voucher" } };
        }

        TransactionDetail IMeterManager.GetLastTransaction()
        {
            var lstTr = Context.TransactionDetails.Where(e => e.Status == (int)RechargeMeterStatusEnum.Success).OrderByDescending(d => d.CreatedAt).FirstOrDefault() ?? null;
            if (lstTr != null)
            {
                lstTr.CurrentDealerBalance = lstTr.CurrentDealerBalance - lstTr.TenderedAmount;
                return lstTr;
            }
            else
                return new TransactionDetail();
        }
        TransactionDetail IMeterManager.GetSingleTransaction(string transactionId)
        {
            var lstTr = Context.TransactionDetails.FirstOrDefault(d => d.MeterToken1 == transactionId.Trim()) ?? null;
            if (lstTr != null)
            {
                return lstTr;
            }
            else
                return null;
        }

        IQueryable<BalanceSheetListingModel> IMeterManager.GetBalanceSheetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 999999999;
            var result = new PagingResult<BalanceSheetListingModel>(); 
            IQueryable<BalanceSheetListingModel> query = null;
            if (model.IsInitialLoad)
            {
                query = from a in Context.TransactionDetails
                        where DbFunctions.TruncateTime(a.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow) && a.Finalised == true
                        select new BalanceSheetListingModel
                        {
                            DateTime = a.CreatedAt,  
                            Reference = a.MeterNumber1,
                            TransactionId = a.TransactionId,
                            TransactionType = "EDSA",
                            DepositAmount = 0,
                            SaleAmount = a.Amount,
                            Balance = 0,
                            POSId = a.POSId
                        };

            }
            else
            {
                query = from a in Context.TransactionDetails
                        where a.Finalised == true
                        select new BalanceSheetListingModel
                        {
                            DateTime = a.CreatedAt,  
                            Reference = a.MeterNumber1,
                            TransactionId = a.TransactionId,
                            TransactionType = "EDSA",
                            DepositAmount = 0,
                            SaleAmount = a.Amount,
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

        IQueryable<DashboardBalanceSheetModel> IMeterManager.GetDashboardBalanceSheetReports(DateTime date)
        {
            var we  = GetVendorStatus();
           return  Context.TransactionDetails
                .Where(d => d.Finalised == true && d.Status == 1)
                .GroupBy(f => f.UserId)
                .Select(f => 
                    new DashboardBalanceSheetModel {
                            SaleAmount = f.Sum(d => d.Amount),
                            Vendor = f.FirstOrDefault().User.Vendor,
                            UserId = f.FirstOrDefault().UserId, 
                            Balance = 0,
                            DepositAmount = 0,
                            Status = "",
                            POSBalance = f.OrderByDescending(a => a.POS.Balance).FirstOrDefault().POS.Balance ?? 0
           });
        }

       public  PagingResult<VendorStatus> GetVendorStatus()
        {
            var res = new PagingResult<VendorStatus>();
            try
            {
                var result = (from tbl in (
                   from Deposits in Context.Deposits
                   group Deposits by new 
                   {
                       Deposits.UserId,
                       Deposits.CreatedAt
                   } into g
                   select new VendorStatus
                   {
                       userid = g.Key.UserId,
                       vendor = "",
                       totaldeposits = 0,
                       totalsales = (decimal)0,
                       runningbalance = 0,
                       POSBalance = 0,
                       overage = 0,
                       PercentageAmount = SqlFunctions.DatePart("month", g.Key.CreatedAt) < 7 &&
                             SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2022 ||
                             SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2021 ? (System.Decimal?)g.Sum(p => p.PercentageAmount / 1000) :
                             SqlFunctions.DatePart("month", g.Key.CreatedAt) > 6 &&
                             SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2022 ? (System.Decimal?)g.Sum(p => p.PercentageAmount) : null
                        }).Concat(from TransactionDetails in Context.TransactionDetails
                             where
                            TransactionDetails.Status == 1
                             group TransactionDetails by new
                             {
                                 TransactionDetails.UserId,
                                 TransactionDetails.CreatedAt
                             } into g
                             select new VendorStatus
                             {

                                 userid = g.Key.UserId,
                                 vendor = "",
                                 totaldeposits = 0,
                                 totalsales = SqlFunctions.DatePart("month", g.Key.CreatedAt) < 7 &&
                                    SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2022 ||
                                    SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2021 ? (System.Decimal?)g.Sum(p => p.Amount / 1000) :
                                    SqlFunctions.DatePart("month", g.Key.CreatedAt) > 6 &&
                                    SqlFunctions.DatePart("year", g.Key.CreatedAt) == 2022 ? (System.Decimal?)g.Sum(p => p.Amount) : null,
                                 runningbalance = 0,
                                 POSBalance = 0,
                                 overage = 0,
                                 PercentageAmount = (System.Decimal?)0,
                             }
               )
                              join POS in Context.POS on tbl.userid equals POS.VendorId
                              join Users in Context.Users on tbl.userid equals Users.UserId
                              group new { tbl, Users, POS } by new
                              {
                                  tbl.userid,
                                  Users.Vendor,
                                  POS.Balance
                              } into g
                              where (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.totalsales)) > 0 &&
                                (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.totalsales)) != g.Key.Balance
                              orderby
                                (g.Key.Balance - (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.totalsales)))
                              select new VendorStatus
                              {
                                  userid = g.Key.userid,
                                  vendor = g.Key.Vendor,
                                  totaldeposits = g.Sum(p => p.tbl.PercentageAmount),
                                  totalsales = (int?)g.Sum(p => p.tbl.totalsales),
                                  runningbalance = (decimal?)(g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.totalsales)),
                                  POSBalance = g.Key.Balance,
                                  overage = (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.totalsales)) - g.Key.Balance,
                                  PercentageAmount = g.Sum(p => p.tbl.PercentageAmount),
                              }).ToList();

                res.List = result;
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }



        void IMeterManager.RedenominateBalnces()
        {
            try
            {
                var posBalances = Context.TransactionDetails.Where(d => d.Amount != null && d.Amount > 0).ToList();

                foreach (var pos in posBalances)
                {
                    try
                    {
                        var balance = pos.Amount / 1000;
                        pos.Amount = balance;
                        pos.TenderedAmount = balance;
                        pos.TransactionAmount = balance;
                    }
                    catch (Exception ex)
                    { 
                        continue;
                    }
                }

                Context.SaveChanges();
            }
            catch (Exception ex)
            { 
                throw ex;
            }
            
        }

        decimal IMeterManager.ReturnMinVend()
        {
            return Context.Platforms.FirstOrDefault(d => d.PlatformId == 1).MinimumAmount;

        }
    } 
}
