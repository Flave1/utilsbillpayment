﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IPlatformTransactionManager
    {
        PlatformTransactionModel New(long UserId, int platformId, long posId, Decimal amount, string beneficiary, string currency, int? apiConnId);
        DataTableResultModel<PlatformTransactionModel> GetPlatformTransactionsForDataTable(DataQueryModel query);
        PlatformTransactionModel GetPlatformTransactionById(DataQueryModel query, long id);
        bool ProcessTransactionViaApi(long transactionId);
        List<PlatformApiLogModel> GetTransactionLogs(long transactionId);
        void CheckPendingTransaction();
    }
}