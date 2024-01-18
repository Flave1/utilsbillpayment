using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using VendTech.DAL;
using System.Threading.Tasks;
using System.Data.Entity;

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

        List<ChequeBankModel> IBankAccountManager.GetChequeBanks()
        {
            return Context.ChequeBanks.Where(p => p.isDeleted == false).ToList().Select(x => new ChequeBankModel
            {
                ChequeBanktId = x.id,
                BankName = x.BankName,
                BBAN = x.BankCode,
                IsActive = x.isactive,
                CreatedOn = x.Createdon
            }).ToList();
        }


        ChequeBankModel IBankAccountManager.GetChequeBankDetail(long id)
        {
            var bank = Context.ChequeBanks.FirstOrDefault(p => p.id == id);
            if (bank == null)
                return null;
            var result = new ChequeBankModel();
            result.BankName = bank.BankName;
            result.ChequeBanktId = bank.id;
            result.BBAN = bank.BankCode;
            result.IsActive = bank.isactive;
            result.CreatedOn = bank.Createdon;
            return result;
        }

        ActionOutput IBankAccountManager.SaveChequeBank(ChequeBankModel model)
        {
            var msg = "Bank account updated successfully.";
            var bank = new ChequeBank();
            if (model.ChequeBanktId > 0)
            {
                bank = Context.ChequeBanks.FirstOrDefault(p => p.id == model.ChequeBanktId);
                if (bank == null)
                    return ReturnError("Account not exist");
            }
            bank.BankName = model.BankName; 
            bank.BankCode = model.BBAN;
            if (model.ChequeBanktId == 0)
            {
                bank.isactive = true;
                bank.isDeleted = false;
                bank.Createdon = DateTime.UtcNow;
                bank.id = Context.ChequeBanks.Max(d => d.id)+1;
                Context.ChequeBanks.Add(bank);
                msg = "Bank added successfully.";
            }
            Context.SaveChanges();
            return ReturnSuccess(msg);
        }

        ActionOutput IBankAccountManager.DeleteChequeBank(int id)
        {
            var bank = Context.ChequeBanks.FirstOrDefault(p => p.id == id);
            if (bank == null)
                return ReturnError("Bank not exist");
            bank.isDeleted = true;
            Context.SaveChanges();
            return ReturnSuccess("Bank deleted successfully.");
        }

        async Task IBankAccountManager.PerformOperation()
        {
            var list = await Context.Deposits.Where(x => x.POSId > 0).ToListAsync();
            var we = 0;
        }
    }


}
