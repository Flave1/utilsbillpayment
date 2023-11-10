using System.Collections.Generic;
using VendTech.BLL.Models;
using VendTech.BLL.Models.CurrencyModel;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface ICurrencyManager
    {
        ActionOutput ChangeCurrencyStatus(string id, bool value);
        ICollection<Currency> GetCurrencies();

        IDictionary<string, Currency> GetCurrenciesDictionaryKeyedById();
        PagingResult<CurrencyListingModel> GetCurrencyPagedList(PagingModel model);
        SaveCurrencyModel GetSingle(string id);
        ActionOutput SaveCurrency(SaveCurrencyModel model);
    }
    
}
