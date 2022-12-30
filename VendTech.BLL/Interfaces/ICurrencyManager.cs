using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface ICurrencyManager
    {
        ICollection<Currency> GetCurrencies();

        IDictionary<string, Currency> GetCurrenciesDictionaryKeyedById();
    }
    
}
