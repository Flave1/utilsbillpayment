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
using System.Data.Entity.Validation;

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
            if (session != null && (session.User.Status == (int)UserStatusEnum.Active || session.User.Status == (int)UserStatusEnum.Pending || session.User.Status == (int)UserStatusEnum.PasswordNotReset)) return new UserModel(session.TokenKey, session.User);
            else return null;
        }
        bool IUserManager.UpdateUserLastAppUsedTime(long userId)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if(user!=null)
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
                Address=user.Address,
                City=user.CityId,
                Country=user.CountryId,
                ProfilePicUrl= string.IsNullOrEmpty(user.ProfilePic) ? "" : Utilities.DomainUrl + user.ProfilePic,
                Name=user.Name,
                SurName=user.SurName,
                Phone=user.Phone,
                
                
                

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
            user.DOB = model.DOB!=null?model.DOB:user.DOB;
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
                //var obj = new QuickbloxServices();
                //var result = obj.RegisterUser(model);
            }
            Context.SaveChanges();
            return ReturnSuccess("User profile updated successfully, All changes will be fully update by the next login.");
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
        PagingResult<UserListingModel> IUserManager.GetUserPagedList(PagingModel model,bool onlyAppUser)
        {
            var result = new PagingResult<UserListingModel>();
            var query = Context.Users.Where(p => p.Status != (int)UserStatusEnum.Deleted).OrderBy(model.SortBy + " " + model.SortOrder);
            //Client want to show app user and vendor on the same screen because they both can login from app
            if (onlyAppUser)
                query = query.Where(p => p.UserRole.Role == UserRoles.AppUser || p.UserRole.Role==UserRoles.Vendor);
            else
                query = query.Where(p => p.UserRole.Role != UserRoles.AppUser  && p.UserRole.Role != UserRoles.Vendor);

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
                    query = query.Where(z =>z.User1!=null && z.User1.Vendor.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("ROLE"))
                    query = query.Where(z => z.UserRole.Role.ToLower().Contains(model.Search.ToLower()));
            }
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
           // string encryptPasswordde = Utilities.DecryptPassword("dGVzdDEyMzQ1Ng==");
            var user = Context.Users.FirstOrDefault(p => (UserRoles.AppUser != p.UserRole.Role) && (UserRoles.Vendor != p.UserRole.Role) && (UserRoles.Agent != p.UserRole.Role) && ( p.Status == (int)UserStatusEnum.Active ||p.Status==(int)UserStatusEnum.PasswordNotReset)&& p.Password == encryptPassword && p.Email.ToLower() == model.UserName.ToLower());
            if (user == null)
                return null;
            var modelUser = new UserDetails
            {
                FirstName = user.Name,
                LastName = user.SurName,
                UserEmail = user.Email,
                UserID = user.UserId,
                UserType=user.UserRole.Role,
                ProfilePicPath=user.ProfilePic
            };
            return ReturnSuccess<UserDetails>(modelUser, "User logged in successfully.");
        }

        ActionOutput<UserDetails> IUserManager.AgentLogin(LoginModal model)
        {
            string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
            var user = Context.Agencies.SingleOrDefault(p =>p.Password == encryptPassword && p.REPEmail.ToLower() == model.UserName.ToLower());
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
            var user = Context.Users.SingleOrDefault(p => p.Password == encryptPassword && p.Email.ToLower() == model.UserName.ToLower() && p.Status==(int)UserStatusEnum.Active && p.UserRole.Role==UserRoles.Vendor);
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
                return Context.Modules.Where(p => p.SubMenuOf == 7).ToList().OrderBy(l => l.ModuleId).Select(p => new SelectListItem
                {
                    Text = p.ModuleName,
                    Value = p.ModuleId.ToString()
                }).ToList();
            }
            return Context.UserAssignedModules.Where(p => p.UserId == UserId && p.Module.SubMenuOf == 7).ToList().OrderBy(l=>l.ModuleId).Select(p => new SelectListItem
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
            var existngUser = Context.Users.Where(z => z.Email.Trim().ToLower() == userDetails.Email.Trim().ToLower() && z.UserId != userDetails.UserId).FirstOrDefault();
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
                VendorId=user.FKVendorId,
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
            var existngUser = Context.Users.Where(z => z.Email.Trim().ToLower() == userDetails.Email.Trim().ToLower() && z.UserId != userDetails.UserId).FirstOrDefault();
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
                user.Email = userDetails.Email.Trim().ToLower();
                user.Name = userDetails.FirstName;
                user.SurName = userDetails.LastName;
                user.Phone = userDetails.Phone;
                user.CountryCode = userDetails.CountryCode;
                user.Password = Utilities.EncryptPassword(userDetails.Password);
                if (userDetails.VendorId.HasValue && userDetails.VendorId > 0)
                    user.FKVendorId = userDetails.VendorId;
                else
                    user.FKVendorId = null;
                //if (userDetails.POSId.HasValue && userDetails.POSId > 0)
                //    user.FKPOSId = userDetails.POSId;
                //else
                //    user.FKPOSId = null;
                user.Status = userDetails.ResetUserPassword ? (int)UserStatusEnum.PasswordNotReset : user.Status;
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
                        SubMenuOf=x.SubMenuOf
                    };
                }).ToList();

                if (userId > 0)
                {
                    var existingPermissons = Context.UserAssignedModules.Where(x => x.UserId == userId).ToList();
                    if (existingPermissons.Count > 0)
                    {
                        chekboxListOfModules.ToList().ForEach(x => x.Checked = existingPermissons.Where(z => z.ModuleId == x.ID).Any());
                    }
                }
            }
            return chekboxListOfModules;
        }
         List<SelectListItem> IUserManager.GetUserRolesSelectList()
         {
             return Context.UserRoles.Where(p => !p.IsDeleted && p.Role  != UserRoles.AppUser &&  p.Role  != UserRoles.Vendor).ToList().Select(p => new SelectListItem
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
             IList<Platform> modules = Context.Platforms.Where(p=>!p.IsDeleted && p.Enabled).ToList();
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
                     if (existingPermissons.Count > 0)
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
                    if (existingPermissons.Count > 0)
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
                Context.Users.Add(dbUser);
                Context.SaveChanges();


                RemoveORAddUserPermissions(dbUser.UserId, userDetails);

                RemoveOrAddUserPlatforms(dbUser.UserId, userDetails);

                RemoveOrAddUserWidgets(dbUser.UserId, userDetails);

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
            string myfile="";
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
                //dbUser.Status = userDetails.ResetUserPassword ? (int)UserStatusEnum.PasswordNotReset : (int)UserStatusEnum.Active;
                dbUser.Status =  (int)UserStatusEnum.Pending;

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
                    ID= dbUser.UserId,
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
                var dbUser = new User();
                dbUser.Name = string.IsNullOrEmpty(userDetails.CompanyName)? userDetails.FirstName: userDetails.CompanyName;
                dbUser.CompanyName = userDetails.CompanyName;
                dbUser.SurName = userDetails.LastName;
                dbUser.Email = userDetails.Email.Trim().ToLower();
                dbUser.Password = Utilities.EncryptPassword("vendtech8");
                dbUser.CreatedAt = DateTime.UtcNow;
                dbUser.UserType = Utilities.GetUserRoleIntValue(UserRoles.AppUser);
                dbUser.IsEmailVerified = false;
                dbUser.Address = $"{userDetails.Address}....{userDetails.Country}";
                dbUser.CountryCode = userDetails.Country.Substring(0,9);
                //dbUser.CityId = userDetails.City; 
                dbUser.Status = (int)UserStatusEnum.Pending;

                dbUser.Phone = userDetails.Mobile;

                try
                {
                    Context.Users.Add(dbUser);
                    Context.SaveChanges();
                }
                catch (DbEntityValidationException ex) 
                {
                    return ReturnError(ex?.EntityValidationErrors.FirstOrDefault().ValidationErrors.FirstOrDefault().ErrorMessage);
                }

                //return new ActionOutput
                //{
                //    ID = dbUser.UserId,
                //    Status = ActionStatus.Successfull,
                //    Message = "User Added Successfully, Verification link has been sent on user email account"
                //};

                return ReturnSuccess( dbUser.UserId, $"Registration Successful !! Confirnmation email sent to  {userDetails.Email}");
            }
        }

        UserModel IUserManager.GetUserDetailsByUserId(long userId)
        {
            if (userId == 0) return new UserModel();
            var user = Context.Users.Where(z => z.UserId == userId).FirstOrDefault();
            if (user == null)
                return null;
            else
            {
                var current_user_data = new UserModel(user);
                current_user_data.SelectedWidgets = user.UserAssignedWidgets.Select(e => e.WidgetId).ToList();
                return current_user_data;
            }
                
        }

        ActionOutput IUserManager.ChangeUserStatus(long userId,UserStatusEnum status)
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
                    Message = status==UserStatusEnum.Block? "User Blocked Successfully.":"User Activate Successfully."
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
            var user = Context.Users.FirstOrDefault(z => z.UserId == userId);
            if (user == null)
                return 0;
            if (user.UserRole.Role == UserRoles.Vendor || user.UserRole.Role == UserRoles.AppUser)
            {
            var posTotalBalance = Context.POS.Where(p => (p.VendorId != null && p.VendorId == user.FKVendorId) && p.Balance != null && !p.IsDeleted && p.Enabled != false).ToList().Sum(p => p.Balance);
                return posTotalBalance.Value;
            }
            else if (user.UserRole.Role == UserRoles.Admin)
            {
                var posTotalBalance = Context.POS.ToList().Sum(p => p.Balance);
                return posTotalBalance.Value;
            }
            else
            {
                return 0;
            }
            
        }

        bool RemoveORAddUserPermissions(long userId, AddUserModel model)
        { 
            var existingpermissons = Context.UserAssignedModules.Where(x => x.UserId == userId).ToList();
            if (existingpermissons.Count > 0)
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
            if (existingPlatforms.Count > 0)
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
            if (existing_widgets.Count > 0)
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


    }


}
