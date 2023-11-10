using System;
using System.Collections.Generic;
using System.Linq;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.Models.CurrencyModel;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class CurrencyManager : BaseManager, ICurrencyManager
    {
        public ICollection<Currency> GetCurrencies()
        {
            return Context.Currencies.OrderBy(p => p.Name).ToList();
        }

        PagingResult<CurrencyListingModel> ICurrencyManager.GetCurrencyPagedList(PagingModel model)
        {
            var result = new PagingResult<CurrencyListingModel>();
            IQueryable<Currency> query = Context.Currencies;

             if (model.SortBy == "CurrencyName")
            {
                query = query.OrderBy(r => r.Name + " " + model.SortOrder);
            }
            else if (model.SortBy == "CountryName")
            {
                query = query.OrderBy(r => r.CountryName + " " + model.SortOrder);
            }
            else if (model.SortBy == "CurrencyCode")
            {
                query = query.OrderBy(r => r.Id + " " + model.SortOrder);
            }
            else if (model.SortBy == "CountryCode")
            {
                query = query.OrderBy(r => r.CountryCode + " " + model.SortOrder);
            }

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("Currency"))
                    query = query.Where(z => z.Name.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Currency Code"))
                    query = query.Where(z => z.CountryCode.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Country"))
                    query = query.Where(z => z.CountryCode.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Country Code"))
                    query = query.Where(z => z.Id.ToLower().Contains(model.Search.ToLower()));
            }
            var list = query.AsEnumerable().Take(model.RecordsPerPage).Select(x => new CurrencyListingModel(x)).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Currency List";
            result.TotalCount = query.Count();
            return result;
        }

        ActionOutput ICurrencyManager.ChangeCurrencyStatus(string id, bool value)
        {
            var currency = Context.Currencies.Where(z => z.Id == id).FirstOrDefault();
            if (currency == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Currency Not Exist."
                };
            }
            else
            {
                currency.IsDeleted = value;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Currency status changed Successfully."
                };
            }
        }

        SaveCurrencyModel ICurrencyManager.GetSingle(string id)
        {
            return Context.Currencies.Where(d => d.Id == id).AsEnumerable().Select(d => new SaveCurrencyModel(d)).FirstOrDefault();
        }
        ActionOutput ICurrencyManager.SaveCurrency(SaveCurrencyModel model)
        {
            bool isNew = false;
            var dbCurrency = new Currency();
            if (!string.IsNullOrEmpty(model.CountryCode))
            {
                dbCurrency = Context.Currencies.FirstOrDefault(p => p.Id.ToLower() == model.CurrencyCode.ToLower());
                if (dbCurrency == null)
                {
                    dbCurrency = new Currency();
                    isNew = true;
                }
            }
            else
            {
                if (Context.Currencies.Any(d => d.Id.Contains(model.CountryCode.ToLower())))
                {
                    return ReturnError("Currency with same ID already exist");
                }
            }
            dbCurrency.Id = model.CurrencyCode;
            dbCurrency.Name = model.CurrencyName;
            dbCurrency.CountryCode = model.CountryCode;
            dbCurrency.CountryName = model.CountryName;

            if (isNew)
                Context.Currencies.Add(dbCurrency);
            SaveChanges();
          
            return ReturnSuccess("Currency saved successfully.");
        }


        public IDictionary<string, Currency> GetCurrenciesDictionaryKeyedById()
        {
            ICollection<Currency> currencies = GetCurrencies();
            IDictionary<string, Currency> result = new Dictionary<string, Currency>();
            foreach (var currency in currencies)
            {
                result.Add(currency.Id, currency);
            }

            return result;
        }
    }
}
