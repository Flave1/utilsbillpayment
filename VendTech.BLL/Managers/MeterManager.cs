using iTextSharp.tool.xml.html;
using Newtonsoft.Json;
using Patagames.Pdf.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.PeerToPeer;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Util;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class MeterManager : BaseManager, IMeterManager
    {
        private HttpClient _client;
        private VendtechEntities _context;
        public MeterManager(VendtechEntities context)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            _context = context;
        }
        ActionOutput IMeterManager.SaveMeter(MeterModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.MeterNumber);
                if (dbMeter == null)
                    return ReturnError("Meter not exist.");
            }
            else
            {
                var met = _context.Meters.FirstOrDefault(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.MeterNumber);
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
            if (model.MeterId == 0 || model.MeterId == null)
            {
                dbMeter.UserId = model.UserId;
                dbMeter.CreatedAt = DateTime.UtcNow;
                _context.Meters.Add(dbMeter);
            }
            _context.SaveChanges();
            return ReturnSuccess(dbMeter.MeterId, "METER DETAILS SAVED SUCCESSFULLY");
        }

        ActionOutput IMeterManager.SavePhoneNUmber(NumberModel model)
        {
            var dbMeter = new Meter();
            if (model.MeterId > 0)
            {
                dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == model.MeterId && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
                if (dbMeter == null)
                    return ReturnError("Phone number not exist.");
            }
            else
            {
                var met = _context.Meters.FirstOrDefault(p => p.Number.Trim() == model.Number.Trim() && p.UserId == model.UserId && p.IsDeleted == false && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
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
                _context.Meters.Add(dbMeter);
            }
            _context.SaveChanges();
            return ReturnSuccess(dbMeter.MeterId, "Phone Number details saved successfully.");
        }
        MeterModel IMeterManager.GetMeterDetail(long meterId)
        {
            var dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == meterId && p.NumberType == (int)NumberTypeEnum.MeterNumber);
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

        bool IMeterManager.IsModuleLocked(int moduleId, long userId)
        {
            return _context.UserAssignedModules.FirstOrDefault(p => p.ModuleId == moduleId && p.UserId == userId) != null;

        }

        NumberModel IMeterManager.GetPhoneNumberDetail(long Id)
        {
            var dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == Id && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
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

            var dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == meterId && p.UserId == userId && p.NumberType == (int)NumberTypeEnum.MeterNumber);
            if (dbMeter == null)
                return ReturnError("Meter not exist.");
            dbMeter.IsDeleted = true;
            _context.SaveChanges();
            return ReturnSuccess("Meter deleted successfully.");
        }

        ActionOutput IMeterManager.DeletePhoneNumber(long id, long userId)
        {

            var dbMeter = _context.Meters.FirstOrDefault(p => p.MeterId == id && p.UserId == userId && p.NumberType == (int)NumberTypeEnum.PhoneNumber);
            if (dbMeter == null)
                return ReturnError("Phone number does not exist.");
            dbMeter.IsDeleted = true;
            _context.SaveChanges();
            return ReturnSuccess("Phone number deleted successfully.");
        }
        List<SelectListItem> IMeterManager.GetMetersDropDown(long userID)
        {
            return _context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsSaved == true && p.IsVerified == true && p.NumberType == (int)NumberTypeEnum.MeterNumber)
                .Select(p => new SelectListItem
                {
                    Text = p.Number + " - " + p.Allias ?? string.Empty,
                    Value = p.MeterId.ToString()
                }).ToList();
        }

        PagingResult<MeterAPIListingModel> IMeterManager.GetMeters(long userID, int pageNo, int pageSize, bool isActive)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = _context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsVerified == isActive && p.NumberType == (int)NumberTypeEnum.MeterNumber).ToList();
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterAPIListingModel(x)).ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var platform = _context.Platforms.FirstOrDefault(d => d.PlatformType == (int)PlatformTypeEnum.ELECTRICITY);
                list[i].PlatformDisabled = platform.DisablePlatform;
                list[i].PlatformId = platform.PlatformId;
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meters fetched successfully.";
            return result;
        }
        PagingResult<MeterAPIListingModel> IMeterManager.GetPhoneNumbers(long userID, int pageNo, int pageSize, bool isActive)
        {
            var result = new PagingResult<MeterAPIListingModel>();
            var query = _context.Meters.Where(p => !p.IsDeleted && p.UserId == userID && p.IsVerified == isActive && p.NumberType == (int)NumberTypeEnum.PhoneNumber).ToList();
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterAPIListingModel(x)).ToList();
            var meterMakes = list.Select(item => item.MeterMake).ToList();
            var platforms = _context.Platforms
                .Where(p => meterMakes.Any(mm => p.ShortName.Contains(mm)))
                .ToList();

            foreach (var item in list)
            {
                var selectedPlatform = platforms.FirstOrDefault(p => p.ShortName.Contains(item?.MeterMake));
                item.PlatformDisabled = (bool)selectedPlatform?.DisablePlatform;
                item.PlatformId = selectedPlatform.PlatformId;
            }
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meters fetched successfully.";
            return result;
        }
        PagingResult<SalesReportExcelModel> IMeterManager.GetSalesExcelReportData(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            var result = new PagingResult<SalesReportExcelModel>();

            var query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);

            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    // posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == model.AgencyId).Select(p => p.POSId).ToList();
                    posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
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
            if (!string.IsNullOrEmpty(model.ProductId))
            {
                int parsedProductId = int.Parse(model.ProductId);
                if (parsedProductId > 0)
                {
                    query = query.Include("Platform").Where(p => p.Platform.PlatformId == parsedProductId);
                }
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
                query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
            else
                query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        //posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == model.VendorId || p.User.AgentId == agentId).Select(p => p.POSId).ToList();
                        posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
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
                if (parsedProductId > 0)
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

            var list = query.AsEnumerable().Select(x => new MeterRechargeApiListingModel(x, 1)).ToList();

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

        PagingResult<MeterRechargeApiListingModelMobile> IMeterManager.GetUserMeterRechargesReportMobileAsync(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            //model.RecordsPerPage = 10000000;
            var result = new PagingResult<MeterRechargeApiListingModelMobile>();

            IQueryable<TransactionDetail> query = null;
            if (!model.IsInitialLoad)
                query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
            else
                query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        //posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == model.VendorId || p.User.AgentId == agentId).Select(p => p.POSId).ToList();
                        posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
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
                if (parsedProductId > 0)
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

            var list = query.AsEnumerable().Select(x => new MeterRechargeApiListingModelMobile(x, 1)).ToList();

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
                query = _context.TransactionDetails.OrderByDescending(x => x.CreatedAt).Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && p.Platform.PlatformType == (int)PlatformTypeEnum.ELECTRICITY);
            else
                query = _context.TransactionDetails.OrderByDescending(x => x.CreatedAt).Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true && DbFunctions.TruncateTime(p.CreatedAt) == DbFunctions.TruncateTime(DateTime.UtcNow));

            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
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
                //(p.MeterId != null && p.Meter?.Number?.Contains(model.Meter ?? "")) ||
                query = query.AsEnumerable().Where(p => (p.MeterNumber1 != null && p.MeterNumber1.Contains(model.Meter))).AsQueryable();
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
            var result = new PagingResult<MeterRechargeApiListingModel>();
            IQueryable<TransactionDetail> query = null;
            if (platform != PlatformTypeEnum.All)
                query = _context.TransactionDetails
                .Where(p => !p.IsDeleted && p.POSId != null && (int)platform == p.Platform.PlatformType && p.Finalised == true)
                .OrderByDescending(d => d.CreatedAt);
            else
                query = _context.TransactionDetails.OrderByDescending(d => d.CreatedAt)
                 .Where(p => !p.IsDeleted && p.Finalised == true && p.POSId != null);

            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId.Value));
            }



            var list = query.Take(10).AsEnumerable().Select(x => new MeterRechargeApiListingModel
            {
                //TransactionDetailsId = x.TransactionDetailsId,
                Amount = Utilities.FormatAmount(x.Amount),
                //PlatformId = (int)x.PlatFormId,
                ProductShortName = x.Platform.Title,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber,
                Status = ((RechargeMeterStatusEnum)x.Status).ToString(),
                TransactionId = x.TransactionId,
                //MeterRechargeId = x.TransactionDetailsId,
                //RechargeId = x.TransactionDetailsId,
                //UserName = x.User?.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : ""),
                VendorName = x.POS.User == null ? "" : x.POS.User.Vendor,
                RechargePin = x.Platform.PlatformType == 4 ? Utilities.FormatThisToken(x.MeterToken1) : x.MeterNumber1 + "/" + x.TransactionId,
                PlatformName = x.Platform.Title,
                NotType = "sale",
            }).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Meter recharges fetched successfully.";
            return result;
        }

        PagingResult<MeterRechargeApiListingModel> IMeterManager.GetUserMeterRecharges(long userID, int pageNo, int pageSize)
        {
            var result = new PagingResult<MeterRechargeApiListingModel>();
            var query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.UserId == userID && p.Finalised == true);
            result.TotalCount = query.Count();
            var list = query.OrderByDescending(p => p.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = Utilities.FormatAmount(x.Amount),
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

        string LogExceptionToDatabase(Exception exc)
        {
            var context = new VendtechEntities();
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = exc.Message;
            errorObj.StackTrace = exc.StackTrace;
            errorObj.InnerException = exc.InnerException == null ? "" : exc.InnerException.Message;
            errorObj.LoggedInDetails = "";
            errorObj.LoggedAt = DateTime.UtcNow;
            errorObj.UserId = 0;
            context.ErrorLogs.Add(errorObj);
            // To do
            context.SaveChanges();
            return errorObj.ErrorLogID.ToString();
        }

        async Task<ReceiptModel> IMeterManager.RechargeMeterReturnIMPROVED(RechargeMeterModel model)
        {
            var response = new ReceiptModel { ReceiptStatus = new ReceiptStatus() };
            var trax = new TransactionDetail();

            var user = _context.Users.FirstOrDefault(p => p.UserId == model.UserId);
            var pos = _context.POS.FirstOrDefault(p => p.POSId == model.POSId);
            var met = _context.Meters.FirstOrDefault(d => d.MeterId == model.MeterId);

            var valResult = model.validateRequest(user, pos);

            if (valResult != "clear")
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = valResult;
                return response;
            }

            model.UpdateRequestModel(met == null ? "" : met?.Number);

            var pendingTrx = await getLastMeterPendingTransaction(model.MeterNumber);

            var isDuplicate = model.IsRequestADuplicate(pendingTrx);

            try
            {
                trax = await ProcessTransaction(isDuplicate, model, isDuplicate ? pendingTrx : trax);
            }
            catch (ArgumentException ex)
            {
                response.ReceiptStatus.Message = ex.Message;
                response.ReceiptStatus.Status = "unsuccessful";
                return response;
            }

            var receipt = BuildRceipt(isDuplicate ? pendingTrx : trax);
            PushNotification(user, model, trax.TransactionDetailsId);

            return receipt;
        }

        public async Task<TransactionDetail> ProcessTransaction(bool isDuplicate, RechargeMeterModel model, TransactionDetail tx, bool treatAsPending = false)
        {
            IceKloudResponse vendResponse = null;
            Datum vendResponseData;
            if (!isDuplicate)
            {
                if (!treatAsPending)
                    tx = await CreateRecordBeforeVend(model);

                model.UpdateRequestModel(tx);

                vendResponse = await MakeRechargeRequest(model);

                if (vendResponse.Content.Data.Error == "Error")
                {
                    throw new ArgumentException(vendResponse.Content.Data.Error);
                }

                if (vendResponse == null) throw new ArgumentException("Unable to process transaction");

                vendResponseData = vendResponse.Content.Data.Data.FirstOrDefault();
                tx.Request = JsonConvert.SerializeObject(vendResponse.RequestModel);
                tx.Response = JsonConvert.SerializeObject(vendResponse);

                if (vendResponse.Status.ToLower() != "success")
                {
                    tx.VendStatus = vendResponse?.Content?.Data?.Error;
                    tx.VendStatusDescription = vendResponse?.Content?.Data?.Error;
                    tx.StatusResponse = JsonConvert.SerializeObject(vendResponseData);
                    tx.QueryStatusCount = 1;
                    _context.TransactionDetails.AddOrUpdate(tx);
                    await _context.SaveChangesAsync();

                    ReadErrorMessage(vendResponse.Content?.Data?.Error, tx);

                    var vendStatus = await QueryVendStatus(model, tx);

                    if (vendStatus.FirstOrDefault().Key != "success" && vendStatus.FirstOrDefault().Key != "newtranx")
                    {
                        FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                        throw new ArgumentException(vendResponse.Content.Data?.Error);
                    }

                    if (vendStatus.FirstOrDefault().Key != "newtranx")
                    {
                        tx = await UpdateTransactionOnStatusSuccessIMPROVED(vendStatus.FirstOrDefault().Value, tx);
                    }
                }
                else
                {
                    tx = await UpdateTransaction(vendResponseData, tx, tx.User.POS.FirstOrDefault(d => d.POSId == tx.POSId));
                }

                tx.MeterId = await UpdateMeterOrSaveAsNewIMPROVED(model);
                return tx;
            }
            else
            {
                model.UpdateRequestModel(tx);

                var vendStatus = await QueryVendStatus(model, tx);

                if (vendStatus.FirstOrDefault().Key != "success" && vendStatus.FirstOrDefault().Key != "newtranx")
                {
                    FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                    if (vendResponse == null) throw new ArgumentException("Meter number not valid");
                    if (string.IsNullOrEmpty(vendResponse.Content.Data?.Error))
                    {
                        throw new ArgumentException("Unable to fetch sale");
                    }
                    throw new ArgumentException(vendResponse.Content.Data?.Error);
                }
                if (vendStatus.FirstOrDefault().Key != "newtranx")
                {
                    tx.MeterId = await UpdateMeterOrSaveAsNewIMPROVED(model);
                    tx = await UpdateTransactionOnStatusSuccessIMPROVED(vendStatus.FirstOrDefault().Value, tx);
                }

                return tx;
            }
        }

        private void ReadErrorMessage(string message, TransactionDetail tx)
        {
            if(message == "The request timed out with the Ouc server.")
            {
                //DisablePlatform(PlatformTypeEnum.ELECTRICITY);
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                //NotifyAdmin1();
                throw new ArgumentException(message);
            }
            if (message == "Error: Vending is disabled")
            {
                DisablePlatform(PlatformTypeEnum.ELECTRICITY);
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                NotifyAdmin1();
                throw new ArgumentException(message);
            }

            if (message == "-9137 : InCMS-BL-CB001607. Purchase not allowed, not enought vendor balance")
            {
                DisablePlatform(PlatformTypeEnum.ELECTRICITY);
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                NotifyAdmin1();
                throw new ArgumentException("Due to some technical resolutions involving EDSA, the system is unable to vend");
            }

            if (message == "InCMS-BL-CO000846. The amount is too low for recharge")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Pending);
                throw new ArgumentException("The amount is too low for recharge");
            }

            if (message == "Unexpected error in OUC VendVoucher")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Pending);
                throw new ArgumentException(message);
            }

            if (message == "CB001600 : InCMS-BL-CB001600. Error serial number, contracted service not found or not active.")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                throw new ArgumentException("Error serial number, contracted service not found or not active");
            }
            if (message == "There was an error when determining if the request for the given meter number can be processed.")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                throw new ArgumentException(message);
            }
            if (message == "Input string was not in a correct format.")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                throw new ArgumentException("Amount tendered is too low");
            }
            if (message == "-47 : InCMS-BL-CB001273. Error, purchase units less than minimum.")
            {
                FlagTransaction(tx, RechargeMeterStatusEnum.Failed);
                throw new ArgumentException("Purchase units less than minimum.");
            }
        }

        private void FlagTransaction(TransactionDetail tx, RechargeMeterStatusEnum status)
        {
            tx.Status = (int)status;
            _context.TransactionDetails.AddOrUpdate(tx);
            _context.SaveChanges();
        }
        private void DisablePlatform(PlatformTypeEnum pl)
        {
            var plt = _context.Platforms.FirstOrDefault(d => d.PlatformType == (int)pl);
            if (plt != null)
            {
                plt.DisablePlatform = true;
                _context.Platforms.AddOrUpdate(plt);
                _context.SaveChanges();
            }
        }

        void NotifyAdmin()
        {
            var body = $"Hello Victor" +
                $"This is to notify you that VENDTECH IS OUT ON FUNDS";
            Utilities.SendEmail("vblell@gmail.com", "[URGENT] VENDTECH OUT ON FUNDS", body);
        }

        void NotifyAdmin1()
        {
            var body = $"Hello Victor</br></br>" +
                $"This is to notify you that VENDTECH IServices is receiving errors from EDSA or RTS and has been disabled</br></br>" +
                $"1) VENDTECH IS OUT OF FUNDS</br></br>" +
                $"2) RTS SERVICES IS DISABLED</br></br>" +
                $"Please keep in mind to ENABLE Services again.</br></br>" +
                $"{Utilities.DomainUrl}/Admin/Platform/ManagePlatforms (ENABLE EDSA ON VENDTECH PLATFORM)";
            Utilities.SendEmail("vblell@gmail.com", "[URGENT] VENDTECH OUT ON FUNDS", body);

        }

        private async Task<TransactionDetail> getLastMeterPendingTransaction(string MeterNumber) =>
           await _context.TransactionDetails.OrderByDescending(p => p.TransactionId).FirstOrDefaultAsync(p => p.Status ==
           (int)RechargeMeterStatusEnum.Pending && p.MeterNumber1.ToLower() == MeterNumber.ToLower());

        async Task<Dictionary<string, IcekloudQueryResponse>> QueryVendStatus(RechargeMeterModel model, TransactionDetail transDetail)
        {
            LogExceptionToDatabase(new Exception($"QueryVendStatus starts at {DateTime.UtcNow} for traxId {model.TransactionId}"));
            Dictionary<string, IcekloudQueryResponse> response = new Dictionary<string, IcekloudQueryResponse>();
            var queryRequest = model.StackStatusRequestModel(model);
            var url = WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

            var icekloud_response = await _client.PostAsJsonAsync(url, queryRequest);

            var strings_result = await icekloud_response.Content.ReadAsStringAsync();

            var statusResponse = JsonConvert.DeserializeObject<IcekloudQueryResponse>(strings_result);

            if (statusResponse.Content.StatusDescription == "The specified Transaction does not exist.")
            {
                //await ProcessTransaction(false, model, transDetail, true);
                response.Add("failed", statusResponse);

                LogExceptionToDatabase(new Exception($"QueryVendStatus 1 ends at {DateTime.UtcNow} for traxId {model.TransactionId}"));
                return response;
            }
            else if (statusResponse.Content.StatusDescription == "Transaction completed with error")
            {
                var newTraxid = Utilities.GetLastMeterRechardeId();
                model.TransactionId = Convert.ToInt64(newTraxid);
                //await ProcessTransaction(false, model, transDetail, false);
                //response.Add("newtranx", statusResponse);
                response.Add("failed", statusResponse);
                LogExceptionToDatabase(new Exception($"QueryVendStatus 2 ends at {DateTime.UtcNow} for traxId {model.TransactionId}"));
                return response;
            }
            else if (!statusResponse.Content.Finalised && statusResponse.Content.StatusRequestCount <= 5)
            {
                LogExceptionToDatabase(new Exception($"QueryVendStatus 3 ends at {DateTime.UtcNow} for traxId {model.TransactionId}"));
                return await QueryVendStatus(model, transDetail);
            }
            else
            {
                transDetail.QueryStatusCount = (int)statusResponse.Content.StatusRequestCount;
                if (string.IsNullOrEmpty(statusResponse.Content.VoucherPin))
                {
                    LogExceptionToDatabase(new Exception($"QueryVendStatus 4 ends at {DateTime.UtcNow} for traxId {model.TransactionId}"));
                    await _context.SaveChangesAsync();
                    response.Add("failed", statusResponse);
                    return response;
                }
                else
                {
                    await _context.SaveChangesAsync();
                    LogExceptionToDatabase(new Exception($"QueryVendStatus 5 ends at {DateTime.UtcNow} for traxId {model.TransactionId}"));
                    response.Add("success", statusResponse);
                    return response;
                }
            }
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
            receipt.Tarrif = model.Tariff != "" ? Utilities.FormatAmount(Convert.ToDecimal(model.Tariff)) : "0";
            receipt.DeviceNumber = model?.MeterNumber1;
            receipt.DebitRecovery = Convert.ToDecimal(model.DebitRecovery);
            var amt = model?.TenderedAmount.ToString("N");
            receipt.Amount = amt.Contains('.') ? amt.TrimEnd('0').TrimEnd('.') : amt;
            receipt.Charges = model.ServiceCharge != "" ? Utilities.FormatAmount(Convert.ToDecimal(model.ServiceCharge)) : "0";
            receipt.Commission = string.Format("{0:N0}", 0.00);
            receipt.Unit = model.Units != "" ? Utilities.FormatAmount(Convert.ToDecimal(model.Units)) : "0";
            receipt.UnitCost = model.CostOfUnits != "" ? Utilities.FormatAmount(Convert.ToDecimal(model.CostOfUnits)) : "0";
            receipt.SerialNo = model?.SerialNumber;
            receipt.Pin1 = Utilities.FormatThisToken(model?.MeterToken1) ?? string.Empty;
            receipt.Pin2 = Utilities.FormatThisToken(model?.MeterToken2) ?? string.Empty;
            receipt.Pin3 = Utilities.FormatThisToken(model?.MeterToken3) ?? string.Empty;
            receipt.Discount = string.Format("{0:N0}", 0);
            receipt.Tax = model.TaxCharge != "" ? Utilities.FormatAmount(Convert.ToDecimal(model.TaxCharge)) : "0";
            receipt.TransactionDate = model.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            receipt.VendorId = model.User?.Vendor;
            receipt.EDSASerial = model.SerialNumber;
            receipt.VTECHSerial = model.TransactionId;
            receipt.PlatformId = model.PlatFormId;

            receipt.ShouldShowSmsButton = (bool)model.POS.WebSms;
            receipt.ShouldShowPrintButton = (bool)model.POS.WebPrint;
            receipt.mobileShowSmsButton = (bool)model.POS.PosSms;
            receipt.mobileShowPrintButton = (bool)model.POS.PosPrint;
            receipt.CurrentBallance = model?.POS?.Balance ?? 0;
            return receipt;
        }

        private void PushNotification(User user, RechargeMeterModel model, long MeterRechargeId)
        {
            var deviceTokens = user.TokensManagers.Where(p => p.DeviceToken != null && p.DeviceToken != string.Empty).Select(p => new { p.AppType, p.DeviceToken }).ToList().Distinct(); ;
            var obj = new PushNotificationModel();
            obj.UserId = model.UserId;
            obj.Id = MeterRechargeId;
            obj.Title = "Meter recharged successfully";
            obj.Message = $"Your meter has successfully recharged with NLe {Utilities.FormatAmount(model.Amount)} PIN: {model.MeterToken1}{model.MeterToken2}{model.MeterToken3}";
            obj.NotificationType = NotificationTypeEnum.MeterRecharge;
            foreach (var item in deviceTokens)
            {
                obj.DeviceToken = item.DeviceToken;
                obj.DeviceType = item.AppType.Value;
                Common.PushNotification.SendNotification(obj);
            }
        }

        private async Task<TransactionDetail> UpdateTransactionOnStatusSuccessIMPROVED(IcekloudQueryResponse response_data, TransactionDetail trans)
        {
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
            //BALANCE DEDUCTION
            await Deductbalace(trans, trans.User.POS.FirstOrDefault(s => s.POSId == trans.POSId));
            return trans;

        }
        private async Task<TransactionDetail> UpdateTransaction(Datum response_data, TransactionDetail trans, POS pos)
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
            await Deductbalace(trans, pos);

            return trans;
        }

        async Task Deductbalace(TransactionDetail trans, POS pos)
        {
            //BALANCE DEDUCTION
            trans.BalanceBefore = pos.Balance ?? 0;
            pos.Balance = (pos.Balance - trans.Amount);
            trans.CurrentVendorBalance = pos.Balance ?? 0;

            _context.TransactionDetails.AddOrUpdate(trans);
            _context.POS.AddOrUpdate(pos);
            await _context.SaveChangesAsync();
        }

        private async Task<TransactionDetail> CreateRecordBeforeVend(RechargeMeterModel model)
        {
            var trans = new TransactionDetail();
            trans.PlatFormId = (int)model.PlatformId;
            trans.UserId = model.UserId;
            trans.MeterId = model.MeterId;
            trans.POSId = model.POSId;
            trans.MeterNumber1 = model.MeterNumber;
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

            _context.TransactionDetails.Add(trans);
            await _context.SaveChangesAsync();

            return trans;
        }

        ActionOutput<MeterRechargeApiListingModel> IMeterManager.GetRechargeDetail(long rechargeId)
        {
            var recharge = _context.TransactionDetails.FirstOrDefault(p => p.TransactionDetailsId == rechargeId);
            if (recharge == null)
                return ReturnError<MeterRechargeApiListingModel>("Recharge not exist.");
            var data = new MeterRechargeApiListingModel();
            data.Amount = Utilities.FormatAmount(recharge.Amount);
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
            var thisTransactionNotification = _context.Notifications.FirstOrDefault(d => d.Type == (int)NotificationTypeEnum.MeterRecharge && d.RowId == rechargeId);
            if (thisTransactionNotification != null)
            {
                thisTransactionNotification.MarkAsRead = true;
                _context.SaveChanges();
            }
            return ReturnSuccess<MeterRechargeApiListingModel>(data, "Recharge detail fetched successfully.");

        }

        ActionOutput<MeterRechargeApiListingModelMobile> IMeterManager.GetMobileRechargeDetail(long rechargeId)
        {
            var recharge = _context.TransactionDetails.FirstOrDefault(p => p.TransactionDetailsId == rechargeId);
            if (recharge == null)
                return ReturnError<MeterRechargeApiListingModelMobile>("Recharge not exist.");
            var data = new MeterRechargeApiListingModelMobile();
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
            var thisTransactionNotification = _context.Notifications.FirstOrDefault(d => d.RowId == rechargeId);
            if (thisTransactionNotification != null)
            {
                thisTransactionNotification.MarkAsRead = true;
                _context.SaveChanges();
            }
            return ReturnSuccess<MeterRechargeApiListingModelMobile>(data, "Recharge detail fetched successfully.");

        }
        RechargeDetailPDFData IMeterManager.GetRechargePDFData(long rechargeId)
        {
            var recharge = _context.TransactionDetails.FirstOrDefault(p => p.TransactionDetailsId == rechargeId);
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

        private async Task<IceKloudResponse> MakeRechargeRequest(RechargeMeterModel model)
        {
            IceKloudResponse response = new IceKloudResponse();
            string strings_result = "";
            IcekloudRequestmodel request_model = null;
            string url = (WebConfigurationManager.AppSettings["IsDevelopment"].ToString() == "1") ?
                         WebConfigurationManager.AppSettings["DevIcekloudURL"].ToString() :
                         WebConfigurationManager.AppSettings["IcekloudURL"].ToString();

            try
            {
                request_model = Buid_new_request_object(model);

                HttpResponseMessage icekloud_response = await _client.PostAsJsonAsync(url, request_model);

                strings_result = await icekloud_response.Content.ReadAsStringAsync();
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
                    LogExceptionToDatabase(new Exception($"MakeRechargeRequest ERROR {DateTime.UtcNow} for traxId {model.TransactionId} see: {strings_result}"));
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
                            return await MakeRechargeRequest(model);
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
        public ReceiptModel Build_receipt_model_from_dbtransaction_detail(TransactionDetail model)
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
            receipt.CurrencyCode = Utilities.GetCountry().CurrencyCode;
            return receipt;
        }
        ReceiptModel IMeterManager.ReturnVoucherReceipt(string token)
        {
            token = BLL.Common.Utilities.ReplaceWhitespace(token, "");
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

        async Task<ReceiptModel> IMeterManager.ReturnTraxStatusReceiptAsync(string trxId)
        {
            var response = new ReceiptModel { ReceiptStatus = new ReceiptStatus { Status = "", Message = "" } };
            try
            {
                var pendingTrax = _context.TransactionDetails.FirstOrDefault(e => e.TransactionId == trxId);

                if (pendingTrax == null)
                {
                    response.ReceiptStatus.Status = "unsuccessful";
                    response.ReceiptStatus.Message = "Unable to find transaction";
                    return response;
                }

                var requestModel = new RechargeMeterModel
                {
                    UserId = pendingTrax.UserId,
                    TransactionId = Convert.ToInt64(pendingTrax.TransactionId),
                };

                var verifiedTrax = await ProcessTransaction(true, requestModel, pendingTrax, true);

                if (verifiedTrax != null)
                {
                    var receipt = Build_receipt_model_from_dbtransaction_detail(verifiedTrax);
                    receipt.ShouldShowSmsButton = (bool)verifiedTrax.POS.WebSms;
                    receipt.ShouldShowPrintButton = (bool)verifiedTrax.POS.WebPrint;
                    receipt.mobileShowSmsButton = (bool)verifiedTrax.POS.PosSms;
                    receipt.mobileShowPrintButton = (bool)verifiedTrax.POS.PosPrint;
                    return receipt;
                }

                return response;
            }
            catch (ArgumentException ex)
            {
                response.ReceiptStatus.Status = "unsuccessful";
                response.ReceiptStatus.Message = ex.Message;
                return response;
            }
        }
        RequestResponse IMeterManager.ReturnRequestANDResponseJSON(string token)
        {
            var transaction_by_token = _context.TransactionDetails.Where(e => e.TransactionId == token).ToList().FirstOrDefault();
            if (transaction_by_token != null)
            {
                var receipt = new RequestResponse { Request = transaction_by_token.Request, Response = transaction_by_token.Response };
                return receipt;
            }
            return new RequestResponse { ReceiptStatus = new ReceiptStatus { Status = "unsuccessful", Message = "Unable to find voucher" } };
        }

        TransactionDetail IMeterManager.GetLastTransaction()
        {
            var lstTr = _context.TransactionDetails.Where(e => e.Status == (int)RechargeMeterStatusEnum.Success
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
            var lstTr = _context.TransactionDetails.FirstOrDefault(d => d.MeterToken1 == transactionId.Trim()) ?? null;
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
            _context.SMS_LOG.Add(smsObj);
            _context.SaveChanges();
        }

        IQueryable<BalanceSheetListingModel> IMeterManager.GetBalanceSheetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId)
        {
            model.RecordsPerPage = 999999999;
            var result = new PagingResult<BalanceSheetListingModel>();
            IQueryable<BalanceSheetListingModel> query = null;
            if (model.IsInitialLoad)
            {
                query = from a in _context.TransactionDetails
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
                query = from a in _context.TransactionDetails
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
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        //posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == model.VendorId || p.User.AgentId == agentId).Select(p => p.POSId).ToList();
                        posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
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
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                    posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                query = query.Where(p => posIds.Contains(p.POSId ?? 0));
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
            var we = GetVendorStatus();
            return _context.TransactionDetails
                 .Where(d => d.Finalised == true && d.Status == 1)
                 .GroupBy(f => f.UserId)
                 .Select(f =>
                     new DashboardBalanceSheetModel
                     {
                         SaleAmount = f.Sum(d => d.Amount),
                         Vendor = f.FirstOrDefault().User.Vendor,
                         UserId = f.FirstOrDefault().UserId,
                         Balance = 0,
                         DepositAmount = 0,
                         Status = "",
                         POSBalance = f.OrderByDescending(a => a.POS.Balance).FirstOrDefault().POS.Balance ?? 0
                     });
        }

        public PagingResult<VendorStatus> GetVendorStatus()
        {
            var res = new PagingResult<VendorStatus>();
            try
            {
                var date = DateTime.Parse("2022/07/01");
                var query = (from tbl in (from Deposits in _context.Deposits
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
                        ).Concat(from TransactionDetails in _context.TransactionDetails
                                 where TransactionDetails.Status == 1
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

                             join POS in _context.POS on new { VendorId = tbl.UserId } equals new { VendorId = POS.VendorId ?? 0 }
                             join Users in _context.Users on new { UserId = (long)tbl.UserId } equals new { UserId = Users.UserId }
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

        public PagingResult<VendorStatus> RunStoredProcParams()
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            var res = new PagingResult<VendorStatus>();

            var vendorStatus = new List<VendorStatus>();
            var server = WebConfigurationManager.AppSettings["serverName"].ToString();
            var db = WebConfigurationManager.AppSettings["dbName"].ToString();
            var user = WebConfigurationManager.AppSettings["dbUser"].ToString();
            var password = WebConfigurationManager.AppSettings["dbPassword"].ToString();
            try
            {
                conn = new SqlConnection($"Data Source={server};Initial Catalog={db};user id={user};password={password};MultipleActiveResultSets=True;");
                conn.Open();

                SqlCommand cmd = new SqlCommand("CalculateRunningBalance", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var status = new VendorStatus();
                    status.overage = Convert.ToDecimal(rdr["overage"]);
                    status.PercentageAmount = Convert.ToDecimal(rdr["PercentageAmount"]);
                    status.POSBalance = Convert.ToDecimal(rdr["POSBalance"]);
                    status.runningbalance = Convert.ToDecimal(rdr["runningbalance"]);
                    status.totalsales = Convert.ToDecimal(rdr["totalsales"]);
                    status.userid = Convert.ToInt64(rdr["userid"]);
                    status.vendor = rdr["vendor"].ToString();

                    vendorStatus.Add(status);
                }
            }

            catch (Exception)
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            res.List = vendorStatus;
            return res;
        }

        void IMeterManager.RedenominateBalnces()
        {
            try
            {
                var posBalances = _context.TransactionDetails.Where(d => d.Amount != null && d.Amount > 0).ToList();

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

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        decimal IMeterManager.ReturnElectricityMinVend()
        {
            return _context.Platforms.FirstOrDefault(d => d.PlatformType == (int)PlatformTypeEnum.ELECTRICITY).MinimumAmount;
        }

        decimal IMeterManager.ReturnAirtimeMinVend()
        {
            return _context.Platforms.FirstOrDefault(d => d.PlatformType == (int)PlatformTypeEnum.AIRTIME).MinimumAmount;
        }

        private async Task<long> UpdateMeterOrSaveAsNewIMPROVED(RechargeMeterModel model)
        {
            long meterId = 0;
            if (model.SaveAsNewMeter)
            {
                MeterModel newMeter = StackNewMeterToDbObject(model);
                newMeter.IsSaved = true;
                meterId = await Task.Run(() => (this as IMeterManager).SaveMeter(newMeter).ID);
                meterId = meterId != 0 ? meterId : 0;
            }
            else
                meterId = model.MeterId ?? 0;
            return meterId;
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
        PagingResult<MiniSalesReport> IMeterManager.GetMiniSalesReport(ReportSearchModel model, bool callFromAdmin, long agentId, string type)
        {
            model.RecordsPerPage = 10000000;
            var result = new PagingResult<MiniSalesReport>();

            IQueryable<TransactionDetail> query = null;

            query = _context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);
            if (model.VendorId > 0)
            {
                var user = _context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                var posIds = new List<long>();
                if (callFromAdmin)
                    posIds = _context.POS.Where(p => p.VendorId == model.VendorId).Select(p => p.POSId).ToList();
                else
                {
                    if (user.Status == (int)UserStatusEnum.Active)
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId) || p.User.AgentId == agentId && p.Enabled == true).Select(p => p.POSId).ToList();
                    }
                    else
                    {
                        posIds = _context.POS.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId).Select(p => p.POSId).ToList();
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

            if (!string.IsNullOrEmpty(model.Product))
            {
                int prod = Convert.ToInt32(model.Product);
                if (prod > 0)
                    query = query.Where(p => p.PlatFormId == prod);
            }

            if (type == "daily")
            {
                var dailyRp = query.GroupBy(i => DbFunctions.TruncateTime(i.CreatedAt)).AsEnumerable().Select(d => new MiniSalesReport
                {
                    DateTime = d.First().CreatedAt.ToString("dd/MM/yyyy").Substring(0, 2) + " - " + d.Last().CreatedAt.ToString("dd/MM/yyyy"),
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

    }
}
