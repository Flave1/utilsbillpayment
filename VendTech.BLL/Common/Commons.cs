using System;
using System.Collections.Generic;

namespace VendTech.BLL.Models
{
    public class ActionOutputBase
    {
        public ActionStatus Status { get; set; }
        public String Message { get; set; }
        public List<String> Results { get; set; }
    }

    public class ActionOutput<T> : ActionOutputBase
    {
        public T Object { get; set; }
        public List<T> List { get; set; }
    }

    public class ActionOutput : ActionOutputBase
    {
        public long ID { get; set; }
    }

    public class PagingResult<T>
    {
        public List<T> List { get; set; }
        public int TotalCount { get; set; }
        public ActionStatus Status { get; set; }
        public String Message { get; set; }
    } 
    public class PagingModel
    {
        public int PageNo { get; set; }
        public int RecordsPerPage { get; set; }
        public PagingModel()
        {
            if (PageNo <= 1)
            {
                PageNo = 1;
            }
            if (RecordsPerPage <= 0)
            {
                RecordsPerPage = AppDefaults.PageSize;
            }
        }

        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string Search { get; set; }
        public string SearchField { get; set; }
        public string VendorId { get; set; }
        public bool IsActive { get; set; } = true;

        public static PagingModel DefaultModel(string sortBy = "CreatedOn", string sortOder = "Asc")
        {
            return new PagingModel { PageNo = 1, RecordsPerPage = AppDefaults.PageSize, SortBy = sortBy, SortOrder = sortOder };
        }
    }

    public class UserDetails
    {
        public long UserID { get; set; }
        public string UserType { get; set; }
        public String FirstName { get; set; }
        public String UserName { get; set; }
        public String LastName { get; set; }
        public String UserEmail { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsEmailVerified { get; set; }
        public int Status { get; set; }

        public string ProfilePicPath { get; set; }
        public int RemainingAppUser { get; set; }
        public int RemainingDepositRelease { get; set; }
        public string AppUserMessage { get; set; }
        public string DepositReleaseMessage { get; set; }
        public bool IsCompany { get; set; }
        public long AgencyId { get; set; }
        public string VendorName { get; set; }
        public string AgencyName { get; set; }

        public UserDetails()
        { }
    }

    public class UserDetailForAdmin
    {
        public long UserID { get; set; }
        public string UserType { get; set; }
        public String FirstName { get; set; }
        public String UserName { get; set; }
        public String LastName { get; set; }
        public String UserEmail { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsEmailVerified { get; set; }
        public int Status { get; set; }

        //public string ProfilePicPath { get; set; }
        public int RemainingAppUser { get; set; }
        public int RemainingDepositRelease { get; set; }
        public string AppUserMessage { get; set; }
        public string DepositReleaseMessage { get; set; }
        public bool IsCompany { get; set; }
        public UserDetailForAdmin()
        { }


    }
    public class ExceptionModal
    {
        public Exception Exception { get; set; }
        public UserDetails User { get; set; }
        public string FormData { get; set; }
        public string QueryData { get; set; }
        public string RouteData { get; set; }
        public string BrowserName { get; set; }
    }

    public class ExceptionReturnModal
    {
        public string ErrorID { get; set; }
        public string ErrorText { get; set; }
        public bool DatabaseLogStatus { get; set; }
    }

    public class FetchItemsModel
    {
        public string Id { get; set; }
    }

    public class CashTransferModel
    {
        public long FromPosId { get; set; }
        public long ToPosId { get; set; }
        public decimal Amount { get; set; }
    }

}