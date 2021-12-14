using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VendTech.BLL.Interfaces
{
    public interface IBankAccountManager
    {
        List<BankAccountModel> GetBankAccounts();
        BankAccountModel GetBankAccountDetail(long id);
        IEnumerable<CheqbankList> GetBankNames_API();
        ActionOutput SaveBankAccount(BankAccountModel model);
        ActionOutput Delete(int id);
        List<ChequeBankModel> GetChequeBanks();
        ChequeBankModel GetChequeBankDetail(long id);
        ActionOutput SaveChequeBank(ChequeBankModel model);
        ActionOutput DeleteChequeBank(int id);
    }
    
}
