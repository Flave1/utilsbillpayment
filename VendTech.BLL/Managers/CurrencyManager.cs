using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class CurrencyManager : BaseManager, ICurrencyManager
    {
        public ICollection<Currency> GetCurrencies()
        {
            return Context.Currencies.OrderBy(p => p.Name).ToList();
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
