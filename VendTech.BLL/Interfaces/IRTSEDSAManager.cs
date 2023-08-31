using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IRTSEDSAManager
    {
        Task<PagingResult<RtsedsaTransaction>> GetSalesInquiry(InquiryRequest model);
        Task<PagingResult<RtsedsaTransaction>> GetTransactionsAsync(TransactionRequest model);
        Task<Dictionary<string, IcekloudQueryResponse>> QueryVendStatus(RechargeMeterModel model, TransactionDetail transDetail);
        Task<IceKloudResponse> RequestVendAsync(RechargeMeterModel model);
    }
}