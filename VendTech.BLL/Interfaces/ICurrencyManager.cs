using System.Collections.Generic;
using VendTech.BLL.Models;
using VendTech.BLL.Models.CurrencyModel;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface ICurrencyManager
    {
        ActionOutput ChangeCountryStatus(int id, bool value);
        ICollection<CurrencyDTO> GetCurrencies();

        IDictionary<string, CurrencyDTO> GetCurrenciesDictionaryKeyedById();
        PagingResult<CountryListingModel> GetCountryPagedList(PagingModel model);
        SaveCountryModel GetSingle(int id);
        ActionOutput SaveCountry(SaveCountryModel model);
    }
    
}
