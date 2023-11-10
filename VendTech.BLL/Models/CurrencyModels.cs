using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VendTech.DAL;

namespace VendTech.BLL.Models.CurrencyModel
{
    public class CurrencyListingModel
    {
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }

        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public bool isDeleted { get; set; }
        public string CreatedAt { get; set; }
        public CurrencyListingModel(Currency obj)
        {
            CurrencyName = obj.Name;
            CurrencyCode = obj.Id;
            CountryName = obj.CountryName;
            CountryCode = obj.CountryCode;
            isDeleted = obj.IsDeleted;
            CreatedAt = obj.CreatedAt != null ? obj.CreatedAt.Value.ToString("MM-DD-YYY"): "";
        } 
    }

    public class SaveCurrencyModel
    {
        [Required(ErrorMessage = "Currency Code Required")]
        public string CurrencyCode { get; set; }

        [Required(ErrorMessage = "Currency Name Required")]
        public string CurrencyName { get; set; }

        [Required(ErrorMessage = "Country Name Required")]

        public string CountryName { get; set; }

        [Required(ErrorMessage = "Country Code Required")]
        public string CountryCode { get; set; }
        public bool isDeleted { get; set; }
        public string CreatedAt { get; set; }
        public SaveCurrencyModel()
        {
            
        }
        public SaveCurrencyModel(Currency obj)
        {
            CurrencyName = obj.Name;
            CurrencyCode = obj.Id;
            CountryName = obj.CountryName;
            CountryCode = obj.CountryCode;
            isDeleted = obj.IsDeleted;
        }
    }


}
