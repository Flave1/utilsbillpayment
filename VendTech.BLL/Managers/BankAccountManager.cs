using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Web;
using System.IO;

namespace VendTech.BLL.Managers
{
    public class BankAccountManager : BaseManager, IBankAccountManager
    {
        List<BankAccountModel> IBankAccountManager.GetBankAccounts()
        {
            return Context.BankAccounts.Where(p => !p.IsDeleted).ToList().Select(x => new BankAccountModel
            {
                AccountName = x.AccountName,
                AccountNumber = x.AccountNumber,
                BankName = x.BankName,
                BBAN = x.BBAN,
                BankAccountId=x.BankAccountId
            }).ToList();
        }

        BankAccountModel IBankAccountManager.GetBankAccountDetail(long id)
        {
            var bank = Context.BankAccounts.FirstOrDefault(p => p.BankAccountId == id);
            if (bank == null)
                return null;
            var result = new BankAccountModel();
            result.BankName = bank.BankName;
            result.AccountNumber = bank.AccountNumber;
            result.BBAN = bank.BBAN;
            result.AccountName = bank.AccountName;
            result.BankAccountId = bank.BankAccountId;
            return result;
        }

        IEnumerable<CheqbankList> IBankAccountManager.GetBankNames_API()
        {
            var result = Context.ChequeBanks.Where(p=>p.isDeleted!=true &&p.isactive==true).ToList().Select(x=>new CheqbankList
            {
                BankCode = x.BankCode,
                BankName = x.BankCode+" - ("+ x.BankName+")",
                Id = x.id,
            }).ToList();
            return result;
        }
        ActionOutput IBankAccountManager.Delete(int id)
        {
           var bank = Context.BankAccounts.FirstOrDefault(p => p.BankAccountId == id);
            if (bank == null)
                return ReturnError("Account not exist");
            bank.IsDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Account deleted successfully.");
        }

        ActionOutput IBankAccountManager.SaveBankAccount(BankAccountModel model)
        {
            var msg = "Bank account updated successfully.";
            var bank = new BankAccount();
            if (model.BankAccountId > 0)
            {
                bank = Context.BankAccounts.FirstOrDefault(p => p.BankAccountId == model.BankAccountId);
                if (bank == null)
                    return ReturnError("Account not exist");
            }
            bank.BankName = model.BankName;
            bank.AccountNumber = model.AccountNumber;
            bank.BBAN = model.BBAN;
            bank.AccountName = model.AccountName;
            if (model.BankAccountId == 0)
            {
                bank.CreatedAt = DateTime.UtcNow;
                Context.BankAccounts.Add(bank);
                msg = "Bank account added successfully.";
            }
            Context.SaveChanges();
            return ReturnSuccess(msg);
        }
        //ActionOutput IBankAccountManager.DeleteBankAccount(int bankAccountId)
        //{
          
        //     var   bank = Context.BankAccounts.FirstOrDefault(p => p.BankAccountId == bankAccountId);
        //        if (bank == null)
        //            return ReturnError("Account not exist");
        //    if (model.BankAccountId == 0)
        //    {
        //        bank.CreatedAt = DateTime.UtcNow;
        //        Context.BankAccounts.Add(bank);
        //        msg = "Bank account added successfully.";
        //    }
        //    Context.SaveChanges();
        //    return ReturnSuccess(msg);
        //}
    }


}
