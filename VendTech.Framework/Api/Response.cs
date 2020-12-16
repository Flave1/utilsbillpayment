using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.Framework.Api
{
    public class ResponseBase
    {
        public string Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        public long? TotalCount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? StatusCode { get; set; }
    }

    public class Response : ResponseBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public dynamic result { get; set; }
    }
    public static class ResponseStatuCodes
    {
        public const int RegistrationOtpNotVerified = 1101;
    }
    public class Response<T> : ResponseBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T result { get; set; }
    }

    public enum Status
    {
        [Description("true")]
        Success,

        [Description("false")]
        Failed,

        EmptyModel
    }

    public static class Messages
    {
        public const string NOT_IMPLEMENTED = "Not implemented yet";
        public const string INCORRECT = "Incorrect username or password";
        public const string EMPTY_MODEL = "Model is Empty";
        public const string VERIFY_EMAIL = "A link has been sent to your email for verification. If you haven’t received any, check your junk ";
        public const string INVALID_IDENTITY = "Invalid indentity or client machine.";
        public const string INVALID_TOKEN = "Invalid token.";
        public const string MISSING_TOKEN = "Request is missing authorization token.";
        public const string USER_NOT_EXISTS = "User does not exist";
        public const string RESETPASWORD_LINK_SENT = "A link is sent to your registered email ID";
        public const string ALREADY_EXISTS = "Email ID already Exist";
        public const string PASSWORD_CHANGED = "Password Changed Successfully";
        public const string PASSWORD_MISMATCH = "Old Password not matched";
        public const string PROFILE_UPDATED = "Profile Updated";
        public const string ISE = "Internal Server Error";
        public const string FEEDBACK_SUBMITTED = "Your feedback has been submitted";
        public const string INQUIRY_SUBMITTED = "Your enquiry has been submitted";
        public const string NOT_MULTIPART = "Unsupported Media Type";
        public const string IMAGE_UPDATED = "Profile Image Updated";
        public const string NO_IMAGE = "No image received";
        public const string BUY_SUCCESS = "Tickets Bought";
        public const string WEEKLY_REGISTER_SUCCESS = "Successfully Registered for Weekly draw";
        public const string NO_ACTIVE_DRAW = "At present, no draw is active. Please try again later";
        public const string MOENY_RECIVED = "We have received your payment. Thanks.";
        public const string PAYMENT_CANCELED = "You have cancelled the payment.";
        public const string AUTH_FAIL = "Authentication failed for the user";
        public const string INVALID_TRANSACTION = "Invalid Transaction Token";
        public const string NOT_AVAIL = "Not Avialable";
        public const string FREE_TICKS_SEND = "2 free tickets has been sent to you.";
        public const string GCM_SESSION_EXPIRED = "Session has been expired";
    }

    public static class EmailSubjects
    {
        public const string VERY_EMAIL = "World Lottery App - Verify your Email";
    }
}
