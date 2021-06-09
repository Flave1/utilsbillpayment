using VendTech.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VendTech.BLL.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendTech.BLL.Models
{
    /// <summary>
    /// Login Model this will be  used to login in the application
    /// </summary>
    public class LoginModal
    {
        [Required(ErrorMessage = "Required")]
        // [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Invalid Email")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
    }

    /// <summary>
    /// UserListing Model : This will be used to List all the users in Admin panel
    /// </summary>
    public class UserListingModel
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserStatusEnum Status { get; set; }
        public string UserType { get; set; }
        public string Permissions { get; set; }
        public string Platforms { get; set; }
        public string Vendor { get; set; }
        public Nullable<DateTime> LastLoggedIn { get; set; }
        public long PosBalance { get; set; }
        public string POSSerialNumber { get; set; }
        public long POSID { get; set; }
        public UserListingModel()
        {

        }

        public UserListingModel(User userObj)
        {
            var firstPos = userObj.POS.FirstOrDefault(d => d.IsDeleted == false && d.Balance != null);
            this.FirstName = userObj.Name;
            this.LastName = userObj.SurName;
            this.Email = userObj.Email;
            this.UserId = userObj.UserId;
            this.UserType = userObj.UserRole.Role;
            this.CreatedOn = userObj.CreatedAt;
            this.Status = (UserStatusEnum)userObj.Status;
            this.Platforms = string.Join(" , ", userObj.UserAssignedPlatforms.ToList().Select(x => x.Platform.Title).ToList());
            this.Permissions = string.Join(" , ", userObj.UserAssignedModules.Where(p => p.Module.Modules1.Count() == 0).ToList().Select(x => x.Module.ModuleName).ToList());
            this.Vendor = userObj.FKVendorId > 0 ? userObj.User1.Vendor : "";
            this.LastLoggedIn = userObj.AppLastUsed;
            this.PosBalance = firstPos != null ? Convert.ToInt64(firstPos.Balance.Value) : new long();
            this.POSSerialNumber = firstPos != null ? firstPos.SerialNumber : string.Empty;
            this.POSID = firstPos != null ? firstPos.POSId : new long();
        }

    }

    public class UserModel
    {
        public long UserId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Required")]
        [MaxLength(10)]
        [Index(IsUnique = true)]
        public string Phone { get; set; }
        public string Token { get; set; }
        public string CountryCode { get; set; }
        //When admin creating user from backend then there is a checkbox which let us know that user needs to reset password on first login or not
        public bool ResetUserPassword { get; set; }
        public string CompanyName { get; set; }
        public string ProfilePicUrl { get; set; }
        [Required(ErrorMessage = "Required")]
        public int UserType { get; set; }
        public string AccountStatus { get; set; }
        public IList<Checkbox> ModuleList { get; set; }
        public IList<PlatformCheckbox> PlatformList { get; set; }
        public IList<WidgetCheckbox> WidgetList { get; set; }
        public List<int> SelectedWidgets { get; set; }
        public List<int> SelectedModules { get; set; }

        public List<int> SelectedPlatforms { get; set; }
        public long? AgentId { get; set; }
        public long? VendorId { get; set; }
        //public long? POSId { get; set; }
        public string POSNumber { get; set; }
        public int Status { get; set; }

        public bool isemailverified { get; set; } 

        public decimal Percentage { get; set; }
        public HttpPostedFile Image { get; set; }
        public HttpPostedFileBase ImagefromWeb { get; set; }

        public bool IsRe_Approval { get; set; } = false;
        public UserModel() { }
        public UserModel(User userObj)
        {
            this.UserId = userObj.UserId;
            this.FirstName = userObj.Name;
            this.LastName = userObj.SurName;
            this.Email = userObj.Email;
            this.UserType = userObj.UserType;
            this.Phone = userObj.Phone;
            this.CompanyName = userObj.CompanyName;
            this.isemailverified = userObj.IsEmailVerified;
            this.Status = userObj.Status;

            ProfilePicUrl = string.IsNullOrEmpty(userObj.ProfilePic) ? "" : Utilities.DomainUrl + userObj.ProfilePic;
            this.AccountStatus = ((UserStatusEnum)(userObj.Status)).ToString();
            this.AgentId = userObj.AgentId;
            var userAssignedPos = new POS();
            if (userObj.UserRole.Role == UserRoles.Vendor)
                userAssignedPos = userObj.POS.FirstOrDefault(p => p.Enabled != false && !p.IsDeleted);
            else if (userObj.UserRole.Role == UserRoles.AppUser && userObj.User1 != null)
                userAssignedPos = userObj.User1.POS.FirstOrDefault(p => p.Enabled != false && !p.IsDeleted);
            if (userAssignedPos != null && userAssignedPos.POSId > 0)
            {
                POSNumber = userAssignedPos.SerialNumber;
            }
        }
        public UserModel(string sessionId, User obj)
            : this(obj)
        {
            Token = sessionId;
        }

    }

    public enum Individual_or_company
    {
        Individual = 1,
        Company = 2
    }

    public class RegisterAPIModel
    {
        [Required(ErrorMessage = "Email required")]
        public string Email { get; set; }
        public int Individual_or_company { get; set; }
        [Required(ErrorMessage = "Phone number required")]
        public string Mobile { get; set; }
        public string FirstName { get; set; }
        public string CompanyName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool IsCompany { get; set; }
        public string Agency { get; set; }
    }
    public class LoginAPIModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string DeviceToken { get; set; }
        public string AppType { get; set; }
    }
    public class LoginAPIPassCodeModel
    {
        public long UserId { get; set; }
        [Required]
        public string PassCode { get; set; }
        public string DeviceToken { get; set; }
        public string AppType { get; set; }
    }
    public class ApiResponseUserDetail
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string SurName { get; set; }
        public string ProfilePic { get; set; }
        public long UserId { get; set; }
        public string DOB { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public string AccountStatus { get; set; }
        public ApiResponseUserDetail(User obj)
        {
            Email = obj.Email;
            Name = obj.Name;
            SurName = obj.SurName;
            Address = obj.Address;
            UserName = obj.UserName;
            ProfilePic = string.IsNullOrEmpty(obj.ProfilePic) ? "" : Utilities.DomainUrl + obj.ProfilePic;
            UserId = obj.UserId;
            CompanyName = obj.CompanyName;

            DOB = obj.DOB == null ? "" : obj.DOB.Value.ToString("MM/dd/yyyy");
            City = obj.City != null ? obj.City.Name : "";
            Country = obj.Country != null ? obj.Country.Name : "";
            Phone = obj.Phone;
            AccountStatus = ((UserStatusEnum)obj.Status).ToString();
        }
    }
    public class AddUserModel : UserModel
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password should be minimum of 6 and maximum of 20 characters.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }
        public string Address { get; set; }
        public int PassCode { get; set; }

        public AddUserModel()
        {

        }

    }
    public class SignUpModel
    {
        public long UserId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public int? City { get; set; }
        public int? Country { get; set; }
        public string Address { get; set; }
        public string DeviceToken { get; set; }
        public string ReferralCode { get; set; }
        public AppTypeEnum AppType { get; set; }
        public AppUserTypeEnum AppUserType { get; set; }
    }
    public class VerifyAccountVerificationCodeMOdel
    {
        public string Code { get; set; }
        public long UserId { get; set; }
    }
    public class ResetPasswordModel
    {
        public long UserId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }
        public string OldPassword { get; set; }
        public string Otp { get; set; }
    }

    public class ChangePasswordModel
    {
        public long UserId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Required")]
        public string OldPassword { get; set; }
    }
    public class UpdateProfileModel
    {
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Required")]
        public string SurName { get; set; }
        public DateTime DOB { get; set; }
        public int? City { get; set; }
        public int? Country { get; set; }
        [Required(ErrorMessage = "Required")]
        [MaxLength(8)]
        public string Phone { get; set; }
        public string Address { get; set; }
        public HttpPostedFile Image { get; set; }
        public HttpPostedFileBase ImagefromWeb { get; set; }

        public string ProfilePicUrl { get; set; }
    }

    public class CountryModel
    {
        public int CountryId { get; set; }
        public string Name { get; set; }
    }
    public class CityModel
    {
        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string CountryName { get; set; }
    }
    public class Checkbox
    {
        public int ID { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public bool? IsAdmin { get; set; }
        public int? SubMenuOf { get; set; }
        public bool Checked { get; set; }
    }
    public class PlatformCheckbox
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Checked { get; set; }
    }
    public class WidgetCheckbox
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Checked { get; set; }
    }
    public class PermissonAndDetailModel
    {
        public UserDetails UserDetails { get; set; }
        public IList<ModulesModel> ModulesModelList { get; set; }
    }

    public class ModulesModel
    {
        //public int ModuleId { get; set; }
        //public string ModuleName { get; set; }
        public string ControllerName { get; set; }
        //public string Description { get; set; }
        //public bool? IsAdmin { get; set; }
        public ModulesModel() { }
        public ModulesModel(Module model)
        {
            //this.ModuleId = model.ModuleId;
            //  this.ModuleName = model.ModuleName;
            this.ControllerName = model.ModuleId.ToString();
            //this.Description = model.Description;
            //this.IsAdmin = model.IsAdmin;
        }
    }

    public class SaveLogoutTimeModel
    {
        public int Time { get; set; }
    }

}