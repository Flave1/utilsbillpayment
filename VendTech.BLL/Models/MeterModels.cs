using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class MeterModel
    {
        public long UserId { get; set; }
        public long MeterId { get; set; }
        [Required(ErrorMessage = "Meter name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Meter # is required"), MaxLength(11, ErrorMessage = "Meter # must be of 11 digits"), MinLength(11, ErrorMessage = "Meter # must be of 11 digits")]
        public string Number { get; set; }
        public string Address { get; set; }
        public string MeterMake { get; set; }
        public string Allias { get; set; }
        public bool isVerified { get; set; }
    }

    public class MeterAPIListingModel : MeterModel
    {
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public MeterAPIListingModel() { }
        public MeterAPIListingModel(Meter obj)
        {
            UserId = obj.UserId;
            UserName = obj.User.Name + " " + obj.User.SurName;
            Name = obj.Name;
            MeterId = obj.MeterId;
            MeterMake = obj.MeterMake;
            CreatedAt = obj.CreatedAt;
            Address = obj.Address;
            Number = obj.Number;
            Allias = obj.Allias;
            isVerified = (bool)obj.IsVerified;
        }
    }



    public class RechargeMeterModel
    {
        public long UserId { get; set; }
        public long? MeterId { get; set; }
        public long? PlatformId { get; set; }
        [Required(ErrorMessage = "Pos Id is Required")]
        public long POSId { get; set; }
        [Required(ErrorMessage = "Amount is Required")]
        public decimal Amount { get; set; }

        public string MeterToken1 { get; set; }
        public string MeterToken2 { get; set; }
        public string MeterToken3 { get; set; }

        [MaxLength(11, ErrorMessage = "Meter Number must be of 11 digits"), MinLength(11, ErrorMessage = "Meter Number must be of 11 digits")]
        public string MeterNumber { get; set; }
        public bool SaveAsNewMeter { get; set; }
        public long TransactionId { get; set; }
        public bool IsSame_Request { get; set; } = false;
        public List<MeterRechargeApiListingModel> History { get; set; }
    }

    public class RechargeDetailPDFData
    {
        public string MeterNumber { get; set; }
        public decimal Amount { get; set; }
        public string CreatedAt { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string UserName { get; set; }

    }

    public class MeterRechargeApiListingModel
    {
        public long RechargeId { get; set; }
        public string MeterNumber { get; set; }
        public string ProductShortName { get; set; }
        public string RechargePin { get; set; }
        public string POSId { get; set; }
        public string UserName { get; set; }
        public string VendorName { get; set; }
        public long VendorId { get; set; }
        public decimal Amount { get; set; }
        public string CreatedAt { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public long MeterRechargeId { get; set; }
        public long? MeterId { get; set; }
    }
    public class SalesReportExcelModel
    {
        public string Date_TIME { get; set; }
        public string PRODUCT_TYPE { get; set; }
        public string TRANSACTIONID { get; set; }
        public string METER_NO { get; set; }
        public string VENDORNAME { get; set; }
        public string POSID { get; set; }
        //public string Request { get; set; }
        //public string Response { get; set; }
        public string PIN { get; set; }
        public string AMOUNT { get; set; }
    }
}
