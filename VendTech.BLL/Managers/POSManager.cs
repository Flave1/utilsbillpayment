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
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace VendTech.BLL.Managers
{
    public class POSManager : BaseManager, IPOSManager
    {

        PagingResult<POSListingModel> IPOSManager.GetPOSPagedList(PagingModel model, long agentId, long vendorId, bool callForGetVendorPos)
        {
            var result = new PagingResult<POSListingModel>();
            var query = Context.POS.Where(p => !p.IsDeleted).OrderBy("SerialNumber" + " " + "Asc").AsEnumerable();
            //if (agentId > 0)
            //    query = query.Where(p => p.Vendor.AgencyId == agentId);
            if (vendorId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == vendorId);
                if (callForGetVendorPos)
                    query = query.Where(p => p.VendorId == user.UserId);
                else
                    query = query.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId);
            }
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.SerialNumber.ToLower().Contains(model.Search.ToLower()) || z.Vendor.Name.ToLower().Contains(model.Search.ToLower()) || z.Vendor.SurName.ToLower().Contains(model.Search.ToLower()) || z.Phone.Contains(model.Search) || ((PosTypeEnum)z.VendorType).ToString().ToLower().Contains(model.Search.ToLower()) || z.Enabled.ToString().ToLower().Contains(model.Search.ToLower()));

                if (model.SearchField.Equals("POS"))
                    query = query.Where(z => z.SerialNumber.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("VENDOR"))
                    query = query.Where(z => z.User.Vendor.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("CELL"))
                    query = query.Where(z => z.Phone.Contains(model.Search));
                else if (model.SearchField.Equals("POS"))
                    query = query.Where(z => ((PosTypeEnum)z.VendorType).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("ENABLED"))
                    query = query.Where(z => z.Enabled.ToString().ToLower().Contains(model.Search.ToLower()));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new POSListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "POS List";
            result.TotalCount = query.Count();
            return result;
        }
        PagingResult<POSListingModel> IPOSManager.GetUserPosPagingListForApp(int pageNo, int pageSize, long userId)
        {
            var result = new PagingResult<POSListingModel>();
            var query = Context.POS.Where(p => !p.IsDeleted).OrderBy("SerialNumber" + " " + "Asc").AsEnumerable();
            //if (agentId > 0)
            //    query = query.Where(p => p.Vendor.AgencyId == agentId);
            if (userId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
                query = query.Where(p => p.VendorId != null && p.VendorId == user.FKVendorId);
            }

            var list = query
               .Skip((pageNo - 1) * pageSize).Take(pageSize)
               .ToList().Select(x => new POSListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "POS List Fetched Successfully";
            result.TotalCount = Convert.ToInt32(list.Sum(x => x.Balance));
            return result;
        }
        List<PosAPiListingModel> IPOSManager.GetPOSSelectListForApi(long userId = 0)
        {
            var query = Context.POS.Where(p => !p.IsDeleted && p.Enabled != false);
            // enable check has been removed
            //   var query = Context.POS.Where(p => !p.IsDeleted);
            var userPos = new List<POS>();
            if (userId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == userId);

                query = query.Where(p => (p.VendorId != null && p.VendorId == user.FKVendorId));
            }
            return query.ToList().OrderBy(p => p.SerialNumber).Select(p => new PosAPiListingModel(p)
           ).ToList();
        }
        List<SelectListItem> IPOSManager.GetVendorPos(long userId)
        {
            var query = Context.POS.Where(p => !p.IsDeleted && p.Enabled != false);
            // var query = Context.POS.Where(p => !p.IsDeleted);
            var userPos = new List<POS>();
            if (userId > 0)
                query = query.Where(p => (p.VendorId != null && p.VendorId == userId));
            var list = query.ToList();
            return list.OrderBy(p => p.SerialNumber).Select(p => new SelectListItem
            {
                Text = p.SerialNumber.ToUpper(),
                Value = p.POSId.ToString()
            }).ToList();
        }

        List<SelectListItem> IPOSManager.GetPOSSelectList(long userId)
        {
            var query = new List<POS>();
            //var query = Context.POS.Where(p => !p.IsDeleted);
            var userPos = new List<POS>();
            if (userId > 0)
            {
                var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
                if (user != null) query = Context.POS.Where(p => (p.VendorId != null && p.VendorId == user.FKVendorId && p.Enabled != false && !p.IsDeleted)).ToList();
            }
            else
                query = Context.POS.Where(p => !p.IsDeleted && p.Enabled != false).ToList();

            return query.OrderBy(p => p.SerialNumber).Select(p => new SelectListItem
            {
                Text = p.SerialNumber.ToUpper(),
                Value = p.POSId.ToString()
            }).ToList();
        }

        SavePosModel IPOSManager.GetPosDetail(long posId)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.POSId == posId);

            if (dbPos == null)
                return null;
            return new SavePosModel()
            {
                SerialNumber = dbPos.SerialNumber,
                VendorId = dbPos.VendorId,
                Phone = dbPos.Phone,
                Type = dbPos.VendorType,
                POSId = dbPos.POSId,
                Percentage = dbPos.CommissionPercentage,
                Enabled = dbPos.Enabled == null ? false : dbPos.Enabled.Value,
                SMSNotificationDeposit = dbPos.SMSNotificationDeposit == null ? false : dbPos.SMSNotificationDeposit.Value,
                EmailNotificationDeposit = dbPos.EmailNotificationDeposit == null ? false : dbPos.EmailNotificationDeposit.Value,
                EmailNotificationSales = dbPos.EmailNotificationSales == null ? false : dbPos.EmailNotificationSales.Value,
                SMSNotificationSales = dbPos.SMSNotificationSales == null ? false : dbPos.SMSNotificationSales.Value,
                CountryCode = dbPos.CountryCode,
                PassCode = dbPos.PassCode,
                Email = Context.Users.FirstOrDefault(x => x.UserId == dbPos.VendorId).Email
            };
        }
        SavePosModel IPOSManager.GetPosDetails(string passCode)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.PassCode == passCode);

            if (dbPos == null)
                return null;
            return new SavePosModel()
            {
                SerialNumber = dbPos.SerialNumber,
                VendorId = dbPos.VendorId,
                Phone = dbPos.Phone,
                Type = dbPos.VendorType,
                POSId = dbPos.POSId,
                Percentage = dbPos.CommissionPercentage,
                Enabled = dbPos.Enabled == null ? false : dbPos.Enabled.Value,
                SMSNotificationDeposit = dbPos.SMSNotificationDeposit == null ? false : dbPos.SMSNotificationDeposit.Value,
                EmailNotificationDeposit = dbPos.EmailNotificationDeposit == null ? false : dbPos.EmailNotificationDeposit.Value,
                EmailNotificationSales = dbPos.EmailNotificationSales == null ? false : dbPos.EmailNotificationSales.Value,
                SMSNotificationSales = dbPos.SMSNotificationSales == null ? false : dbPos.SMSNotificationSales.Value,
                CountryCode = dbPos.CountryCode,
                PassCode = dbPos.PassCode,
                Email = Context.Users.FirstOrDefault(x => x.UserId == dbPos.VendorId).Email
            };
        }

        UserModel IPOSManager.GetUserPosDetails(string posSerialNumber)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.SerialNumber == posSerialNumber);

            if (dbPos == null)
                return null;
            var user = Context.Users.FirstOrDefault(x => x.UserId == dbPos.VendorId);
            return new UserModel()
            {
                Email = user.Email,
                UserId = user.UserId,
                FirstName = user.Name,
                Phone = user.Phone
            };
        }
        ActionOutput IPOSManager.DeletePos(long posId)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.POSId == posId);
            if (dbPos == null)
                return ReturnError("POS not exist");
            dbPos.IsDeleted = true;
           // EnableOrdisablePOSAccount(false, dbPos.POSId);
            Context.SaveChanges();
            return ReturnSuccess("POS deleted successfully.");
        }

        ActionOutput IPOSManager.ChangePOSStatus(int posId, bool value)
        {
            var pos = Context.POS.Where(z => z.POSId == posId).FirstOrDefault();
            if (pos == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "POS Not Exist."
                };
            }
            else
            {
                pos.Enabled = value;
               // EnableOrdisablePOSAccount(value, pos.POSId);
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "POS status changed Successfully."
                };
            }
        }

        decimal IPOSManager.GetPosCommissionPercentage(long posId)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.POSId == posId);
            if (dbPos == null || dbPos.CommissionPercentage == null)
                return 0;
            return dbPos.Commission.Percentage;
        }
        decimal IPOSManager.GetPosBalance(long posId)
        {
            var dbPos = Context.POS.FirstOrDefault(p => p.POSId == posId);
            if (dbPos == null || dbPos.Balance == null)
                return 0;
            return dbPos.Balance.Value;
        }
        decimal IPOSManager.GetPosCommissionPercentageByUserId(long userId)
        {
            var userObj = Context.Users.FirstOrDefault(p => p.UserId == userId);
            var userAssignedPos = new POS();
            if (userObj.UserRole.Role == UserRoles.Vendor)
                userAssignedPos = userObj.POS.FirstOrDefault();
            else if (userObj.UserRole.Role == UserRoles.AppUser && userObj.User1 != null)
                userAssignedPos = userObj.User1.POS.FirstOrDefault();

            if (userAssignedPos != null && userAssignedPos.Commission != null)
                return userAssignedPos.Commission.Percentage;
            return 0;
        }

        IList<PlatformCheckbox> IPOSManager.GetAllPlatforms(long posId)
        {
            IList<PlatformCheckbox> chekboxListOfModules = null;
            IList<Platform> modules = Context.Platforms.Where(p => !p.IsDeleted && p.Enabled).ToList();
            if (modules.Count() > 0)
            {
                chekboxListOfModules = modules.Select(x =>
                {
                    return new PlatformCheckbox()
                    {
                        Id = x.PlatformId,
                        Title = x.Title,
                        Checked = false
                    };
                }).ToList();

                if (posId > 0)
                {
                    var existingPermissons = Context.POSAssignedPlatforms.Where(x => x.POSId == posId).ToList();
                    if (existingPermissons.Count > 0)
                    {
                        chekboxListOfModules.ToList().ForEach(x => x.Checked = existingPermissons.Where(z => z.PlatformId == x.Id).Any());
                    }
                }
            }
            return chekboxListOfModules;
        }
        ActionOutput IPOSManager.SavePos(SavePosModel model)
        {
            var dbPos = new POS();
            if (model.POSId > 0)
            {
                dbPos = Context.POS.FirstOrDefault(p => p.POSId == model.POSId);
                if (dbPos == null)
                    return ReturnError("Pos not exist");
            }
            dbPos.SerialNumber = model.SerialNumber;
            dbPos.VendorId = model.VendorId != null ? model.VendorId : dbPos.VendorId;
            dbPos.VendorType = model.Type;
            dbPos.Phone = model.Phone;
            dbPos.Enabled = model.Enabled;
            dbPos.SMSNotificationDeposit = model.SMSNotificationDeposit;
            dbPos.SMSNotificationSales = model.SMSNotificationSales;
            dbPos.EmailNotificationSales = model.EmailNotificationSales;
            dbPos.EmailNotificationDeposit = model.EmailNotificationDeposit;
            dbPos.CountryCode = model.CountryCode;
            dbPos.CreatedAt = DateTime.UtcNow;
            dbPos.CommissionPercentage = model.Percentage;
            dbPos.IsDeleted = false;

            if (model.POSId == 0)
                Context.POS.Add(dbPos);
            Context.SaveChanges();
            //EnableOrdisablePOSAccount(model.Enabled, model.POSId);
            //Deleting Exisiting Platforms
            var existingPlatforms = Context.POSAssignedPlatforms.Where(x => x.POSId == dbPos.POSId).ToList();
            if (existingPlatforms.Count > 0)
            {
                Context.POSAssignedPlatforms.RemoveRange(existingPlatforms);
                Context.SaveChanges();
            }
            List<POSAssignedPlatform> newPlatforms = new List<POSAssignedPlatform>();

            if (model.SelectedPlatforms != null)
            {
                model.SelectedPlatforms.ToList().ForEach(c =>
                 newPlatforms.Add(new POSAssignedPlatform()
                 {
                     POSId = dbPos.POSId,
                     PlatformId = c,
                     CreatedAt = DateTime.UtcNow,
                 }));
                Context.POSAssignedPlatforms.AddRange(newPlatforms);
                Context.SaveChanges();
            }
            return ReturnSuccess("Pos saved successfully.");
        }
        //void EnableOrdisablePOSAccount(bool isEnabled, long posId)
        //{
        //    var pos = Context.POS.Where(z => z.POSId == posId).FirstOrDefault();
        //    if (pos != null)
        //    {
        //        var posUserAccount = pos.User;
        //        if (posUserAccount != null && !isEnabled)
        //            posUserAccount.Status = (int)UserStatusEnum.Block;
        //        else if (posUserAccount != null && isEnabled)
        //            posUserAccount.Status = (int)UserStatusEnum.Active;
        //    }
        //}
        ActionOutput IPOSManager.SavePasscodePos(SavePassCodeModel savePassCodeModel)
        {
            var dbPos = new POS();
            if (savePassCodeModel.POSId > 0)
            {
                dbPos = Context.POS.FirstOrDefault(p => p.POSId == savePassCodeModel.POSId);
                if (dbPos == null)
                    return ReturnError("Pos does not exist");
            }
            else if (!string.IsNullOrEmpty(savePassCodeModel.Phone))
            {
                dbPos = Context.POS.FirstOrDefault(p => p.Phone == savePassCodeModel.Phone);
                if (dbPos == null)
                    return ReturnError("Pos does not exist");
            }
            dbPos.CountryCode = !string.IsNullOrEmpty(savePassCodeModel.CountryCode) ? savePassCodeModel.CountryCode : dbPos.CountryCode;
            dbPos.PassCode = savePassCodeModel.PassCode;
            if (savePassCodeModel.POSId == 0)
                Context.POS.Add(dbPos);
            Context.SaveChanges();
            return ReturnSuccess("Passcode saved successfully.");
        }

        ActionOutput IPOSManager.SavePasscodePosApi(SavePassCodeModel savePassCodeModel)
        {
            var dbPos = new POS();
            if (!string.IsNullOrEmpty(savePassCodeModel.PosNumber))
            {
                dbPos = Context.POS.FirstOrDefault(p => p.SerialNumber == savePassCodeModel.PosNumber);
                if (dbPos == null)
                    return ReturnError("Pos does not exist");
            }
            else if (!string.IsNullOrEmpty(savePassCodeModel.Phone))
            {
                dbPos = Context.POS.FirstOrDefault(p => p.Phone == savePassCodeModel.Phone);
                if (dbPos == null)
                    return ReturnError("Pos does not exist");
            }
            dbPos.CountryCode = !string.IsNullOrEmpty(savePassCodeModel.CountryCode) ? savePassCodeModel.CountryCode : dbPos.CountryCode;
            dbPos.PassCode = savePassCodeModel.PassCode;
            if (dbPos.POSId == 0)
                Context.POS.Add(dbPos);
            Context.SaveChanges();
            return ReturnSuccess("Passcode saved successfully.");
        }

    }
}