using iTextSharp.tool.xml.html;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class MeterManager : BaseManager, IMeterManager
    {
        private readonly IRTSEDSAManager _rtsManager;

        public MeterManager(IRTSEDSAManager rtsManager)
        {
            _rtsManager = rtsManager;
        }

        ActionOutput IMeterManager.SaveMeter(MeterModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.MeterNumber);
                if (dbMeter == null)
                    return ReturnError("Meter not exist.");
            }
            else
            {
                var met = Context.Meters.FirstOrDefault(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.MeterNumber);
                if (met != null)
                    return ReturnError(met.MeterId, "Same meter number already exist for you.");
            }
            dbMeter.Name = model.Name;
            dbMeter.Number = model.Number;
            dbMeter.MeterMake = model.MeterMake;
            dbMeter.Address = model.Address;
            dbMeter.Allias = model.Alias;
            dbMeter.IsSaved = model.isVerified;
            dbMeter.NumberType = (int)NumberTypeEnum.MeterNumber;
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

        ActionOutput IMeterManager.SavePhoneNUmber(NumberModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
                if (dbMeter == null)
                    return ReturnError("Phone number not exist.");
            }
            else
            {
                var met = Context.Meters.FirstOrDefault(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
                if (met != null)
                    return ReturnError(met.MeterId, "Same number already exist for you.");
            }
            dbMeter.Name = model.Name;
            dbMeter.Number = model.Number;
            dbMeter.MeterMake = model.MeterMake;
            dbMeter.Allias = model.Alias;
            dbMeter.IsSaved = model.isVerified;
            dbMeter.NumberType = (int)NumberTypeEnum.PhoneNumber;
            dbMeter.IsVerified = model.isVerified;
            if (model.MeterId == 0)
            {
                dbMeter.UserId = model.UserId;
                dbMeter.CreatedAt = DateTime.UtcNow;
                Context.Meters.Add(dbMeter);
            }
            Context.SaveChanges();
            return ReturnSuccess(dbMeter.MeterId, "Phone Number details saved successfully.");
        }
        MeterModel IMeterManager.GetMeterDetail(long meterId)
        {
            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == meterId && p.NumberType == (int)NumberTypeEnum.MeterNumber);
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
                Alias = dbMeter.Allias,
                NumberType = (int)NumberTypeEnum.MeterNumber
            };
        }

        NumberModel IMeterManager.GetPhoneNumberDetail(long Id)
        {
            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == Id && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
            if (dbMeter == null)
                return null;
            return new NumberModel
            {
                Address = dbMeter.Address,
                MeterId = dbMeter.MeterId,
                MeterMake = dbMeter.MeterMake,
                Name = dbMeter.Name,
                Number = dbMeter.Number,
                isVerified = (bool)dbMeter.IsVerified,
                Alias = dbMeter.Allias,
                NumberType = (int)NumberTypeEnum.PhoneNumber
            };
        }
        ActionOutput IMeterManager.DeleteMeter(long meterId, long userId)
        {

            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == meterId && p.UserId == userId && p.NumberType == (int)NumberTypeEnum.MeterNumber);
            if (dbMeter == null)
                return ReturnError("Meter not exist.");
            dbMeter.IsDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Meter deleted successfully.");
        }

        ActionOutput IMeterManager.DeletePhoneNumber(long id, long userId)
        {

            var dbMeter = Context.Meters.FirstOrDefault(p => p.MeterId == id && p.UserId == userId && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
            if (dbMeter == null)
                return ReturnError("Phone number does not exist.");
            dbMeter.IsDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Phone number deleted successfully.");
        }
        List<SelectListItem> IMeterManager.GetMetersDropDown(long userID)
        {
            return Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsSaved == true && p.IsVerified == true && p.NumberType == (int)NumberTypeEnum.MeterNumber)
                .Select(p => new SelectListItem
                {
                    Text = p.Number + " - " + p.Allias ?? string.Empty,
                    Value = p.MeterId.ToString()
                }).ToList();
        }

        PagingResult<MeterAPIListingModel> IMeterManager.GetMeters(long userID, int pageNo, int pageSize, bool isActive)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsVerified == isActive && p.NumberType == (int)NumberTypeEnum.MeterNumber).ToList();
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterAPIListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meters fetched successfully.";
            return result;
        }
        PagingResult<MeterAPIListingModel> IMeterManager.GetPhoneNumbers(long userID, int pageNo, int pageSize, bool isActive)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = Context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsVerified == isActive && p.NumberType == (int)NumberTypeEnum.PhoneNumber).ToList();
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
            var list = query.ToList().Select(x => new SalesReportExcelModel(x)).ToList();
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
                query = query.Where(p => p.MeterNumber1.ToLower().Contains(model.Meter.ToLower()));
                //query = query.Include("Meter").Where(p => (p.MeterId != null && p.Meter.Number.Contains(model.Meter)) || (p.MeterNumber1 != null && p.MeterNumber1.Contains(model.Meter)));
            }
            if (!string.IsNullOrEmpty(model.Product))
            {
                int parsedProductId = int.Parse(model.Product);
                if(parsedProductId > 0)
                {
                    query = query.Include("Platform").Where(p => p.Platform.PlatformId == parsedProductId);
                }
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

            var list = query.AsEnumerable().Select( x => new MeterRechargeApiListingModel(x, 1)).ToList();

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
                query = Context.TransactionDetails.OrderByDescending(x => x.CreatedAt).Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
            else
                query = Context.TransactionDetails.OrderByDescending(x => x.CreatedAt).Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

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
        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRechargesHistory(ReportSearchModel model, bool callFromAdmin, PlatformTypeEnum platform)
        {
            if(model.RecordsPerPage != 20)
            {
                model.RecordsPerPage = 10;
            }
            var result = new PagingResult<MeterRechargeApiListingModel>();
            IQueryable<TransactionDetail> query = null;
            if(platform != PlatformTypeEnum.All)
                query = Context.TransactionDetails.OrderByDescending(d => d.CreatedAt)
                .Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null && (int)platform == p.Platform.PlatformType);
            else
                query = Context.TransactionDetails.OrderByDescending(d => d.CreatedAt)
                 .Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null);

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



            var list = query.Take(model.RecordsPerPage).AsEnumerable().OrderByDescending(x => x.CreatedAt).Select(x => new MeterRechargeApiListingModel(x)).ToList();

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
                TransactionId = Utilities.GetLastMeterRechardeId().ToString(),//Utilities.GenerateUniqueId(),
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
            obj.Message = "Your meter has successfully recharged with NLe " + Utilities.FormatAmount(model.Amount) + " PIN: " + dbMeterRecharge.MeterToken1;
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                Common.PushNotification.SendNotification(obj);
            }

            return ReturnSuccess("Meter recharged successfully.");

            //return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = dbMeterRecharge, Count = retailSchedDetail.NoOfBags });

        }


        //ReceiptModel IMeterManager.RechargeMeterReturn_old(RechargeMeterModel model)
        //{
        //    var response = new ReceiptModel { ReceiptStatus = new ReceiptStatus() };

        //    Platform platf = new Platform();
        //    if (model.PlatformId == null)
        //    {
        //        platf = Context.Platforms.Find(1);
        //        model.PlatformId = platf.PlatformId;
        //    }
        //    else
        //        platf = Context.Platforms.Find(model.PlatformId);

        //    if (platf.DisablePlatform)
        //    {
        //        response.ReceiptStatus.Status = "unsuccessful";
        //        response.ReceiptStatus.Message = platf.DisabledPlatformMessage;
        //        return response;
        //    }

        //    var user = Context.Users.FirstOrDefault(p => p.UserId == model.UserId);
        //    if (user == null)
        //    {
        //        response.ReceiptStatus.Status = "unsuccessful";
        //        response.ReceiptStatus.Message = "User does not exist";
        //        return response;
        //    }
        //    var pos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId);

        //    if (pos == null)
        //    {
        //        response.ReceiptStatus.Status = "unsuccessful";
        //        response.ReceiptStatus.Message = "POS NOT FOUND!! Please Contact Administrator.";
        //        return response;
        //    }
        //    if (pos.Balance == null)
        //    {
        //        response.ReceiptStatus.Status = "unsuccessful";
        //        response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
        //        return response;
        //    }


        //    if (model.Amount > pos.Balance || pos.Balance.Value < model.Amount)
        //    {
        //        response.ReceiptStatus.Status = "unsuccessful";
        //        response.ReceiptStatus.Message = "INSUFFICIENT BALANCE FOR THIS TRANSACTION.";
        //        return response;
        //    }

        //    if (model.MeterId != null)
        //    {
        //        var met = Context.Meters.Find(model.MeterId);
        //        model.MeterNumber = met.Number;
        //    }
        //    else
        //    {
        //        model.IsSaved = false;
        //    }


        //    IceKloudResponse icekloud_response = new IceKloudResponse();
        //    IcekloudQueryResponse query_response = new IcekloudQueryResponse();
        //    TransactionDetail db_transaction_detail = new TransactionDetail();

        //    model.TransactionId = Convert.ToInt64(Utilities.GetLastMeterRechardeId());
        //    icekloud_response = Make_recharge_request_from_icekloud(model);

        //    var vend_request = JsonConvert.SerializeObject(icekloud_response.RequestModel);
        //    var vend_response = JsonConvert.SerializeObject(icekloud_response);
        //    var response_data = icekloud_response.Content.Data.Data.FirstOrDefault();

        //    if (icekloud_response.Content.Data.Error == "Unable to connect to the remote server")
        //    {
        //        var query_request = Buid_vend_query_object(model);
        //        query_response = Query_vend_status(query_request);
        //        if (!query_response.Content.Finalised)
        //        {
        //            db_transaction_detail = UpdateTransaction(query_response, model);
        //            db_transaction_detail.PlatFormId = platf.PlatformId;
        //            db_transaction_detail.Platform = platf;

        //            Context.TransactionDetails.Add(db_transaction_detail);
        //            Context.SaveChanges();

        //            response.ReceiptStatus.Status = "unsuccessful";
        //            response.ReceiptStatus.Message = query_response.Content.StatusDescription;
        //            return response;
        //        }
        //    }
        //    else if (icekloud_response.Status.ToLower() != "success")
        //    {
        //        //Will save to a different table
        //        db_transaction_detail = Build_db_transaction_detail_from_FAILED_response(icekloud_response, model);
        //        db_transaction_detail.PlatFormId = platf.PlatformId;
        //        db_transaction_detail.Platform = platf;
        //        //db_transaction_detail.TransactionDetailsId = generateTrxTableKey();
        //        Context.TransactionDetails.Add(db_transaction_detail);

        //        SaveSales();

        //        response.ReceiptStatus.Status = "unsuccessful";
        //        if ("Input string was not in a correct format." == icekloud_response.Content.Data?.Error)
        //        {
        //            response.ReceiptStatus.Message = "Amount tendered is too low";
        //            return response;
        //        }
        //        response.ReceiptStatus.Message = icekloud_response.Content.Data?.Error;
        //        return response;
        //    }


        //    if (response_data != null)
        //    {
        //        db_transaction_detail = Build_db_transaction_detail_from_Icekloud_response(response_data, model, vend_request, vend_response);
        //    }
        //    else if (query_response.Content.Finalised)
        //    {
        //        db_transaction_detail = UpdateTransaction(query_response, model);
        //    }

        //    db_transaction_detail.PlatFormId = platf.PlatformId;
        //    db_transaction_detail.Platform = platf;
        //    db_transaction_detail.TransactionId = model.TransactionId.ToString();
        //    try
        //    {
        //        MeterModel newMeter = new MeterModel();
        //        long meterId = 0;
        //        if (model.SaveAsNewMeter)
        //        {
        //            newMeter = StackNewMeterToDbObject(model);
        //            newMeter.IsSaved = true;
        //            meterId = (this as IMeterManager).SaveMeter(newMeter).ID;
        //            db_transaction_detail.MeterId = meterId != 0 ? meterId : 0;

        //        }
        //        else
        //        {
        //            db_transaction_detail.MeterId = model.MeterId;
        //        }

        //        db_transaction_detail.BalanceBefore = pos.Balance ?? 0;
        //        pos.Balance = (pos.Balance - model.Amount);
        //        db_transaction_detail.CurrentVendorBalance = pos.Balance ?? 0;
        //        db_transaction_detail.QueryStatusCount = 0;
        //        Context.TransactionDetails.Add(db_transaction_detail);
        //        SaveSales();

        //        PushNotification(user, model, db_transaction_detail.TransactionDetailsId);

        //        var receipt = BuildRceipt(db_transaction_detail);
        //        receipt.ShouldShowSmsButton = (bool)db_transaction_detail.POS.WebSms;
        //        receipt.ShouldShowPrintButton = (bool)db_transaction_detail.POS.WebPrint;
        //        receipt.mobileShowSmsButton = (bool)db_transaction_detail.POS.PosSms;
        //        receipt.mobileShowPrintButton = (bool)db_transaction_detail.POS.PosPrint;
        //        receipt.CurrentBallance = db_transaction_detail?.POS?.Balance ?? 0;
        //        return receipt;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }

        //}

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
            {
                platf = Context.Platforms.Find(model.PlatformId);
                model.PlatformId = platf.PlatformId;
            }

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
                model.IsSaved = false;

            var queryResponse = new IcekloudQueryResponse();

            TransactionDetail transDetail = CreateRecordBeforeVend(model); 
            model.TransactionId = long.Parse(transDetail.TransactionId); 
            var vendResponse = await _rtsManager.RequestVendAsync(model); 

            transDetail.Request = JsonConvert.SerializeObject(vendResponse.RequestModel); 
            transDetail.Response = JsonConvert.SerializeObject(vendResponse);


            var vendResponseData = vendResponse.Content.Data.Data.FirstOrDefault();

            if (vendResponse.Status.ToLower() != "success")
            {
                transDetail.VendStatus = vendResponse?.Content?.Data?.Error;
                transDetail.VendStatusDescription = vendResponse?.Content?.Data?.Error;
                transDetail.StatusResponse = JsonConvert.SerializeObject(vendResponseData);
                transDetail.QueryStatusCount = 1;
                Context.SaveChanges();

                response.ReceiptStatus.Status = "unsuccessful";
                if ("Input string was not in a correct format." == vendResponse.Content.Data?.Error)
                {
                    response.ReceiptStatus.Message = "Amount tendered is too low";
                    return response;
                }
                var vendStatus = await _rtsManager.QueryVendStatus(model, transDetail);

                if(vendStatus.FirstOrDefault().Key != "success")
                {
                    response.ReceiptStatus.Message = vendResponse.Content.Data?.Error;
                    return response;
                }

                transDetail = UpdateTransactionOnStatusSuccess(vendStatus.FirstOrDefault().Value, transDetail);
            }
            else
            {
                transDetail = UpdateTransaction(vendResponseData, transDetail, pos);
            }

            try
            {
                transDetail.MeterId = UpdateMeterOrSaveAsNew(model);

                PushNotification(user, model, transDetail.TransactionDetailsId);

                var receipt = BuildRceipt(transDetail);
                receipt.ShouldShowSmsButton = (bool)transDetail.POS.WebSms;
                receipt.ShouldShowPrintButton = (bool)transDetail.POS.WebPrint;
                receipt.mobileShowSmsButton = (bool)transDetail.POS.PosSms;
                receipt.mobileShowPrintButton = (bool)transDetail.POS.PosPrint;
                receipt.CurrentBallance = transDetail?.POS?.Balance ?? 0;
                return receipt;
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        private  TransactionDetail CreateRecordBeforeVend(RechargeMeterModel model)
        {
            var trans = new TransactionDetail();
            trans.PlatFormId = model.PlatformId;
            trans.UserId = model.UserId;
            trans.MeterId = model.MeterId;
            trans.POSId = model.POSId;
            trans.MeterNumber1 = model.MeterNumber;
            trans.TransactionDetailsId = (long)model.MeterId;
            trans.MeterToken1 = model.MeterToken1;
            trans.Amount = model.Amount;
            trans.TransactionId = Utilities.GetLastMeterRechardeId().ToString();
            trans.IsDeleted = false;
            trans.Status = (int)RechargeMeterStatusEnum.Pending;
            trans.CreatedAt = DateTime.UtcNow;
            trans.AccountNumber = "";
            trans.CurrentDealerBalance = 00;
            trans.Customer = "";
            trans.ReceiptNumber = "";
            trans.RequestDate = DateTime.UtcNow;
            trans.RTSUniqueID = "00";
            trans.SerialNumber = "";
            trans.ServiceCharge = "";
            trans.Tariff = "";
            trans.TaxCharge = "";
            trans.TenderedAmount = model.Amount;
            trans.TransactionAmount = model.Amount;
            trans.Units = "";
            trans.VProvider = "";
            trans.Finalised = false;
            trans.StatusRequestCount = 0;
            trans.Sold = false;
            trans.DateAndTimeSold = "";
            trans.DateAndTimeFinalised = "";
            trans.DateAndTimeLinked = "";
            trans.VoucherSerialNumber = "";
            trans.VendStatus = "";
            trans.VendStatusDescription = "";
            trans.StatusResponse = "";
            trans.DebitRecovery = "0";
            trans.CostOfUnits = "0";
            Context.TransactionDetails.Add(trans);
            SaveChanges();
            return trans;
        }

        private MeterModel StackNewMeterToDbObject(RechargeMeterModel model)
        { 
            return new MeterModel
            {
                Address = "",
                Alias = "",
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
            data.PlatformId = recharge.PlatFormId;
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

        public ReceiptModel BuildRceipt(TransactionDetail model)
        {
            if (model.POS == null) model.POS = new POS();
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
            receipt.PlatformId = model.PlatFormId;
            return receipt;
        }
        private void PushNotification(User user, RechargeMeterModel model, long MeterRechargeId)
        {
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = MeterRechargeId;
            obj.Title = "Meter recharged successfully";
            obj.Message = $"Your meter has successfully recharged with NLe { Utilities.FormatAmount(model.Amount) } PIN: {model.MeterToken1}{model.MeterToken2}{model.MeterToken3}";
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                Common.PushNotification.SendNotification(obj);
            }
        }
        private TransactionDetail UpdateTransaction(Datum response_data, TransactionDetail trans, POS pos)
        {
            trans.CurrentDealerBalance = response_data.DealerBalance;
            trans.CostOfUnits = response_data.PowerHubVoucher.CostOfUnits;
            trans.MeterToken1 = response_data?.PowerHubVoucher.Pin1?.ToString() ?? string.Empty;
            trans.Status = (int)RechargeMeterStatusEnum.Success;
            trans.AccountNumber = response_data.PowerHubVoucher?.AccountNumber ?? string.Empty;
            trans.Customer = response_data.PowerHubVoucher?.Customer ?? string.Empty;
            trans.ReceiptNumber = response_data?.PowerHubVoucher.ReceiptNumber ?? string.Empty;
            trans.SerialNumber = response_data?.SerialNumber ?? string.Empty;
            trans.RTSUniqueID = response_data.PowerHubVoucher.RtsUniqueId;
            trans.ServiceCharge = response_data?.PowerHubVoucher?.ServiceCharge;
            trans.Tariff = response_data.PowerHubVoucher?.Tariff;
            trans.TaxCharge = response_data?.PowerHubVoucher?.TaxCharge;
            trans.Units = response_data?.PowerHubVoucher?.Units;
            trans.VProvider = "";
            trans.CustomerAddress = response_data?.PowerHubVoucher?.CustAddress;
            trans.Finalised = true;
            trans.VProvider = response_data.Provider;
            trans.StatusRequestCount = 0;
            trans.Sold = true;
            trans.VoucherSerialNumber = response_data?.SerialNumber;
            trans.VendStatus = "";
            //BALANCE DEDUCTION
            trans.BalanceBefore = pos.Balance ?? 0;
            pos.Balance = (pos.Balance - trans.Amount);
            trans.CurrentVendorBalance = pos.Balance ?? 0;
            //trans.QueryStatusCount = 0;
            Context.SaveChanges();
            return trans;

        }
        private TransactionDetail UpdateTransactionOnStatusSuccess(IcekloudQueryResponse response_data, TransactionDetail trans)
        {
            try
            {
                //trans.CurrentDealerBalance = response_data.Content.ba;
                //trans.CostOfUnits = response_data.PowerHubVoucher.CostOfUnits;
                trans.MeterToken1 = response_data?.Content?.VoucherPin ?? string.Empty;
                trans.TransactionId = response_data?.Content?.TransactionId.ToString();
                trans.Status = response_data.Content.Finalised ? (int)RechargeMeterStatusEnum.Success : 0;
                trans.AccountNumber = response_data.Content?.CustomerAccNo ?? string.Empty;
                trans.Customer = response_data.Content?.Customer ?? string.Empty;
                trans.ReceiptNumber = response_data.Content?.VoucherSerialNumber ?? string.Empty;
                trans.RTSUniqueID = "00";
                trans.SerialNumber = response_data?.Content?.SerialNumber ?? string.Empty;
                trans.ServiceCharge = response_data?.Content?.ServiceCharge;
                trans.Tariff = response_data.Content?.Tariff;
                trans.TaxCharge = response_data?.Content?.TaxCharge;
                trans.Units = response_data?.Content?.Units;
                trans.VProvider = response_data?.Content?.Provider ?? string.Empty;
                trans.Finalised = response_data?.Content?.Finalised;
                trans.Sold = response_data?.Content?.Sold;
                trans.DateAndTimeSold = response_data.Content?.DateAndTimeSold;
                trans.DateAndTimeFinalised = response_data?.Content?.DateAndTimeFinalised;
                trans.DateAndTimeLinked = response_data?.Content?.DateAndTimeLinked;
                trans.VoucherSerialNumber = response_data?.Content?.VoucherSerialNumber;
                trans.VendStatus = response_data.Content?.Status;
                trans.VendStatusDescription = response_data?.Content?.StatusDescription;
                trans.StatusResponse = JsonConvert.SerializeObject(response_data);
                trans.DebitRecovery = "0";
                return trans;
            }
            catch (Exception e)
            {

                throw;
            }

        }
        ReceiptModel IMeterManager.ReturnVoucherReceipt(string token)
        {
            token = BLL.Common.Utilities.ReplaceWhitespace(token, "");
            var transaction_by_token = Context.TransactionDetails.Where(e => e.MeterToken1 == token).ToList().FirstOrDefault();
            if (transaction_by_token != null)
            {
                var receipt = BuildRceipt(transaction_by_token);
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
            var lstTr = Context.TransactionDetails.Where(e => e.Status == (int)RechargeMeterStatusEnum.Success 
            && e.Platform.PlatformType == (int)PlatformTypeEnum.ELECTRICITY).OrderByDescending(d => d.CreatedAt).FirstOrDefault() ?? null;
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

        void IMeterManager.LogSms(TransactionDetail td, string phone)
        {
            var smsObj = new SMS_LOG
            {
                Date_Time = td.CreatedAt,
                Meter_Number = td.MeterNumber1,
                Phone_number = phone,
                POSID = td.POSId,
                TransactionID = td.TransactionId,
                UserID = td.UserId,
                AgentID = td.User.AgentId
            };
            Context.SMS_LOG.Add(smsObj);
            Context.SaveChanges();
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
                            TransactionType = a.Platform.Title,
                            DepositAmount = 0,
                            SaleAmount = a.Amount,
                            Balance = 0,
                            POSId = a.POSId,
                            BalanceBefore = a.BalanceBefore.Value
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
                            TransactionType = a.Platform.Title,
                            DepositAmount = 0,
                            SaleAmount = a.Amount,
                            Balance = 0,
                            POSId = a.POSId,
                            BalanceBefore = a.BalanceBefore.Value
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
                var date = DateTime.Parse("2022/07/01");
                var query = (from tbl in ( from Deposits in Context.Deposits
                            group Deposits by new DepTrans1
                            {
                                UserId = Deposits.UserId,
                                CreatedAt = Deposits.CreatedAt,
                                AgencyCommission = Deposits.AgencyCommission,
                                TransactionId = Deposits.TransactionId,
                                NewBalance = Deposits.NewBalance
                            } into vv
                            select new Dep1
                            {
                                CreatedAt = vv.Key.CreatedAt,
                                TransactionId = vv.Key.TransactionId,
                                UserId = vv.Key.UserId,
                                NewBalance = vv.Key.NewBalance,
                                PercentageAmount =
                               DbFunctions.TruncateTime(vv.Key.CreatedAt) > DbFunctions.TruncateTime(date) ? (System.Decimal?)vv.Sum(p => p.PercentageAmount) :
                              DbFunctions.TruncateTime(vv.Key.CreatedAt) < DbFunctions.TruncateTime(date) ? (System.Decimal?)vv.Sum(p => p.PercentageAmount / 1000) : null,
                                Totalsales = (decimal?)0,
                                AgencyCommission = vv.Key.AgencyCommission
                            }
                        ).Concat(from TransactionDetails in Context.TransactionDetails where  TransactionDetails.Status == 1
                                    group TransactionDetails by new
                                    {
                                        TransactionDetails.UserId,
                                        TransactionDetails.CreatedAt,
                                        TransactionDetails.TransactionId,
                                        TransactionDetails.CurrentVendorBalance
                                    } into bb
                                    select new Dep1
                                    {
                                        CreatedAt = bb.Key.CreatedAt,
                                        TransactionId = bb.Key.TransactionId,
                                        UserId = bb.Key.UserId,
                                        NewBalance = bb.Key.CurrentVendorBalance,
                                        PercentageAmount = (decimal?)0,
                                        Totalsales =
                                          DbFunctions.TruncateTime(bb.Key.CreatedAt) > DbFunctions.TruncateTime(date) ? (decimal?)bb.Sum(p => p.Amount) :
                                          DbFunctions.TruncateTime(bb.Key.CreatedAt) < DbFunctions.TruncateTime(date) ? (decimal?)bb.Sum(p => p.Amount / 1000) : null,
                                        AgencyCommission = (decimal?)0
                                    }
                                )

                             join POS in Context.POS on new { VendorId = tbl.UserId } equals new { VendorId = POS.VendorId ?? 0}
                             join Users in Context.Users on new { UserId = (long)tbl.UserId } equals new { UserId = Users.UserId }
                             group new { tbl, Users, POS } by new
                             {
                                 tbl.UserId,
                                 Users.Vendor,
                                 POS.Balance
                             } into g
                             where (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.Totalsales)) > 0 &&
                               (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.Totalsales)) != g.Key.Balance
                             orderby
                               (g.Key.Balance - (g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.Totalsales)))
                             select new VendorStatus
                             {
                                userid = g.Key.UserId,
                                 vendor = g.Key.Vendor,
                                 totaldeposits = (decimal?)g.Sum(p => p.tbl.PercentageAmount),
                                 totalsales = (int?)g.Sum(p => p.tbl.Totalsales),
                                 runningbalance = (decimal?)(g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.Totalsales)),
                                 POSBalance = g.Key.Balance,
                                 overage = (decimal?)((g.Sum(p => p.tbl.PercentageAmount) - g.Sum(p => p.tbl.Totalsales)) - g.Key.Balance)
                             }).Where(x => x.overage.ToString().StartsWith("-")).ToList();



                

                 res.List = query;
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }

        decimal IMeterManager.ReturnMinVend()
        {
            return Context.Platforms.FirstOrDefault(d => d.PlatformId == 1).MinimumAmount;

        }
       private long UpdateMeterOrSaveAsNew(RechargeMeterModel model)
        {
            MeterModel newMeter = new MeterModel();
            long meterId = 0;
            if (model.SaveAsNewMeter)
            {
                newMeter = StackNewMeterToDbObject(model);
                newMeter.IsSaved = true;
                meterId = (this as IMeterManager).SaveMeter(newMeter).ID;
                meterId = meterId != 0 ? meterId : 0;
            }
            else
                meterId = model.MeterId ?? 0;
            return meterId;
        }

        PagingResult<MiniSalesReport> IMeterManager.GetMiniSalesReport(ReportSearchModel model, bool callFromAdmin, long agentId, string type)
        {
            model.RecordsPerPage = 10000000;
            var result = new PagingResult<MiniSalesReport>();

            IQueryable<TransactionDetail> query = null;

            query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
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

            if (type == "daily")
            {
                var dailyRp = query.GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt)).AsEnumerable().Select(d => new MiniSalesReport
                {
                    DateTime = d.First().CreatedAt.ToString("dd/MM/yyyy").Substring(0,2) + " - " + d.Last().CreatedAt.ToString("dd/MM/yyyy"),
                    TAmount = Utilities.FormatAmount(d.Sum(s => s.Amount))
                }).ToList();
                result.List = dailyRp;
            }
            if (type == "weekly")
            {
                var dailyRp = query.AsEnumerable().GroupBy(i => Utilities.WeekOfYearISO8601(i.CreatedAt)).Select(d => new MiniSalesReport
                {
                    DateTime = d.First().CreatedAt.ToString("dd/MM/yyyy").Substring(0, 2) + " - " + d.Last().CreatedAt.ToString("dd/MM/yyyy"),
                    TAmount = Utilities.FormatAmount(d.Sum(s => s.Amount))
                }).ToList();
                result.List = dailyRp;
            }
            if (type == "monthly")
            {
                var dailyRp = query.AsEnumerable().GroupBy(i => i.CreatedAt.Month).Select(d => new MiniSalesReport
                {
                    DateTime = d.First().CreatedAt.ToString("dd/MM/yyyy").Substring(0, 2) + " - " + d.Last().CreatedAt.ToString("dd/MM/yyyy"),
                    TAmount = Utilities.FormatAmount(d.Sum(s => s.Amount))
                }).ToList();
                result.List = dailyRp;
            }

            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;

        }

        //private static IceKloudResponse Make_recharge_request_from_icekloud(RechargeMeterModel model)
        //{
        //    IceKloudResponse response = new IceKloudResponse();
        //    string strings_result = string.Empty;
        //    IcekloudRequestmodel request_model = new IcekloudRequestmodel();
        //    HttpResponseMessage icekloud_response = new HttpResponseMessage();
        //    HttpClient _http_client = new HttpClient();
        //    string url = (WebConfigurationManager.AppSettings["IsDevelopment"].ToString() == "1") ?
        //                 WebConfigurationManager.AppSettings["DevIcekloudURL"].ToString() :
        //                 WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

        //    try
        //    {
        //        request_model = Buid_new_request_object(model);

        //        icekloud_response = _http_client.PostAsJsonAsync(url, request_model).Result;

        //        strings_result = icekloud_response.Content.ReadAsStringAsync().Result;
        //        response = JsonConvert.DeserializeObject<IceKloudResponse>(strings_result);
        //        response.RequestModel = request_model;
        //        return response;
        //    }
        //    catch (AggregateException err)
        //    {
        //        foreach (var errInner in err.InnerExceptions)
        //        {
        //            Debug.WriteLine(errInner);
        //        }
        //        throw new AggregateException();
        //    }
        //    catch (Exception)
        //    {
        //        try
        //        {
        //            IceCloudErorResponse error_response = JsonConvert.DeserializeObject<IceCloudErorResponse>(strings_result);

        //            if (error_response.Status == "Error")
        //            {
        //                if (error_response.SystemError.ToLower() == "Unable to connect to the remote server".ToLower())
        //                {
        //                    response.Status = "unsuccesful";
        //                    response.Content.Data.Error = error_response.SystemError;
        //                    response.RequestModel = request_model;
        //                    return response;
        //                }
        //                if (error_response.SystemError.ToLower() == "The specified TransactionID already exists for this terminal.".ToLower())
        //                {
        //                    model.TransactionId = model.TransactionId + 1;
        //                    return Make_recharge_request_from_icekloud(model);
        //                }


        //                response.Status = error_response?.Status;
        //                response.Content.Data.Error = error_response?.Stack.ToArray()[0]?.Detail ?? error_response?.SystemError;
        //                response.RequestModel = request_model;
        //                return response;
        //            }
        //        }
        //        catch (Exception e) { throw e; }
        //        throw;
        //    }
        //}
       

    }
}
