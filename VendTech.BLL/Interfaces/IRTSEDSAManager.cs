﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface IRTSEDSAManager
    {
        Task<PagingResult<RtsedsaTransaction>> GetSalesInquiry(InquiryRequest model);
        Task<PagingResult<RtsedsaTransaction>> GetTransactionsAsync(TransactionRequest model);
        List<SelectListItem> MeterNumbers(long userID);
    }
}