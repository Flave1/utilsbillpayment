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
            dbMeter.IsVerified = model.MeterId > 0 ? model.isVerified : false;
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
            return Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID)
                .Select(p => new SelectListItem
                {
                    Text = p.Number + " - " + p.Allias ?? string.Empty,
                    Value = p.MeterId.ToString()
                }).ToList();
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
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        posIds = Context.POS.Where(p => p.VendorId != null
                        && p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
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

            var list = query.ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                ProductShortName = x.Platform?.ShortName == null ? "" : x.Platform.ShortName,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number,
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

        }

        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesHistory(ReportSearchModel model, bool callFromAdmin)
        {
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = Context.TransactionDetails.OrderByDescending(d => d.RequestDate).Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null);
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


            var list = query.Take(10).ToList().Select(x => new MeterRechargeApiListingModel(x)).ToList();

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
            if (model.MeterId != null)
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
                Context.SaveChanges();

                response.ReceiptStatus.Status = "unsuccessful";
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
                     meterId = (this as IMeterManager).SaveMeter(newMeter).ID;
                     db_transaction_detail.MeterId = meterId != 0 ? meterId : 0;
                }
                else
                {
                    db_transaction_detail.MeterId = model.MeterId;
                }

                pos.Balance = pos.Balance.Value - model.Amount;
                Context.TransactionDetails.Add(db_transaction_detail);
                Context.SaveChanges();

                Push_notification_to_user(user, model, db_transaction_detail.TransactionDetailsId);

                var receipt = Build_receipt_model_from_dbtransaction_detail(db_transaction_detail);
                receipt.ShouldShowSmsButton = (bool)db_transaction_detail.POS.SMSNotificationDeposit;
                return receipt;
            }
            catch (Exception e)
            {
                throw e;
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
                Number = model.MeterNumber
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
            receipt.Tarrif = string.Format("{0:N0}", model?.Tariff);
            receipt.DeviceNumber = model?.MeterNumber1;
            receipt.DebitRecovery = model.DebitRecovery;
            var amt = model?.TenderedAmount.ToString("N");
            receipt.Amount = amt.Contains('.') ? amt.TrimEnd('0').TrimEnd('.') : amt;
            receipt.Charges = string.Format("{0:N0}", Convert.ToDecimal(model.ServiceCharge));
            receipt.Commission = string.Format("{0:N0}", 0.00);
            receipt.Unit = string.Format("{0:N0}", model?.Units);
            receipt.UnitCost = string.Format("{0:N0}", model.CostOfUnits);
            receipt.SerialNo = model?.SerialNumber;
            receipt.Pin1 = Utilities.FormatThisToken(model?.MeterToken1) ?? string.Empty;
            receipt.Pin2 = Utilities.FormatThisToken(model?.MeterToken2) ?? string.Empty;
            receipt.Pin3 = Utilities.FormatThisToken(model?.MeterToken3) ?? string.Empty;
            receipt.Discount = string.Format("{0:N0}", 0);
            receipt.Tax = string.Format("{0:N0}", model.TaxCharge);
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
                trans.RTSUniqueID = 00;
                trans.SerialNumber = response_data?.Content?.SerialNumber.ToString() ?? string.Empty;
                trans.ServiceCharge = response_data.Content?.ServiceCharge?.ToString() ?? string.Empty;
                trans.Tariff = response_data.Content?.Tariff?.ToString() ?? string.Empty;
                trans.TaxCharge = Convert.ToDecimal(response_data?.Content?.TaxCharge);
                trans.TenderedAmount = Convert.ToDecimal(response_data?.Content?.Denomination);
                trans.TransactionAmount = Convert.ToDecimal(response_data?.Content?.Denomination);
                trans.Units = Convert.ToDecimal(response_data?.Content?.Units);
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
                tran.RTSUniqueID = response_data != null ? (long)response_data?.PowerHubVoucher?.RtsUniqueId : new long();
                tran.SerialNumber = response_data != null ? response_data?.SerialNumber.ToString() : string.Empty;
                tran.ServiceCharge = response_data != null ? response_data?.PowerHubVoucher?.ServiceCharge.ToString() : string.Empty;
                tran.Tariff = response_data != null ? response_data?.PowerHubVoucher?.Tariff.ToString() : string.Empty;
                tran.TaxCharge = response_data != null ? Convert.ToDecimal(response_data?.PowerHubVoucher?.TaxCharge) : new decimal();
                tran.TenderedAmount = response_data != null ? (decimal)response_data?.PowerHubVoucher?.TenderedAmount : new decimal();
                tran.TransactionAmount = response_data != null ? (decimal)response_data?.PowerHubVoucher?.TransactionAmount : new decimal();
                var units = response_data != null ? response_data?.PowerHubVoucher?.Units : new double();
                tran.Units = units != null ? Convert.ToDecimal(units):0;
                var costofunits = response_data != null ? response_data?.PowerHubVoucher?.CostOfUnits : string.Empty;
                tran.CostOfUnits = costofunits != null? Convert.ToDecimal(costofunits): 0;
                tran.CustomerAddress = response_data != null ? response_data?.PowerHubVoucher?.CustAddress?.ToString() : string.Empty;
                tran.PlatFormId = Convert.ToInt16(model.PlatformId);
                tran.Request = vend_request;
                tran.Response = vend_response;
                tran.Finalised = true;
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
                trans.RTSUniqueID = 00;
                trans.SerialNumber = string.Empty;
                trans.ServiceCharge = string.Empty;
                trans.Tariff = string.Empty;
                trans.TaxCharge = 0;
                trans.TenderedAmount = model.Amount;
                trans.TransactionAmount = model.Amount; ;
                trans.Units = 0;
                trans.VProvider = string.Empty;
                trans.Finalised = false;
                trans.StatusRequestCount = 1;
                trans.Sold = false;
                trans.DateAndTimeSold = string.Empty;
                trans.DateAndTimeFinalised = string.Empty;
                trans.DateAndTimeLinked = string.Empty;
                trans.VoucherSerialNumber = string.Empty;
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
                receipt.ShouldShowSmsButton = (bool)transaction_by_token.POS.SMSNotificationDeposit;
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
            var lstTr = Context.TransactionDetails.Where(e => e.Status == (int)RechargeMeterStatusEnum.Success).OrderByDescending(d => d.RequestDate).FirstOrDefault() ?? null;
            if (lstTr != null)
            {
                lstTr.CurrentDealerBalance = lstTr.CurrentDealerBalance - lstTr.TenderedAmount;
                return lstTr;
            }
            else
                return new TransactionDetail();
        }
        TransactionDetail IMeterManager.GetSingleTransaction(long transactionDetailId)
        {
            var lstTr = Context.TransactionDetails.FirstOrDefault(d => d.TransactionDetailsId == transactionDetailId) ?? null;
            if (lstTr != null)
            {
                return lstTr;
            }
            else
                return null;
        }
    }



}
