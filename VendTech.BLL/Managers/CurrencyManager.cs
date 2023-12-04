using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.Models.CurrencyModel;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class CurrencyManager : BaseManager, ICurrencyManager
    {
        public ICollection<CurrencyDTO> GetCurrencies()
        {
            return Context.Countries.Where(d => d.Disabled == false).OrderBy(p => p.CountryName).Select(d => new CurrencyDTO
            {
                Id = d.CurrencySymbol,
                Name = d.CurrencyName,
            }).ToList();
        }

        PagingResult<CountryListingModel> ICurrencyManager.GetCountryPagedList(PagingModel model)
        {
            var result = new PagingResult<CountryListingModel>();
            IQueryable<Country> query = Context.Countries;

             if (model.SortBy == "CurrencyName")
            {
                query = query.OrderBy(r => r.CurrencyName + " " + model.SortOrder);
            }
            else if (model.SortBy == "CountryName")
            {
                query = query.OrderBy(r => r.CountryName + " " + model.SortOrder);
            }
            else if (model.SortBy == "CurrencyCode")
            {
                query = query.OrderBy(r => r.CurrencySymbol + " " + model.SortOrder);
            }
            else if (model.SortBy == "CountryCode")
            {
                query = query.OrderBy(r => r.CountryCode + " " + model.SortOrder);
            }

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("Currency"))
                    query = query.Where(z => z.CountryName.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Currency Code"))
                    query = query.Where(z => z.CountryCode.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Country"))
                    query = query.Where(z => z.CountryCode.ToLower().Contains(model.Search.ToLower()));
                if (model.SearchField.Equals("Country Code"))
                    query = query.Where(z => z.CurrencySymbol.ToLower().Contains(model.Search.ToLower()));
            }
            var list = query.AsEnumerable().Take(model.RecordsPerPage).Select(x => new CountryListingModel(x)).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Currency List";
            result.TotalCount = query.Count();
            return result;
        }

        ActionOutput ICurrencyManager.ChangeCountryStatus(int id, bool value)
        {
            var currency = Context.Countries.Where(z => z.CountryId == id).FirstOrDefault();
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
                currency.Disabled = value;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Currency status changed Successfully."
                };
            }
        }

        SaveCountryModel ICurrencyManager.GetSingle(int id)
        {
            return Context.Countries.Where(d => d.CountryId == id).AsEnumerable().Select(d => new SaveCountryModel(d)).FirstOrDefault();
        }
        ActionOutput ICurrencyManager.SaveCountry(SaveCountryModel model)
        {
            bool isNew = false;
            var dbCurrency = new Country();
            if (model.CountryId > 0)
            {
                dbCurrency = Context.Countries.FirstOrDefault(p => p.CountryId == model.CountryId);
                if (dbCurrency == null)
                {
                    dbCurrency = new Country();
                    isNew = true;
                }
            }
            else
            {
                if (Context.Countries.Any(d => d.CurrencySymbol.Contains(model.CurrencyCode.ToLower()) || d.CountryCode.Contains(model.CountryCode.ToLower())))
                {
                    return ReturnError("Country with same Symbol or country code  already exist");
                }
            }
            dbCurrency.CurrencySymbol = model.CurrencyCode;
            dbCurrency.CurrencyName = model.CurrencyName;
            dbCurrency.CountryCode = model.CountryCode;
            dbCurrency.CountryName = model.CountryName;

            if (isNew)
            {
                dbCurrency.CountryId = Context.Countries.Max(d => d.CountryId) + 1;
                Context.Countries.Add(dbCurrency);
            }
               
            SaveChanges();
          
            return ReturnSuccess("Country saved successfully.");
        }


        public IDictionary<string, CurrencyDTO> GetCurrenciesDictionaryKeyedById()
        {
            ICollection<CurrencyDTO> currencies = GetCurrencies();
            IDictionary<string, CurrencyDTO> result = new Dictionary<string, CurrencyDTO>();
            foreach (var currency in currencies)
            {
                result.Add(currency.Id, currency);
            }

            return result;
        }

        CountryDTO2 ICurrencyManager.RetrieveDomainCountry(string domain)
        {
            using (var vtcx = new VendtechEntities())
            {
                return vtcx.Countries.Where(d => d.DomainUrl.ToLower() == domain.ToLower())
                    .Select(f => new CountryDTO2
                    {
                        CountryId = f.CountryId,
                        CurrencyName = f.CurrencyName,
                        CurrencyCode = f.CurrencySymbol,
                        CountryName = f.CountryName,
                        CountryCode = f.CountryCode,
                        DomainUrl = f.DomainUrl,
                    }).FirstOrDefault() ?? new CountryDTO2();
            }
        }

        async Task<CountryDTO2> ICurrencyManager.RetrieveDomainCountryAsync(string domain)
        {
            using (var vtcx = new VendtechEntities())
            {
                return await vtcx.Countries.Where(d => d.DomainUrl.ToLower() == domain.ToLower())
                    .Select(f => new CountryDTO2
                    {
                        CountryId = f.CountryId,
                        CurrencyName = f.CurrencyName,
                        CurrencyCode = f.CurrencySymbol,
                        CountryName = f.CountryName,
                        CountryCode = f.CountryCode,
                        DomainUrl = f.DomainUrl,
                    }).FirstOrDefaultAsync() ?? new CountryDTO2();
            }
        }
    }
}
