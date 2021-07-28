using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class UserManager : BaseManager, IUserManager
    {

        string IUserManager.GetWelcomeMessage()
        {
            return "Welcome To Base Project Demo";
        }

        UserModel IUserManager.ValidateUserSession(string token)
        {
            var session = Context.TokensManagers.Where(o => o.TokenKey.Equals(token)).FirstOrDefault();
            if (session != null)
            {
                var pos = Context.POS.FirstOrDefault(x => x.SerialNumber == session.PosNumber);
                if (session != null &&
                    (session.User.Status == (int)UserStatusEnum.Active
                    || session.User.Status == (int)UserStatusEnum.Pending
                    || session.User.Status == (int)UserStatusEnum.PasswordNotReset) && pos.Enabled == true) return new UserModel(session.TokenKey, session.User);
                else return null;
            }
            else
            {
                return null;
            }
        }
        bool IUserManager.UpdateUserLastAppUsedTime(long userId)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if (user != null)
            {
                user.AppLastUsed = DateTime.UtcNow;
                Context.SaveChanges();
                return true;
            }
            return false;
        }
        ActionOutput IUserManager.UpdateProfilePic(long userId, HttpPostedFile image)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            var myfile = string.Empty;
            if (user == null)
                return ReturnError("User not exist.");
            if (image != null)
            {
                var ext = Path.GetExtension(image.FileName); //getting the extension(ex-.jpg)  
                myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                // store the file inside ~/project folder(Images/ProfileImages)  
                var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                if (!Directory.Exists(folderName))
                    Directory.CreateDirectory(folderName);
                var path = Path.Combine(folderName, myfile);
                image.SaveAs(path);

                //var obj = new QuickbloxServices();
                //var result = obj.RegisterUser(model);
            }
            else
                return ReturnError("Please attach an image.");
            if (!string.IsNullOrEmpty(user.ProfilePic))
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~" + user.ProfilePic)))
                    File.Delete(HttpContext.Current.Server.MapPath("~" + user.ProfilePic));
            }
            user.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
            Context.SaveChanges();
            return ReturnSuccess("Profile picture updated successfully.");
        }
        ActionOutput<ApiResponseUserDetail> IUserManager.GetUserDetailsForApi(long userId)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if (user == null)
                return ReturnError<ApiResponseUserDetail>("User Not exist");
            return ReturnSuccess<ApiResponseUserDetail>(new ApiResponseUserDetail(user), "User detail fetched successfully.");
        }
        UpdateProfileModel IUserManager.GetAppUserProfile(long userId)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if (user == null)
                return null;
            return new UpdateProfileModel
            {
                Address = user.Address,
                City = user.CityId,
                Country = user.CountryId,
                ProfilePicUrl = string.IsNullOrEmpty(user.ProfilePic) ? "" : Utilities.DomainUrl + user.ProfilePic,
                Name = user.Name,
                SurName = user.SurName,
                Phone = user.Phone,




            };
        }
        ActionOutput IUserManager.UpdateUserProfile(long userId, UpdateProfileModel model)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            var myfile = string.Empty;

            if (user == null)
                return ReturnError("User Not exist");
            user.UpdatedAt = DateTime.UtcNow;
            user.Name = model.Name;
            user.SurName = model.SurName;
            user.CityId = model.City;
            user.DOB = model.DOB != null ? model.DOB : user.DOB;
            user.CountryId = model.Country;
            user.Phone = model.Phone;
            user.Address = model.Address;
            try
            {
                if (model.Image != null)
                {
                    var ext = Path.GetExtension(model.Image.FileName); //getting the extension(ex-.jpg)  
                    myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                                                              // store the file inside ~/project folder(Images/ProfileImages)  
                    var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    var path = Path.Combine(folderName, myfile);
                    model.Image.SaveAs(path);
                    if (!string.IsNullOrEmpty(user.ProfilePic))
                    {
                        if (File.Exists(HttpContext.Current.Server.MapPath("~" + user.ProfilePic)))
                            File.Delete(HttpContext.Current.Server.MapPath("~" + user.ProfilePic));
                    }
                    user.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
                    //var obj = new QuickbloxServices();
                    //var result = obj.RegisterUser(model);
                }
                Context.SaveChanges();
                return ReturnSuccess("User profile updated successfully, All changes will be fully update by the next login.");
            }
            catch (Exception e)
            {
                return ReturnError(e.ToString());
            }
        }

        ActionOutput IUserManager.UpdateAdminprofile(long userId, UpdateProfileModel model)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            var myfile = string.Empty;

            if (user == null)
                return ReturnError("Admin Not exist");
            user.UpdatedAt = DateTime.UtcNow;
            user.Name = model.Name;
            user.SurName = model.SurName;
            user.CityId = model.City;
            user.DOB = model.DOB != null ? model.DOB : user.DOB;
            user.CountryId = model.Country;
            user.Phone = model.Phone;
            user.Address = model.Address;
            if (model.Image != null)
            {
                var ext = Path.GetExtension(model.Image.FileName); //getting the extension(ex-.jpg)  
                myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                // store the file inside ~/project folder(Images/ProfileImages)  
                var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                if (!Directory.Exists(folderName))
                    Directory.CreateDirectory(folderName);
                var path = Path.Combine(folderName, myfile);
                model.Image.SaveAs(path);
                if (!string.IsNullOrEmpty(user.ProfilePic))
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath("~" + user.ProfilePic)))
                        File.Delete(HttpContext.Current.Server.MapPath("~" + user.ProfilePic));
                }
                user.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
            }
            Context.SaveChanges();
            return ReturnSuccess("Admin profile has been  updated successfully, All changes will be fully update by the next login.");
        }


        PagingResult<NotificationApiListingModel> IUserManager.GetUserNotifications(int pageNo, int pageSize, long userId)
        {
            var result = new PagingResult<NotificationApiListingModel>();
            var query = Context.Notifications.Where(p => p.UserId == userId);
            result.TotalCount = query.Count();
            result.List = query.OrderByDescending(p => p.SentOn).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new NotificationApiListingModel(x)).ToList();
            result.Message = "Notifications fetched successfully.";
            query.ToList().ForEach(p => p.MarkAsRead = true);
            Context.SaveChanges();
            return result;
        }

        DataResult<List<MeterRechargeApiListingModel>, List<DepositListingModel>, ActionStatus> IUserManager.GetUserNotificationApi(int pageNo, int pageSize, long userId)
        {
            var result = new DataResult<List<MeterRechargeApiListingModel>, List<DepositListingModel>, ActionStatus>();
            IQueryable<TransactionDetail> query = null;

            query = Context.TransactionDetails.Where(p => !p.IsDeleted && p.POSId != null && p.Finalised == true);

            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            var posIds = new List<long>();
            posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
            query = query.Where(p => posIds.Contains(p.POSId.Value));
            result.Result1 = query.OrderByDescending(x => x.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new MeterRechargeApiListingModel
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm"),//ToString("dd/MM/yyyy HH:mm"),
                MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number,
                TransactionId = x.TransactionId,
                MeterRechargeId = x.TransactionDetailsId,
                RechargePin = x?.MeterToken1,
                POSId = x.POSId == null ? "" : x.POS.SerialNumber
            }).ToList();
            IQueryable<DepositLog> query1 = null;

            query1 = Context.DepositLogs.OrderByDescending(p => p.Deposit.CreatedAt).Where(p => p.NewStatus == (int)DepositPaymentStatusEnum.Released);

            posIds = Context.POS.Where(p => p.VendorId != null && (p.VendorId == user.FKVendorId)).Select(p => p.POSId).ToList();
            query1 = query1.OrderByDescending(x => x.CreatedAt).Where(p => posIds.Contains(p.Deposit.POSId));
            var totalrecoed = query.ToList().Count();
            result.Result2 = query1
               .Skip((pageNo - 1) * pageSize).Take(pageSize).ToList().Select(x => new DepositListingModel(x.Deposit)).ToList();
            result.Result3 = ActionStatus.Successfull;
            return result;
        }

        PagingResult<UserListingModel> IUserManager.GetUserPagedList(PagingModel model, bool onlyAppUser, string status)
        {
            var result = new PagingResult<UserListingModel>();
            var query = Context.Users.Where(p => p.Status != (int)UserStatusEnum.Deleted).OrderBy(model.SortBy + " " + model.SortOrder);
            //Client want to show app user and vendor on the same screen because they both can login from app
            if (onlyAppUser)
                query = query.Where(p => p.UserRole.Role == UserRoles.AppUser || p.UserRole.Role == UserRoles.Vendor);
            else
                query = query.Where(p => p.UserRole.Role != UserRoles.AppUser && p.UserRole.Role != UserRoles.Vendor);

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.Name.ToLower().Contains(model.Search.ToLower()) || z.SurName.ToLower().Contains(model.Search.ToLower()) || ((UserStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()) || z.UserRole.Role.ToLower().Contains(model.Search.ToLower()));

                if (model.SearchField.Equals("FIRST"))
                    query = query.Where(z => z.Name.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("LAST"))
                    query = query.Where(z => z.SurName.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("STATUS"))
                    query = query.Where(z => ((UserStatusEnum)z.Status).ToString().ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("VENDOR"))
                    query = query.Where(z => z.User1 != null && z.User1.Vendor.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("ROLE"))
                    query = query.Where(z => z.UserRole.Role.ToLower().Contains(model.Search.ToLower()));
            }
            else if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(z => ((UserStatusEnum)z.Status).ToString().ToLower().Contains(status.ToLower()));
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new UserListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "User List";
            result.TotalCount = query.Count();
            return result;
        }
        ActionOutput<string> IUserManager.SaveReferralCode(long userId)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if (user == null)
                return ReturnError<string>("User not exist");
            var dbReferralCode = new ReferralCode();
            dbReferralCode.Code = Utilities.RandomString(6);
            dbReferralCode.IsUsed = false;
            dbReferralCode.FK_UserId = userId;
            dbReferralCode.CreatedAt = DateTime.UtcNow;
            Context.ReferralCodes.Add(dbReferralCode);
            Context.SaveChanges();
            return ReturnSuccess<string>(dbReferralCode.Code, "Referral code generated successfully");
        }
        int IUserManager.GetUnreadNotifications(long userId)
        {
            return Context.Notifications.Where(p => p.UserId == userId && !p.MarkAsRead).Count();
        }

        ActionOutput<UserDetails> IUserManager.AdminLogin(LoginModal model)
        {
            string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
            string encryptPasswordde = Utilities.DecryptPassword("dnRlY2hAdnRlY2gqMjAyMQ==");
            var user = Context.Users.FirstOrDefault(p =>
            (UserRoles.AppUser != p.UserRole.Role) && (UserRoles.Vendor != p.UserRole.Role) && (UserRoles.Agent != p.UserRole.Role) &&
            (p.Status == (int)UserStatusEnum.Active || p.Status == (int)UserStatusEnum.PasswordNotReset) &&
             p.Password == encryptPassword &&
             p.Email.ToLower() == model.UserName.ToLower());
            if (user == null)
                return null;
            var modelUser = new UserDetails
            {
                FirstName = user.Name,
                LastName = user.SurName,
                UserEmail = user.Email,
                UserID = user.UserId,
                UserType = user.UserRole.Role,
                ProfilePicPath = user.ProfilePic
            };
            return ReturnSuccess<UserDetails>(modelUser, "User logged in successfully.");
        }


        IList<UserAssignedModuleModel> IUserManager.GetNavigations(long userId)
        {
            var userModule = (from ua in Context.UserAssignedModules
                              join m in Context.Modules on ua.ModuleId equals m.ModuleId
                              where ua.UserId == userId
                              select new
                              {
                                  AssignUserModuleId = ua.AssignUserModuleId,
                                  ModuleName = m.ModuleName
                              }).ToList();
            return userModule.Select(x => new UserAssignedModuleModel
            {
                Modules = x.ModuleName,
                AssignUserModuleId = x.AssignUserModuleId
            }).ToList();
        }
        long IUserManager.GetUserId(string phone)
        {
            var userDetail = Context.Users.FirstOrDefault(x => x.Phone == phone);
            if (userDetail != null)
            {
                return userDetail.UserId;
            }
            return Convert.ToInt64(0);
        }

        ActionOutput<UserDetails> IUserManager.AgentLogin(LoginModal model)
        {
            string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
            var user = Context.Agencies.SingleOrDefault(p => p.Password == encryptPassword && p.REPEmail.ToLower() == model.UserName.ToLower());
            if (user == null)
                return null;
            var modelUser = new UserDetails
            {
                FirstName = user.REPName,
                LastName = user.REPLastName,
                UserEmail = user.REPEmail,
                UserID = user.AgencyId
            };
            return ReturnSuccess<UserDetails>(modelUser, "User logged in successfully.");
        }

        ActionOutput<UserDetails> IUserManager.VendorLogin(LoginModal model)
        {
            string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
            string _encryptPassword = Utilities.DecryptPassword("dGVzdG15cGF5");
            var user = Context.Users.SingleOrDefault(p => p.Password == encryptPassword && p.Email.ToLower() == model.UserName.ToLower() && p.Status == (int)UserStatusEnum.Active && p.UserRole.Role == UserRoles.Vendor);
            if (user == null)
                return null;
            var modelUser = new UserDetails
            {
                FirstName = user.Name,
                LastName = user.Name,
                UserEmail = user.Email,
                UserID = user.UserId
            };
            return ReturnSuccess<UserDetails>(modelUser, "User logged in successfully.");
        }
        public IList<ModulesModel> GetAllModulesAtAuthentication(long userId)
        {
            var moduleListModel = new List<ModulesModel>();
            var modulesPermissons = Context.UserAssignedModules.Where(x => x.UserId == userId).ToList().Select(x => x.ModuleId);
            var modules = Context.Modules.Where(c => modulesPermissons.Contains(c.ModuleId)).ToList();
            if (modules.Count() > 0)
            {
                moduleListModel = modules.Select(x => new ModulesModel(x)).ToList();
            }
            return moduleListModel;
        }
        public List<SelectListItem> GetAssignedReportModules(long UserId, bool isAdmin)
        {
            if (isAdmin)
            {
                return Context.Modules.Where(p => p.SubMenuOf == 9).ToList().OrderBy(l => l.ModuleId).Select(p => new SelectListItem
                {
                    Text = p.ModuleName,
                    Value = p.ModuleId.ToString()
                }).ToList();
            }
            return Context.UserAssignedModules.Where(p => p.UserId == UserId && p.Module.SubMenuOf == 9).ToList().OrderBy(l => l.ModuleId).Select(p => new SelectListItem
            {
                Text = p.Module.ModuleName,
                Value = p.Module.ModuleId.ToString()
            }).ToList();

        }
        ActionOutput IUserManager.UpdateUserDetails(AddUserModel userDetails)
        {
            string myfile = string.Empty;
            var user = Context.Users.Where(z => z.UserId == userDetails.UserId).FirstOrDefault();
            if (user == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User Not Exist."
                };
            }
            if (IsEmailUsed(userDetails.Email, userDetails.UserId))
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "This email-id already exists for another user."
                };
            }
            else
            {
                user.Email = userDetails.Email.Trim().ToLower();
                user.Name = userDetails.FirstName;
                if (user.UserType != Utilities.GetUserRoleIntValue(UserRoles.AppUser))
                    user.UserType = (int)userDetails.UserType;
                user.SurName = userDetails.LastName;
                user.Phone = userDetails.Phone;
                user.Status = userDetails.ResetUserPassword ? (int)UserStatusEnum.PasswordNotReset : user.Status;
                user.CountryCode = userDetails.CountryCode;
                user.Password = Utilities.EncryptPassword(userDetails.Password);
                if (userDetails.Image != null)
                {
                    var ext = Path.GetExtension(userDetails.Image.FileName); //getting the extension(ex-.jpg)  
                    myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                    // store the file inside ~/project folder(Images/ProfileImages)  
                    var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    var path = Path.Combine(folderName, myfile);
                    userDetails.Image.SaveAs(path);
                    if (!string.IsNullOrEmpty(user.ProfilePic))
                    {
                        if (File.Exists(HttpContext.Current.Server.MapPath("~" + user.ProfilePic)))
                            File.Delete(HttpContext.Current.Server.MapPath("~" + user.ProfilePic));
                    }
                    user.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
                    //var obj = new QuickbloxServices();
                    //var result = obj.RegisterUser(model);
                }
                Context.SaveChanges();

                RemoveORAddUserPermissions(user.UserId, userDetails);

                RemoveOrAddUserPlatforms(user.UserId, userDetails);

                RemoveOrAddUserWidgets(user.UserId, userDetails);

                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "User Details Updated Successfully."
                };
            }
        }
        AddUserModel IUserManager.GetAppUserDetailsByUserId(long userId)
        {
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user == null)
                return null;
            return new AddUserModel
            {
                Password = Utilities.DecryptPassword(user.Password),
                ConfirmPassword = Utilities.DecryptPassword(user.Password),
                UserId = user.UserId,
                FirstName = user.Name,
                LastName = user.SurName,
                Email = user.Email,
                UserType = user.UserType,
                Phone = user.Phone,
                CompanyName = user.CompanyName,
                VendorId = user.FKVendorId,
                Address = user.Address,
                ProfilePicUrl = string.IsNullOrEmpty(user.ProfilePic) ? "" : Utilities.DomainUrl + user.ProfilePic,
                //POSId=user.FKPOSId,
                AccountStatus = ((UserStatusEnum)(user.Status)).ToString()
            };
        }
        ActionOutput IUserManager.UpdateAppUserDetails(AddUserModel userDetails)
        {
            var user = Context.Users.Where(z => z.UserId == userDetails.UserId).FirstOrDefault();
            var myfile = string.Empty;

            if (user == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User Not Exist."
                };
            }

            if (IsEmailUsed(userDetails.Email, userDetails.UserId))
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "This email-id already exists for another user."
                };
            }
            else
            {
                user.Email = userDetails.Email.Trim().ToLower();
                user.Name = userDetails.FirstName;
                user.SurName = userDetails.LastName;
                user.Phone = userDetails.Phone;
                user.CountryCode = userDetails.CountryCode;
                user.Address = userDetails.Address;
                user.Password = Utilities.EncryptPassword(userDetails.Password);
                if (userDetails.VendorId.HasValue && userDetails.VendorId > 0)
                    user.FKVendorId = userDetails.VendorId;

                if (userDetails.ResetUserPassword)
                    user.Status = (int)UserStatusEnum.PasswordNotReset;
                else
                    user.Status = user.Status;

                if (userDetails.IsRe_Approval)
                    user.Status = (int)UserStatusEnum.Active;

                if (userDetails.Image != null)
                {
                    var ext = Path.GetExtension(userDetails.Image.FileName); //getting the extension(ex-.jpg)  
                    myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                    // store the file inside ~/project folder(Images/ProfileImages)  
                    var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    var path = Path.Combine(folderName, myfile);
                    userDetails.Image.SaveAs(path);
                    if (!string.IsNullOrEmpty(user.ProfilePic))
                    {
                        if (File.Exists(HttpContext.Current.Server.MapPath("~" + user.ProfilePic)))
                            File.Delete(HttpContext.Current.Server.MapPath("~" + user.ProfilePic));
                    }
                    user.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;

                }
                Context.SaveChanges();

                RemoveORAddUserPermissions(user.UserId, userDetails);

                RemoveOrAddUserPlatforms(user.UserId, userDetails);

                RemoveOrAddUserWidgets(user.UserId, userDetails);

                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "User Details Updated Successfully."
                };
            }
        }
        IList<Checkbox> IUserManager.GetAllModules(long userId)
        {
            IList<Checkbox> chekboxListOfModules = null;
            IList<Module> modules = Context.Modules.ToList();
            if (modules.Count() > 0)
            {
                chekboxListOfModules = modules.Select(x =>
                {
                    return new Checkbox()
                    {
                        ID = x.ModuleId,
                        ModuleName = x.ModuleName,
                        Description = x.Description,
                        Checked = false,
                        SubMenuOf = x.SubMenuOf,
                        IsAdmin = x.IsAdmin
                    };
                }).ToList();

                if (userId > 0)
                {
                    var existingPermissons = Context.UserAssignedModules.Where(x => x.UserId == userId).ToList();
                    if (existingPermissons.Count() > 0)
                    {
                        chekboxListOfModules.ToList().ForEach(x => x.Checked = existingPermissons.Where(z => z.ModuleId == x.ID).Any());
                    }
                }
            }
            return chekboxListOfModules;
        }
        List<SelectListItem> IUserManager.GetUserRolesSelectList()
        {
            return Context.UserRoles.Where(p => !p.IsDeleted && p.Role != UserRoles.AppUser && p.Role != UserRoles.Vendor).ToList().Select(p => new SelectListItem
            {
                Text = p.Role.ToUpper(),
                Value = p.RoleId.ToString().ToUpper()
            }).ToList();
        }
        List<SelectListItem> IUserManager.GetAppUsersSelectList()
        {
            return Context.Users.Where(p => p.Status == (int)UserStatusEnum.Active && p.UserRole.Role == UserRoles.AppUser).ToList().Select(p => new SelectListItem
            {
                Text = p.Name.ToUpper() + " " + p.SurName.ToUpper(),
                Value = p.UserId.ToString().ToUpper()
            }).ToList();
        }

        IList<PlatformCheckbox> IUserManager.GetAllPlatforms(long userId)
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

                if (userId > 0)
                {
                    var existingPermissons = Context.UserAssignedPlatforms.Where(x => x.UserId == userId).ToList();
                    if (existingPermissons.Count() > 0)
                    {
                        chekboxListOfModules.ToList().ForEach(x => x.Checked = existingPermissons.Where(z => z.PlatformId == x.Id).Any());
                    }
                }
            }
            return chekboxListOfModules;
        }


        IList<WidgetCheckbox> IUserManager.GetAllWidgets(long userId)
        {
            IList<WidgetCheckbox> chekboxListOfModules = null;
            IList<Widget> modules = Context.Widgets.Where(p => !p.IsDeleted && p.Enabled).ToList();
            if (modules.Count() > 0)
            {
                chekboxListOfModules = modules.Select(x =>
                {
                    return new WidgetCheckbox()
                    {
                        Id = x.WidgetId,
                        Title = x.Title,
                        Checked = false
                    };
                }).ToList();

                if (userId > 0)
                {
                    var existingPermissons = Context.UserAssignedWidgets.Where(x => x.UserId == userId).ToList();
                    if (existingPermissons.Count() > 0)
                    {
                        chekboxListOfModules.ToList().ForEach(x => x.Checked = existingPermissons.Where(z => z.WidgetId == x.Id).Any());
                    }
                }
            }
            return chekboxListOfModules;
        }

        ActionOutput IUserManager.AddUserDetails(AddUserModel userDetails)
        {

            var existing_user_by_number = Context.Users.FirstOrDefault(z => z.Phone.Trim().ToLower() == userDetails.Phone.Trim().ToLower()) ?? null;

            if (existing_user_by_number != null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User with this phone number already exist"
                };
            }

            string myfile = string.Empty;
            //var existngUser = Context.Users.Where(z => z.Email.Trim().ToLower() == userDetails.Email.Trim().ToLower()).FirstOrDefault();
            if (false)
            {
                //return new ActionOutput
                //{
                //    Status = ActionStatus.Error,
                //    Message = "This email-id already exists for another user."
                //};
            }
            else
            {
                var dbUser = new User();
                dbUser.Name = userDetails.FirstName;
                dbUser.SurName = userDetails.LastName;
                dbUser.Email = userDetails.Email.Trim().ToLower();
                dbUser.Password = Utilities.EncryptPassword(userDetails.Password);
                dbUser.CreatedAt = DateTime.Now;
                dbUser.UserType = userDetails.UserType;
                dbUser.Status = userDetails.ResetUserPassword ? (int)UserStatusEnum.PasswordNotReset : (int)UserStatusEnum.Active;
                dbUser.Phone = userDetails.Phone;
                dbUser.CountryCode = userDetails.CountryCode;
                if (userDetails.Image != null)
                {
                    var ext = Path.GetExtension(userDetails.Image.FileName); //getting the extension(ex-.jpg)  
                    myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                    // store the file inside ~/project folder(Images/ProfileImages)  
                    var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    var path = Path.Combine(folderName, myfile);
                    userDetails.Image.SaveAs(path);

                    dbUser.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
                    //var obj = new QuickbloxServices();
                    //var result = obj.RegisterUser(model);
                }

                try
                {
                    Context.Users.Add(dbUser);
                    Context.SaveChanges();

                    RemoveORAddUserPermissions(dbUser.UserId, userDetails);
                    RemoveOrAddUserPlatforms(dbUser.UserId, userDetails);
                    RemoveOrAddUserWidgets(dbUser.UserId, userDetails);
                }
                catch (Exception e)
                {
                    throw e;
                }
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "User Added Successfully."
                };
            }
        }

        ActionOutput IUserManager.AddAppUserDetails(AddUserModel userDetails)
        {

            var existing_user_by_number = Context.Users.FirstOrDefault(z => z.Phone.Trim().ToLower() == userDetails.Phone.Trim().ToLower()) ?? null;

            if (existing_user_by_number != null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User with this phone number already exist"
                };
            }
            string myfile = "";
            var existngUser = Context.Users.Where(z => z.Email.Trim().ToLower() == userDetails.Email.Trim().ToLower()).FirstOrDefault();
            if (false)
            {
                //return new ActionOutput
                //{
                //    Status = ActionStatus.Error,
                //    Message = "This email-id already exists for another user."
                //};
            }

            else
            {
                var dbUser = new User();
                dbUser.Name = userDetails.FirstName;
                dbUser.SurName = userDetails.LastName;
                dbUser.Email = userDetails.Email.Trim().ToLower();
                dbUser.Password = Utilities.EncryptPassword(userDetails.Password);
                dbUser.CreatedAt = DateTime.Now;
                dbUser.UserType = Utilities.GetUserRoleIntValue(UserRoles.AppUser);
                dbUser.IsEmailVerified = false;
                dbUser.Address = userDetails.Address;
                //dbUser.Status = userDetails.ResetUserPassword ? (int)UserStatusEnum.PasswordNotReset : (int)UserStatusEnum.Active;
                dbUser.Status = (int)UserStatusEnum.Pending;

                dbUser.Phone = userDetails.Phone;
                dbUser.CountryCode = userDetails.CountryCode;
                if (userDetails.VendorId.HasValue && userDetails.VendorId > 0)
                    dbUser.FKVendorId = userDetails.VendorId;
                //if (userDetails.POSId.HasValue && userDetails.POSId > 0)
                //    dbUser.FKPOSId = userDetails.POSId;
                if (userDetails.Image != null)
                {
                    var ext = Path.GetExtension(userDetails.Image.FileName); //getting the extension(ex-.jpg)  
                    myfile = Guid.NewGuid().ToString() + ext; //appending the name with id  
                    // store the file inside ~/project folder(Images/ProfileImages)  
                    var folderName = HttpContext.Current.Server.MapPath("~/Images/ProfileImages");
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    var path = Path.Combine(folderName, myfile);
                    userDetails.Image.SaveAs(path);

                    dbUser.ProfilePic = string.IsNullOrEmpty(myfile) ? "" : "/Images/ProfileImages/" + myfile;
                }
                try
                {
                    Context.Users.Add(dbUser);
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new ActionOutput
                    {
                        ID = dbUser.UserId,
                        Status = ActionStatus.Error,
                        Message = ex?.Message ?? ex?.InnerException?.Message
                    };
                }


                RemoveORAddUserPermissions(dbUser.UserId, userDetails);

                RemoveOrAddUserPlatforms(dbUser.UserId, userDetails);

                RemoveOrAddUserWidgets(dbUser.UserId, userDetails);

                return new ActionOutput
                {
                    ID = dbUser.UserId,
                    Status = ActionStatus.Successfull,
                    Message = "User Added Successfully, Verification link has been sent on user email account"
                };
            }
        }

        ActionOutput IUserManager.AddAppUserDetails(RegisterAPIModel userDetails)
        {

            var existing_user_by_number = Context.Users.FirstOrDefault(z => z.Phone.Trim().ToLower() == userDetails.Mobile.Trim().ToLower()) ?? null;

            if (existing_user_by_number != null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "User with this phone number already exist"
                };
            }
            //var existing_user_by_email = Context.Users.FirstOrDefault(z => z.Email.Trim().ToLower() == userDetails.Email.Trim().ToLower()) ?? null;

            //if (existing_user_by_email != null)
            //{
            //    return new ActionOutput
            //    {
            //        Status = ActionStatus.Error,
            //        Message = "This email-id already exists for another user."
            //    };
            //} 
            else
            {
                //var last_pos = Context.POS.ToList().Max(s => Convert.ToUInt64(s.SerialNumber ?? string.Empty)).ToString() ?? string.Empty;
                //var last_pos1 = Convert.ToUInt64(last_pos) + 1;
                var dbUser = new User();
                dbUser.Name = userDetails.FirstName;
                dbUser.CompanyName = userDetails.CompanyName;
                dbUser.IsCompany = userDetails.IsCompany;
                dbUser.SurName = userDetails.LastName;
                dbUser.Email = userDetails.Email.Trim().ToLower();
                dbUser.Password = Utilities.EncryptPassword(Utilities.GenerateByAnyLength(4));
                dbUser.CreatedAt = DateTime.UtcNow;
                dbUser.UserType = Utilities.GetUserRoleIntValue(UserRoles.Vendor); // userDetails.IsCompany ? Utilities.GetUserRoleIntValue(UserRoles.Vendor) : Utilities.GetUserRoleIntValue(UserRoles.AppUser);
                dbUser.IsEmailVerified = false;
                dbUser.Address = userDetails.Address;
                dbUser.CountryCode = "+232";
                dbUser.CityId = Convert.ToInt32(userDetails.City != null ? userDetails.City : "0");
                dbUser.Status = (int)UserStatusEnum.Pending;
                dbUser.CountryId = Convert.ToInt16(userDetails.Country);
                dbUser.Phone = userDetails.Mobile;
                dbUser.AgentId = Convert.ToInt64(userDetails.Agency != null ? userDetails.Agency : "0");
                dbUser.Vendor = userDetails.IsCompany ? userDetails.CompanyName : $"{userDetails.FirstName} {userDetails.LastName}"; // - {last_pos1}"; //userDetails.IsCompany ? userDetails.CompanyName: string.Empty;

                Context.Users.Add(dbUser);
                Context.SaveChanges();
                dbUser.FKVendorId = dbUser.UserId;
                Context.SaveChanges();
                //return new ActionOutput
                //{
                //    ID = dbUser.UserId,
                //    Status = ActionStatus.Successfull,
                //    Message = "User Added Successfully, Verification link has been sent on user email account"
                //};

                return ReturnSuccess(dbUser.UserId, $"Registration Successful !! Confirnmation email sent to  {userDetails.Email}");
            }
        }

        UserModel IUserManager.GetUserDetailsByUserId(long userId)
        {
            try
            {
                if (userId == 0) return new UserModel();
                var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
                user.UserRole = Context.UserRoles.FirstOrDefault(x => x.RoleId == user.UserType);
                if (user == null)
                    return null;
                else
                {
                    var current_user_data = new UserModel(user);
                    current_user_data.SelectedWidgets = user.UserAssignedWidgets.Select(e => e.WidgetId).ToList();
                    return current_user_data;
                }
            }
            catch (Exception ex)
            {
                return new UserModel();
            }

        }

        string IUserManager.GetUserPasswordbyUserId(long userId)
        {
            if (userId == 0) return string.Empty;
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user == null)
                return string.Empty;
            else
            {
                return Utilities.DecryptPassword(user.Password);
            }
        }

        ActionOutput IUserManager.ChangeUserStatus(long userId, UserStatusEnum status)
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
                user.Status = (int)status;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = status == UserStatusEnum.Block ? "User Blocked Successfully." : "User Activate Successfully."
                };
            }
        }

        ActionOutput IUserManager.DeclineUser(long userId)
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
                user.Status = (int)UserStatusEnum.Declined;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "User Declined Successfully."
                };
            }
        }

        ActionOutput IUserManager.DeleteUser(long userId)
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
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "User Deleted Successfully."
                };
            }
        }

        decimal IUserManager.GetUserWalletBalance(long userId)
        {
            try
            {
                var user = Context.Users.FirstOrDefault(z => z.UserId == userId);
                if (user == null)
                    return 0;
                if (user.UserRole.Role == UserRoles.AppUser || user.UserRole.Role == UserRoles.Vendor) //user.UserRole.Role == UserRoles.Vendor ||
                {
                    var posTotalBalance = Context.POS.Where(p => (p.VendorId != null && p.VendorId == user.FKVendorId) && p.Balance != null && !p.IsDeleted && p.Enabled != false).ToList().Sum(p => p.Balance);
                    return posTotalBalance.Value;
                }
                else if (user.UserRole.Role != UserRoles.AppUser)
                {
                    var posTotalBalance = Context.POS.ToList().Sum(p => p.Balance);
                    return posTotalBalance.Value;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return new decimal();
            }
        }

        bool RemoveORAddUserPermissions(long userId, AddUserModel model)
        {
            var existingpermissons = Context.UserAssignedModules.Where(x => x.UserId == userId).ToList();
            if (existingpermissons.Count() > 0)
            {
                Context.UserAssignedModules.RemoveRange(existingpermissons);
                Context.SaveChanges();
            }
            List<UserAssignedModule> newpermissos = new List<UserAssignedModule>();
            if (model.SelectedModules != null)
            {
                model.SelectedModules.ToList().ForEach(c =>
                 newpermissos.Add(new UserAssignedModule()
                 {
                     UserId = userId,
                     ModuleId = c,
                     CreatedAt = DateTime.UtcNow,
                 }));
                Context.UserAssignedModules.AddRange(newpermissos);
                Context.SaveChanges();
            }
            return true;
        }
        bool RemoveOrAddUserPlatforms(long userId, AddUserModel model)
        {
            var existingPlatforms = Context.UserAssignedPlatforms.Where(x => x.UserId == userId).ToList();
            if (existingPlatforms.Count() > 0)
            {
                Context.UserAssignedPlatforms.RemoveRange(existingPlatforms);
                Context.SaveChanges();
            }

            List<UserAssignedPlatform> newPlatforms = new List<UserAssignedPlatform>();

            if (model.SelectedPlatforms != null)
            {
                model.SelectedPlatforms.ToList().ForEach(c =>
                 newPlatforms.Add(new UserAssignedPlatform()
                 {
                     UserId = userId,
                     PlatformId = c,
                     CreatedAt = DateTime.UtcNow,
                 }));
                Context.UserAssignedPlatforms.AddRange(newPlatforms);
                Context.SaveChanges();
            }
            return true;
        }

        bool RemoveOrAddUserWidgets(long userId, AddUserModel model)
        {
            //Deleting Exisiting Widgets
            var existing_widgets = Context.UserAssignedWidgets.Where(x => x.UserId == userId).ToList();
            if (existing_widgets.Count() > 0)
            {
                Context.UserAssignedWidgets.RemoveRange(existing_widgets);
                Context.SaveChanges();
            }

            List<UserAssignedWidget> newwidgets = new List<UserAssignedWidget>();
            if (model.SelectedWidgets != null)
            {
                model.SelectedWidgets.ToList().ForEach(c =>
                 newwidgets.Add(new UserAssignedWidget()
                 {
                     UserId = userId,
                     WidgetId = c,
                     CreatedAt = DateTime.UtcNow,
                 }));
                Context.UserAssignedWidgets.AddRange(newwidgets);
                Context.SaveChanges();
            }
            return true;
        }


        bool IsEmailUsed(string email, long userId)
        {
            var existngUser = Context.Users.Where(z => z.Email.Trim().ToLower() == email.Trim().ToLower() && z.UserId != userId).FirstOrDefault();
            if (existngUser == null)
                return false;
            return false;
        }

        List<User> IUserManager.GetAllAdminUsersByAppUserPermission()
        {
            return Context.Users.Where(p =>
            (UserRoles.AppUser != p.UserRole.Role) && (UserRoles.Vendor != p.UserRole.Role) && (UserRoles.Agent != p.UserRole.Role) &&
            (p.Status == (int)UserStatusEnum.Active) && p.UserAssignedModules.Select(f => f.ModuleId).Contains(11)).ToList(); // 11 is the Module key for appUsers
        }
        List<User> IUserManager.GetAllAdminUsersByDepositRelease()
        {
            return Context.Users.Where(p =>
            (UserRoles.AppUser != p.UserRole.Role) && (UserRoles.Vendor != p.UserRole.Role) && (UserRoles.Agent != p.UserRole.Role) &&
            (p.Status == (int)UserStatusEnum.Active) && p.UserAssignedModules.Select(f => f.ModuleId).Contains(7)).ToList(); // 7 is the Module key for Deposit Release
        }

        UserDetails IUserManager.GetNotificationUsersCount(long currentUserId)
        {
            try
            {
                var notificationDetail = Context.UserAssignedModules.Where(x => x.UserId == currentUserId && (x.ModuleId == 6 || x.ModuleId == 10));
                var modelUser = new UserDetails();
                modelUser.AppUserMessage = notificationDetail.FirstOrDefault(x => x.ModuleId == 10) != null ? "NEW APP USERS APPROVAL" : string.Empty;
                modelUser.DepositReleaseMessage = notificationDetail.FirstOrDefault(x => x.ModuleId == 6) != null ? "NEW DEPOSITS RELEASE" : string.Empty;
                modelUser.RemainingAppUser = !string.IsNullOrEmpty(modelUser.AppUserMessage) ? Context.Users.Where(x => (x.UserRole.Role == UserRoles.AppUser || x.UserRole.Role == UserRoles.Vendor) && x.Status == (int)UserStatusEnum.Pending).Count() : 0;
                modelUser.RemainingDepositRelease = !string.IsNullOrEmpty(modelUser.DepositReleaseMessage) ? Context.Deposits.Where(x => x.Status == (int)DepositPaymentStatusEnum.Pending).Count() : 0;

                return modelUser;
            }
            catch (Exception)
            {
                return new UserDetails();
            }
        }

        IEnumerable<UserLiteDto> IUserManager.GetVendorNames_API()
        {
            var result = Context.POS.Where(x => x.User != null).ToList().Select(x => new UserLiteDto
            {
                VendorId = Convert.ToInt64(x.VendorId),
                VendorName = x.User?.Vendor
            });
            return result;
        }
        UserLiteDto IUserManager.GetVendorNamePOSNumber(int posId)
        {
            var result = new UserLiteDto();
            try
            {
                result = Context.POS.Where(p => p.POSId == posId).Select(x => new UserLiteDto
                {
                    POSId = x.POSId,
                    VendorName = x.User.Vendor
                }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
    }


}
