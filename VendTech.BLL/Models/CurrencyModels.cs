using System.ComponentModel.DataAnnotations;
using VendTech.DAL;

namespace VendTech.BLL.Models.CurrencyModel
{
    public class CountryListingModel
    {
        public int CountryId { get; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }

        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public bool Disabled { get; set; }
        public string CreatedAt { get; set; }
        public CountryListingModel(Country obj)
        {
            CountryId = obj.CountryId;
            CurrencyName = obj.CurrencyName;
            CurrencyCode = obj.CurrencySymbol;
            CountryName = obj.CountryName;
            CountryCode = obj.CountryCode;
            Disabled = obj.Disabled;
            CreatedAt = obj.CreatedAt != null ? obj.CreatedAt.Value.ToString("MM-DD-YYY"): "";
        } 
    }

    public class SaveCountryModel
    {
        [Required(ErrorMessage = "Currency Code Required")]
        public string CurrencyCode { get; set; }

        [Required(ErrorMessage = "Currency Name Required")]
        public string CurrencyName { get; set; }

        [Required(ErrorMessage = "Country Name Required")]

        public string CountryName { get; set; }

        [Required(ErrorMessage = "Country Code Required")]
        public string CountryCode { get; set; }
        public bool Disabled { get; set; }
        public string CreatedAt { get; set; }
        public int CountryId { get; set; }
        public SaveCountryModel()
        {
            
        }
        public SaveCountryModel(Country obj)
        {
            CountryId = obj.CountryId;
            CurrencyName = obj.CurrencyName;
            CurrencyCode = obj.CurrencySymbol;
            CountryName = obj.CountryName;
            CountryCode = obj.CountryCode;
            Disabled = obj.Disabled;
        }
    }

    public class CurrencyDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CountryDTO
    {
        public int CountryId { get; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public CountryDTO(Country obj)
        {
            CountryId = obj.CountryId;
            CurrencyName = obj.CurrencyName;
            CurrencyCode = obj.CurrencySymbol;
            CountryName = obj.CountryName;
            CountryCode = obj.CountryCode;
        }
    }

    public class CountryDTO2
    {
        public int CountryId { get; set; } = 3;
        public string CurrencyName { get; set; } = "Sierra Leone LEONE (SLE)";
        public string CurrencyCode { get; set; } = "SLE";
        public string CountryName { get; set; } = "SIERRA LEONE";
        public string CountryCode { get; set; } = "+232";
        public string DomainUrl { get; set; } = "-t";
    }

}
