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
    public class VendorManager : BaseManager, IVendorManager
    {

        PagingResult<VendorListingModel> IVendorManager.GetVendorsPagedList(PagingModel model, long agentId)
        {
            var result = new PagingResult<VendorListingModel>();
            var query = Context.Users.Where(p => p.UserRole.Role == UserRoles.Vendor && p.Status != (int)UserStatusEnum.Deleted).OrderBy(model.SortBy + " " + model.SortOrder);
            if (agentId > 0)
                query = query.Where(p => p.AgentId == agentId);
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("vendorname"))
                    query = query.Where(z => z.Vendor.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("agency"))
                    query = query.Where(z => z.Agency.AgencyName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("fisrtname"))
                    query = query.Where(z => z.Name.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("lastname"))
                    query = query.Where(z => z.SurName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("phone"))
                    query = query.Where(z => z.Phone.ToLower().Contains(model.Search.ToLower()));
            }

            var list = query.Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new VendorListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Vendor List";
            result.TotalCount = query.Count();
            return result;
        }
        List<POSListingModel> IVendorManager.GetVendorPOS(long vendorId)
        {
            return Context.POS.Where(p => !p.IsDeleted && p.VendorId == vendorId).OrderByDescending(p => p.CreatedAt).ToList().Select(p => new POSListingModel(p)).ToList();
        }
        SaveVendorModel IVendorManager.GetVendorDetail(long vendorId)
        {
            var vendor = Context.Users.FirstOrDefault(p => p.UserId == vendorId);
            if (vendor == null)
                return null;
            return new SaveVendorModel()
            {
                Vendor = vendor.Vendor,
                VendorId = vendor.UserId,
                Name = vendor.Name,
                SurName = vendor.SurName,
                AgencyId = vendor.AgentId == null ? 0 : vendor.AgentId.Value,
                Password = Utilities.DecryptPassword(vendor.Password),
                ConfirmPassword = Utilities.DecryptPassword(vendor.Password),
                Phone = vendor.Phone,
                Email = vendor.Email,
                AgentPercentage = vendor.Commission != null ? vendor.Commission.Percentage : 0,
                Percentage = vendor.VendorCommissionPercentage,
                VendorType = vendor.VendorType,
                Address = vendor.Address,
                City = vendor.CityId.ToString(),
                Country = vendor.CountryId.ToString()
                //POSId=vendor.FKPOSId
            };
        }

        SaveVendorModel IVendorManager.GetVendorDetailApi(long vendorId)
        {
            var vendor = Context.Users.FirstOrDefault(p => p.FKVendorId == vendorId && p.Status == (int)UserStatusEnum.Active);
            if (vendor == null)
            {
                vendor = Context.Users.FirstOrDefault(p => p.UserId == vendorId);
            }
            if (vendor == null)
                return null;
            return new SaveVendorModel()
            {
                Vendor = vendor.Vendor,
                VendorId = vendor.UserId,
                Name = vendor.Name,
                SurName = vendor.SurName,
                AgencyId = vendor.AgentId == null ? 0 : vendor.AgentId.Value,
                Password = Utilities.DecryptPassword(vendor.Password),
                ConfirmPassword = Utilities.DecryptPassword(vendor.Password),
                Phone = vendor.Phone,
                Email = vendor.Email,
                AgentPercentage = vendor.Commission != null ? vendor.Commission.Percentage : 0,
                Percentage = vendor.VendorCommissionPercentage,
                VendorType = vendor.VendorType,
                Address = vendor.Address,
                City = vendor.CityId.ToString(),
                Country = vendor.CountryId.ToString()
                //POSId=vendor.FKPOSId
            };
        }

        SaveVendorModel IVendorManager.GetVendorDetailByPosId(long posId)
        {
            var vendor = Context.POS.FirstOrDefault(p => p.POSId == posId).User;
            if (vendor == null)
                return null;
            return new SaveVendorModel()
            {
                Vendor = vendor.Vendor,
                VendorId = vendor.UserId,
                Name = vendor.Name,
                SurName = vendor.SurName,
                AgencyId = vendor.AgentId.Value,
                Password = Utilities.DecryptPassword(vendor.Password),
                ConfirmPassword = Utilities.DecryptPassword(vendor.Password),
                Phone = vendor.Phone,
                Email = vendor.Email,
                AgentPercentage = vendor.Commission != null ? vendor.Commission.Percentage : 0,
                Percentage = vendor.VendorCommissionPercentage,
                VendorType = vendor.VendorType,
                //POSId = vendor.FKPOSId
            };
        }
        decimal IVendorManager.GetVendorPendingDepositTotal(long vendorId)
        {
            if (vendorId > 0)
            {
                if (Context.Deposits.Where(p => p.POSId == vendorId && p.Status == (int)DepositPaymentStatusEnum.Pending && p.POS.Enabled != false).ToList().Count() > 0)

                    return Context.Deposits.Where(p => p.POSId == vendorId && p.Status == (int)DepositPaymentStatusEnum.Pending && p.POS.Enabled != false).ToList().Sum(p => p.PercentageAmount).Value;
                return 0;
            }
            else
            {
                if (Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.POS.Enabled != false).ToList().Count() > 0)
                    return Context.Deposits.Where(p => p.Status == (int)DepositPaymentStatusEnum.Pending && p.POS.Enabled != false).ToList().Sum(p => p.PercentageAmount).Value;
                return 0;
            }


        }
        ActionOutput IVendorManager.DeleteVendor(long userId)
        {
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User Not Exist."
                };
            }
            else
            {
                user.Status = (int)UserStatusEnum.Deleted;
                var otherUsers = Context.Users.Where(p => p.FKVendorId == userId).ToList();
                otherUsers.ForEach(x => x.FKVendorId = null);
                var assignedPos = Context.POS.Where(p => p.VendorId == userId).ToList();
                assignedPos.ForEach(x => x.VendorId = null);
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Vendor Deleted Successfully."
                };
            }
        }
        ActionOutput IVendorManager.SaveVendor(SaveVendorModel model)
        {
            var vendor = new User();
            if (model.VendorId > 0)
            {
                vendor = Context.Users.FirstOrDefault(p => p.UserId == model.VendorId);
                if (vendor == null)
                    return ReturnError("Vendor not exist");
            }

            if (model.VendorId == 0)
            {
                var existing_user_by_number = Context.Users.FirstOrDefault(z => z.Phone.Trim().ToLower() == model.Phone.Trim().ToLower()) ?? null;
                if (existing_user_by_number != null)
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error,
                        Message = "User OR Vendor with this phone number already exist"
                    };
                }
            }

            //if(string.IsNullOrEmpty(model.City) || string.IsNullOrEmpty(model.Country))
            //{
            //    return new ActionOutput
            //    {
            //        Status = ActionStatus.Error,
            //        Message = "Country and City required "
            //    };
            //}

            //if (Context.Users.Any(p => p.Email.ToLower() == model.Email.ToLower() && p.UserId != model.VendorId))
            //    return ReturnError("This email is already exist with another user");

            vendor.Email = model.Email;
            vendor.Name = model.Name;
            vendor.SurName = model.SurName;
            vendor.Phone = model.Phone;
            vendor.CountryCode = model.CountryCode;
            vendor.Password = Utilities.EncryptPassword(model.Password);
            vendor.AgentId = model.AgencyId;
            vendor.UserType = Utilities.GetUserRoleIntValue(UserRoles.Vendor);
            vendor.CreatedAt = DateTime.UtcNow;
            vendor.Status = model.VendorId == 0 ? (int)UserStatusEnum.Pending : vendor.Status;
            vendor.VendorCommissionPercentage = model.Percentage;
            vendor.VendorType = model.VendorType;
            vendor.Vendor = model.Vendor;
            //vendor.CityId = Convert.ToInt32(model.City);
            //vendor.CountryId = Convert.ToInt32(model.Country);
            vendor.Address = model.Address;
            //if (model.POSId.HasValue && model.POSId > 0)
            //    vendor.FKPOSId = model.POSId;
            //else
            //    vendor.FKPOSId = null;
            if (model.VendorId == 0)
                Context.Users.Add(vendor);
            Context.SaveChanges();
            vendor.FKVendorId = vendor.UserId;
            Context.SaveChanges();
            return ReturnSuccess("Vendor saved successfully.");
        }
        List<SelectListItem> IVendorManager.GetVendorsSelectList(long agentId)
        {
            try
            {
                IQueryable<User> query = Context.Users.Where(p => p.Vendor != null && p.UserRole.Role == UserRoles.Vendor && p.Status == (int)UserStatusEnum.Active && p.POS.Any());
                if (agentId > 0)
                    query = query.Where(p => p.AgentId == agentId);
                return query.ToList().Select(p =>   new SelectListItem
                {
                    Text = p.Vendor.ToUpper(),
                    Value = p.UserId.ToString()
                }).ToList();
            }
            catch (Exception)
            {
                return new List<SelectListItem>();
            }
        }

        List<SelectListItem> IVendorManager.GetPosSelectList()
        {
            //IQueryable<User> query = Context.POS.Where(p => p.VendorId!=null && !p.IsDeleted).Select(p=>p.User);
            //if (agentId > 0)
            //    query = query.Where(p => p.AgentId == agentId);
            //return query.ToList().Select(p => new SelectListItem
            //{
            //    Text = p.Vendor,
            //    Value = p.UserId.ToString()
            //}).ToList();
            return Context.POS.Where(p => !p.IsDeleted && p.Enabled != false).ToList().OrderBy(p => p.SerialNumber).Select(x => new SelectListItem
            {
                Text = x.SerialNumber.ToUpper(),
                Value = x.POSId.ToString()
            }).ToList();
        }

        decimal IVendorManager.GetVendorPercentage(long userId)
        {
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user.FKVendorId == null)
                return user.Commission == null ? 0 : user.Commission.Percentage;
            else
            {
                var vendor = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
                return vendor.Commission == null ? 0 : vendor.Commission.Percentage;
            }
        }

        long IVendorManager.GetVendorIdByAppUserId(long userId)
        {
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user.UserRole.Role == UserRoles.Vendor)
                return userId;
            else if (user.FKVendorId != null)
                return user.FKVendorId.Value;
            return 0;
        }
    }


}
