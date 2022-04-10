using VendTech.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VendTech.BLL.Common;

namespace VendTech.BLL.Models
{
    public class ReportSearchModeluser
    {
        public ReportSearchModeluser()
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
        public long? VendorId { get; set; }
        public string ReportType { get; set; }
        public long? POS { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Meter { get; set; }
        public string RechargeToken { get; set; }
        public string refNumber { get; set; }
        public string TransactionId { get; set; }
        public int? BANK { get; set; }
        public int? DepositType { get; set; }
        public int PageNo { get; set; }
        public int RecordsPerPage { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }

}
