﻿using System.Collections.Generic;

namespace VendTech.BLL.Models
{
    public class RtsedsaTransactionResp
    {
        public List<RtsedsaTransaction> Data { get; set; }
    }
    public class RtsedsaTransaction
    {
        public long Account { get; set; }
        public string CodUser { get; set; } = "";
        public string CustomerName { get; set; }
        public long DateTransaction { get; set; }
        public long DebtPayment { get; set; }
        public string MeterSerial { get; set; }
        public string Receipt { get; set; }
        public long TotalAmount { get; set; }
        public long TransactionId { get; set; }
        public double Unit { get; set; }
        public long UnitPayment { get; set; }
        public string UnitType { get; set; }
    }

    public class RtsedsaInquiryRequest
    {
        public Header Header { get; set; }
        public string MeterSerial { get; set; }
        public long DateFrom { get; set; }
        public long DateTo { get; set; }
    }
    public class RtsedsaTransactionRequest
    {
        public Header Header { get; set; }
        public long Date { get; set; }
    }

    public class Header
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string System { get; set; }
    }

    public class InquiryRequest
    {
        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public string MeterSerial { get; set; }
    }
    public class TransactionRequest
    {
        public long Date { get; set; }
    }
}
