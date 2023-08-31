using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public string Alias { get; set; }
        public bool isVerified { get; set; } = true;
        public bool IsSaved { get; set; }
        public int NumberType { get; set; }
    }

    public class NumberModel
    {
        public long UserId { get; set; }
        public long MeterId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Number # is required"), MaxLength(8, ErrorMessage = "Number # must be of 8 digits"), MinLength(8, ErrorMessage = "Number # must be of 8 digits")]
        public string Number { get; set; }
        public string Address { get; set; }
        public string MeterMake { get; set; }
        public string Alias { get; set; }
        public bool isVerified { get; set; } = true;
        public bool IsSaved { get; set; }
        public int NumberType { get; set; }
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
            Alias = obj.Allias;
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
        public Nullable<int> PlatformId { get; set; }
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
        public int? PlatformId { get; set; }
        public string PlatformName { get; set; }
        public DateTime CreatedAtDate { get; set; }
        public MeterRechargeApiListingModel() { }
        public MeterRechargeApiListingModel(TransactionDetail x)
        {
            TransactionDetailsId = x.TransactionDetailsId;
            Amount = x.Amount;
            PlatformId = (int)x.PlatFormId;
            ProductShortName = x.Platform.Title;
            //if (x.Platform.Title != null)
            //{
            //if (x.Platform.PlatformId == 1)
            //    ProductShortName = x.Platform.ShortName;
            //else if (x.PlatFormId == 2)
            //    ProductShortName = "ORANGE";
            //else if (x.PlatFormId == 3)
            //    ProductShortName = "AFRICELL";

            //}
            CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm"),
            MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number;
            POSId = x.POSId == null ? "" : x.POS.SerialNumber;
            Status = ((RechargeMeterStatusEnum)x.Status).ToString();
            TransactionId = x.TransactionId;
            MeterRechargeId = x.TransactionDetailsId;
            RechargeId = x.TransactionDetailsId;
            UserName = x.User?.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : "");
            VendorName = x.POS.User == null ? "" : x.POS.User.Vendor;
            RechargePin = x.Platform.PlatformType == 4 ? Utilities.FormatThisToken(x.MeterToken1) : x.MeterNumber1 + "/" + x.TransactionId;
            PlatformName = x.Platform.Title;
        }

        public MeterRechargeApiListingModel(TransactionDetail x, int v)
        {
            Amount = x.Amount;
            TransactionId = x.TransactionId;
            MeterRechargeId = x.TransactionDetailsId;
            RechargeId = x.TransactionDetailsId;
            UserName = x.User.Name + (!string.IsNullOrEmpty(x.User.SurName) ? " " + x.User.SurName : "");
            PlatformId = (int)x.PlatFormId;
            ProductShortName = x.Platform.Title;
            //if (x.Platform.ShortName != null)
            //{
            //if (x.Platform.PlatformId == 1)
            //    ProductShortName = x.Platform.ShortName;
            //else if (x.Platform.PlatformId == 2)
            //    ProductShortName = "ORANGE";
            //else if (x.Platform.PlatformId == 3)
            //    ProductShortName = "AFRICELL";
            //else if (x.Platform.PlatformId == 4)
            //    ProductShortName = "QCELL";
            //}
            CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm"),
            MeterNumber = x.Meter == null ? x.MeterNumber1 : x.Meter.Number;
            POSId = x.POSId == null ? "" : x.POS.SerialNumber;
            Status = ((RechargeMeterStatusEnum)x.Status).ToString();
            VendorName = x.POS.User == null ? "" : x.POS.User.Vendor;
            RechargePin = x.Platform.PlatformType == 4 ? Utilities.FormatThisToken(x.MeterToken1) : x.MeterNumber1 + "/" + x.TransactionId;
            CreatedAtDate = x.CreatedAt;
            PlatformName = x.Platform.Title;
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
        public int? PlatformId { get; set; }
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
            PlatformId = x.PlatFormId;
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
        public SalesReportExcelModel() { }
        public SalesReportExcelModel(TransactionDetail x)
        {
            Date_TIME = x.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            PRODUCT_TYPE = x.Platform.Title;
            if (x.PlatFormId == 1)
                PIN = x.MeterToken1;
            else if (x.PlatFormId == 2)
                PIN = x.MeterNumber1;
            else if (x.PlatFormId == 3)
                PIN = x.MeterNumber1;
            AMOUNT = Utilities.FormatAmount(x.Amount);
            TRANSACTIONID = x.TransactionId;
            METER_NO = x.Meter == null ? x.MeterNumber1 : x.Meter.Number;
            VENDORNAME = x.POS.User == null ? "" : x.POS.User.Vendor;
            POSID = x.POSId == null ? "" : x.POS.SerialNumber;
        }
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

    public class MiniSalesReport
    {
        public string DateTime { get; set; }
        public string TAmount { get; set; }
    }

    public class RequestObject
    {
        public string token_string { get; set; }
    }

    public class RequestObject1
    {
        public string Id { get; set; }
    }
}
