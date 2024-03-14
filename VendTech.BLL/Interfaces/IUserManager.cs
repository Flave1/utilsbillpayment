using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IUserManager
    {


        /// <summary>
        /// Dummy Method for testing purpose:  Get Welcome Message
        /// </summary>
        /// <returns></returns>
        string GetWelcomeMessage();
        UserModel ValidateUserSession(string token);
        bool UpdateUserLastAppUsedTime(long userId);
        ActionOutput<ApiResponseUserDetail> GetUserDetailsForApi(long userId);
        ActionOutput UpdateUserProfile(long userId, UpdateProfileModel model);
        ActionOutput UpdateAdminprofile(long userId, UpdateProfileModel model);

        PagingResult<NotificationApiListingModel> GetUserNotifications(int pageNo, int pageSize, long userId);
        DataResult<List<MeterRechargeApiListingModel>, List<DepositListingModel>, ActionStatus> GetUserNotificationApi(int pageNo, int pageSize, long userId);
        ActionOutput<string> SaveReferralCode(long userId);
        IList<PlatformCheckbox> GetAllPlatforms(long userId);
        List<SelectListItem> GetAppUsersSelectList();
        List<SelectListItem> GetUserRolesSelectList();
        ActionOutput AddAppUserDetails(AddUserModel userDetails);
        ActionOutput AddAppUserDetails(RegisterAPIModel userDetails);
        UpdateProfileModel GetAppUserProfile(long userId);
        /// <summary>
        /// This will be used to get user listing model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ActionOutput UpdateProfilePic(long userId, HttpPostedFile image);
        /// 
        PagingResult<UserListingModel> GetUserPagedList(PagingModel model, bool onlyAppUser = false,string status="");

        UserDetails GetNotificationUsersCount(long currentUserId);
        IList<Checkbox> GetAllModules(long userId);

        /// <summary>
        /// Update User Details from Admin Panel
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        ActionOutput UpdateUserDetails(AddUserModel userDetails);
        ActionOutput UpdateAppUserDetails(AddUserModel userDetails);

        /// <summary>
        /// Add User Details from Admin Panel
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        ActionOutput AddUserDetails(AddUserModel userDetails);

        /// <summary>
        /// Get User Details by User Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserModel GetUserDetailsByUserId(long userId);
        AddUserModel GetAppUserDetailsByUserId(long userId);

        /// <summary>
        /// Delete User By User Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        ActionOutput DeleteUser(long userId);
        ActionOutput DeclineUser(long userId);
        ActionOutput ChangeUserStatus(long userId, UserStatusEnum status);
        ActionOutput<UserDetailForAdmin> AdminLogin(LoginModal model);
        ActionOutput<UserDetails> AgentLogin(LoginModal model);
        ActionOutput<UserDetails> VendorLogin(LoginModal model);
        IList<ModulesModel> GetAllModulesAtAuthentication(long userId); 
        decimal GetUserWalletBalance(User user, long agent = 0);
        decimal GetUserWalletBalance(long userId, long agent = 0);
        int GetUnreadNotifications(long userId);
        List<SelectListItem> GetAssignedReportModules(long UserId, bool isAdmin);
        IList<WidgetCheckbox> GetAllWidgets(long userId);
        IList<UserAssignedModuleModel> GetNavigations(long userId);
        long GetUserId(string phone);
        string GetUserPasswordbyUserId(long userId);
        List<User> GetAllAdminUsersByAppUserPermission();
        IEnumerable<UserLiteDto> GetVendorNames_API();
        UserLiteDto GetVendorNamePOSNumber(int posId);
        List<User> GetAllAdminUsersByDepositRelease();
        User GetUserDetailByEmail(string email);
        List<SelectListItem> GetAgentSelectList();
        UserLogo GetUserLogo(long userId);
        UserDetails BackgroundAdminLogin(long taskId);
        long GetVendtechAgencyId();
        void SaveChanges();
        PendingDeposit GetUserPendingDeposit(long userId);
    }

}
