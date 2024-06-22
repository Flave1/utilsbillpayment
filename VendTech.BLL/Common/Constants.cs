using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VendTech.BLL.Models
{
    public static class Cookies
    {
        //replace xyz with required name as per your project
        public const string AuthorizationCookie = "xyzAuthorize";
        public const string AgentAuthorizationCookie = "xyzAgentAuthorize";
        public const string VendorAuthorizationCookie = "xyzVendorAuthorize";

        //replace xyz with required name as per your project
        public const string AdminAuthorizationCookie = "xyzAdminAuthorizeKey";

    }
    public static class SelectedAdminTab
    {
        public const string Users = "Users";
        public const string Platforms = "Platforms";
        public const string Agents = "Agents";
        public const string POS = "POS";
        public const string Vendors = "Vendors";
        public const string Deposits = "Deposits";
        public const string Reports = "Reports";
        public const string Password = "Password";
        public const string Profile = "Profile";
        public const string Meters = "Meters";
        public const string Transfer = "Transfer";
        public const string BillPayment = "BillPayment";
        public const string RTSEDSA = "RTSEDSA";
        public const string Currency = "Currency";


    }

    public static class SelectedSubMenuAdminTab
    {
        public const string CMS = "CMSUSer";
        public const string App = "APPUser";
        public const string DepositRelease = "DepositRelease";
        public const string DepositUpload = "DepositUpload";
        public const string CMSManager = "CMSManager";
        public const string Templates = "Templates";
    }
    public static class UserRoles
    {
        public const string Admin = "ADMIN";
        public const string Staff = "Staff";
        public const string Finance = "Finance";
        public const string Operations = "Operations";
        public const string Commercial = "Commercial";
        public const string AppUser = "AppUser";
        public const string B2BUsers = "B2BUsers";
        public const string Vendor = "Vendor";
        public const string Agent = "Vendor";
        public const string GENERAL_MANAGER = "GENERAL MANAGER"; //just added this
    }
    public static class AppDefaults
    {
        public const Int32 PageSize = 10;
    
    }
    public static class AppSettings
    {
        public const string LogoutTime = "LogoutTime";
    }

    public static class ApiCodes
    {
        public const string RESET_PASSCODE = "10001";
    }
}