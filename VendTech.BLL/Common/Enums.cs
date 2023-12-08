using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VendTech.BLL.Models
{
    public enum ActionStatus
    {
        Successfull = 1,
        Error = 2,
        LoggedOut = 3,
        Unauthorized = 4
    }
    public enum NotificationStatusEnum
    {
        Success = 10,
        Failed = 20,
    }
    public enum TemplateTypes
    {
        [Description("Forget Password")]
        ForgetPassword = 2,
        [Description("Account Verification")]
        NewAppUser = 4,
        [Description("Change Password")]
        ChangePassword = 5,
        [Description("DepositOTP")]
        DepositOTP = 1,
        [Description("New App User Registration")]
        NewAppUserRegistration = 6,
        [Description("NewCMSUser")]
        NewCMSUser = 7,
        [Description("GeneratePasscode")]
        GeneratePasscode = 3,
        [Description("UserAccountReactivation")]
        UserAccountReactivation = 8,
        [Description("UserAccountReactivationForAdmin")]
        UserAccountReactivationForAdmin = 9,
        [Description("DepositApprovedNotification")]
        DepositApprovedNotification = 10,
        [Description("New User Email to Admin")]
        NewUserEmailToAdmin = 11,
        [Description("Deposit Request Notification")]
        DepositRequestNotification = 12,
        [Description("Uncleared Deposit Notification")]
        UnclearedDepositNotification = 13,
        [Description("Forgot Passcode")]
        ForgotPasscode = 14,
        [Description("Transfer From Notification")]
        TransferFromNotification = 15,
        [Description("Transfer To Notification")]
        TransferToNotification = 16,
        [Description("Receipt Via Email PDF")]
        SendReceiptViaEmail = 17,
        [Description("Receipt Via Email Content")]
        SendReceiptViaEmailContent = 18,
        [Description("Balance running low reminder")]
        BalanceLowReminder = 19,
    }
    public enum PosTypeEnum
    {
        [Description("WEB")]
        WEB = 10,
        [Description("APP")]
        APP = 20,
        [Description("POS")]
        POS = 30,
        [Description("API")]
        API = 40,


    }
    public enum AgentTypeEnum
    {
        [Description("WEB")]
        WEB = 10,
        [Description("APP")]
        APP = 20,
        [Description("POS")]
        POS = 30,
        [Description("API")]
        API = 40,
    }
    public enum UserStatusEnum
    {
        //Dont change status heading , in app there is check based on text
        [Description("Pending")]
        Pending = 0,
        [Description("Active")]
        Active = 1,
        [Description("Deleted")]
        Deleted = 2,
        [Description("Block")]
        Block = 3,
        [Description("Declined")]
        Declined = 4,
        [Description("PasswordNotReset")]
        PasswordNotReset = 5,
    }
    public enum AgencyStatusEnum
    {
        [Description("Active")]
        Active = 1,
        [Description("Deleted")]
        Deleted = 2,
    }
    //public enum UserTypeEnum
    //{
    //    [Description("AppUser")]
    //    AppUser = 9,
    //    [Description("Admin")]
    //    Admin = 2,
    //    [Description("Staff")]
    //    Staff = 5,
    //    [Description("Finance")]
    //    Finance = 6,
    //    [Description("Operations")]
    //    Operations = 7,
    //    [Description("Commercial")]
    //    Commercial = 8,
    //}
    public enum AppUserTypeEnum
    {
        [Description("Individual")]
        Individual = 10,
        [Description("Company")]
        Company = 20,
    }

    public enum NumberTypeEnum
    {
        [Description("Meter Meter")]
        MeterNumber = 0,
        [Description("Phone Number")]
        PhoneNumber = 1,
    }

    public enum AppUserTypeEnumApi
    {
        [Description("Personal Account")]
        Individual = 10,
        [Description("Corporate Account")]
        Company = 20,
    }
    public enum RechargeMeterStatusEnum
    {
        InProgress = 0,
        [Description("Success")]
        Success = 1,
        Pending = 2,
        Failed = 3,
        Error = 4,

    }

    public enum DepositPaymentTypeEnum
    {
        [Description("BANK DEPOSIT")]
        Cash = 1,
        [Description("CHEQUE")]
        Cheque = 2,
        [Description("PURCHASE ORD")]
        PurchaseOrder = 3,
        [Description("TRANSFER")]
        Transfer = 4,
        [Description("CASH/CHEQUE")]
        Cash_Cheque = 5,
        [Description("SMART KOPOR")]
        SmartKopor = 6,
        [Description("ADMIN TRANSFER OUT")]
        AdminTransferOut = 7,
        [Description("AGENCY COMMISION")]
        AgencyCommision = 8,
        [Description("VENDOR FLOAT IN")]
        VendorFloatIn = 9
    }
    public enum DepositPaymentStatusEnum
    {
        [Description("Pending")]
        Pending = 1,
        [Description("Rejected By Accountant")]
        RejectedByAccountant = 2,
        [Description("Approved By Accountant")]
        ApprovedByAccountant = 3,
        [Description("Released")]
        Released = 4,
        [Description("Rejected")]
        Rejected = 5,
        [Description("Rejected")]
        Reversed = 6,
    }
    public enum AppTypeEnum
    {
        IOS = 0,
        Android = 1
    }

    public enum NotificationTypeEnum
    {
        MeterRecharge=1,
        DepositStatusChange = 2,
        AirtimeRecharge = 3
    }

    /**
     * An Enum for the product (platform) types.
     */
    public enum PlatformTypeEnum
    {
        All = 0,
        AIRTIME = 1,
        CABLE_TV = 2,
        DATA = 3,
        ELECTRICITY = 4,
    }

    public enum StatusEnum
    {
        Active = 0,
        Disabled = 1,
        Deleted = 2,
    }

    public enum TransactionStatus
    {
        InProgress = 0,
        Pending = 1,
        Successful = 2,
        Failed = 3,
        Error = 4,
    }
    public enum InnerTransactionStatus
    {
        InProgress = 0,
        Pending = 1,
        Successful = 2,
        Failed = 3,
        Error = 4,
    }
    public enum OuterTransactionStatus
    {
        Failed = 0,
        Successful = 2
    }

    public enum UserScheduleTypes
    {
        LowBalnce = 0,
        PolicyUpdate = 1
    }
    public enum UserScheduleStatus
    {
        Pending = 0,
        Sent = 1,
        Funded = 2,
        NotSent = 3
    }
    public enum ApiLogType
    {
        InitialRequest = 1,
        PendingCheckRequest = 2,
    }

    public class EnumUtils
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            // Can't use type constraints on value types, so have to do check like this
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static string GetEnumName<T>(int value)
        {
            ValidateType<T>();
            return Enum.GetName(typeof(T), value);
        }

        private static void ValidateType<T>()
        {
            // Can't use type constraints on value types, so have to do check like this
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }
        }
    }



    /**
     * A helper class
     */
    public class NameValueModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}