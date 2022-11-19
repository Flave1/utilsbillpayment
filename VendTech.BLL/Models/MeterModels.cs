using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Common;
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
        public bool isVerified { get; set; } = true;
        public bool IsSaved { get; set; }
    }

    public class MeterAPIListingModel : MeterModel
    { 
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public long POSId { get; set; }
        public string POSSerialNumber { get; set; }
        public string Balance { get; set; }
        public MeterAPIListingModel() { }
        public MeterAPIListingModel(Meter obj)
        {
            var pos = obj.User.POS.FirstOrDefault();
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
            POSId = pos.POSId;
            POSSerialNumber = pos.SerialNumber;
            Balance = Utilities.FormatAmount(pos.Balance);
        }
    }



    public class RechargeMeterModel
    {
        public bool IsSaved { get; set; }
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

        //[MaxLength(11, ErrorMessage = "Meter Number must be of 11 digits"), MinLength(11, ErrorMessage = "Meter Number must be of 11 digits")]
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

    public class LastMeterTransaction
    {
        public string RequestDate { get; set; }
        public string LastDealerBalance { get; set; }
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
        public long TransactionDetailsId { get; set; }
        public DateTime CreatedAtDate { get; set; }
        public MeterRechargeApiListingModel() { }
        public MeterRechargeApiListingModel(TransactionDetail x)
        {
            TransactionDetailsId = x.TransactionDetailsId;
            Amount = x.Amount;
            ProductShortName = x.Platform?.ShortName == null ? "" : x.Platform.ShortName;
            CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm"),
            MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number;
            POSId = x.POSId == null ? "" : x.POS.SerialNumber;
            Status = ((RechargeMeterStatusEnum)x.Status).ToString();
            TransactionId = x.TransactionId;
            MeterRechargeId = x.TransactionDetailsId;
            RechargeId = x.TransactionDetailsId;
            UserName = x.User?.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : "");
            VendorName = x.POS.User == null ? "" : x.POS.User.Vendor;
            RechargePin = x.MeterToken1;
        }

        public MeterRechargeApiListingModel(TransactionDetail x, int v)
        {
            Amount = x.Amount;
            TransactionId = x.TransactionId;
            MeterRechargeId = x.TransactionDetailsId;
            RechargeId = x.TransactionDetailsId;
            UserName = x.User.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : "");
            ProductShortName = x.Platform.ShortName == null ? "" : x.Platform.ShortName;
            CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm"),
            MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number;
            POSId = x.POSId == null ? "" : x.POS.SerialNumber;
            Status = ((RechargeMeterStatusEnum)x.Status).ToString();
            VendorName = x.POS.User == null ? "" : x.POS.User.Vendor;
            RechargePin = x.MeterToken1;
            CreatedAtDate = x.CreatedAt;
        }
    }

    public class GSTRechargeApiListingModel
    { 
        public string CreatedAt { get; set; } 
        public string TransactionId { get; set; }   
        public string Receipt { get; set; }
        public string MeterNumber { get; set; }    
        public decimal? Amount { get; set; }
        public decimal? ServiceCharge { get; set; }
        public decimal? Gst { get; set; }
        public decimal? UnitsCost { get; set; }
        public decimal? Tarrif { get; set; }
        public double Units { get; set; }
        public GSTRechargeApiListingModel() { }
        public GSTRechargeApiListingModel(TransactionDetail x)
        {
            CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm"),
            MeterNumber = x.MeterNumber1; 
            TransactionId = x.TransactionId;
            Receipt = x.ReceiptNumber;
            ServiceCharge = Convert.ToDecimal(x.ServiceCharge); ;
            Gst = Convert.ToDecimal(x.TaxCharge);
            UnitsCost = Convert.ToDecimal(x.CostOfUnits);
            Tarrif = Convert.ToDecimal(x.Tariff);
            Units = Convert.ToDouble(x.Units);
            Amount = x.Amount;
        }
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

    //public class GSTSalesReportExcelModel
    //{
    //    public string MeterNumber { get; set; }
    //    public decimal Amount { get; set; }
    //    public string CreatedAt { get; set; }
    //    public string TransactionId { get; set; }
    //    public string Receipt { get; set; }
    //    public string ServiceCharge { get; set; }
    //    public decimal Gst { get; set; }
    //    public decimal UnitsCost { get; set; }
    //    public decimal Tarrif { get; set; }
    //    public decimal Units { get; set; } 
    //}

}
