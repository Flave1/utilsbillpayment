using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;
using VendTech.DAL;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace VendTech.BLL.Models
{

    public class PlatformApiModel : IntIdentifierModelBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int ApiType { get; set; }
        public string ApiTypeName { get; set; }

        [Required]
        public int Status { get; set; }
        public string StatusName { get; set; }

        [Required]
        public string Currency { get; set; }
        public string CurrencyName { get; set; }

        public string Config { get; set; }

        //Form Heplers
        public List<SelectListItem> ApiTypeList { get; set; }
        public List<SelectListItem> StatusTypeList { get; set; }
        public List<SelectListItem> CurrencyList { get; set; }  

        public static PlatformApiModel New()
        {
            return new PlatformApiModel();
        }

        public VendTech.DAL.PlatformApi To(VendTech.DAL.PlatformApi api)
        {
            if (api == null) throw new ArgumentNullException(nameof(api));

            api.Name = Name;
            api.ApiType = ApiType;
            api.Status = Status;
            api.Currency = Currency;
            api.Config = Config;

            return api;
        }

        public static PlatformApiModel From(IPlatformApiManager apiManager, VendTech.DAL.PlatformApi platformApi)
        {
            if (platformApi == null) throw new ArgumentNullException("PlatformApi is null");

            PlatformApiModel platformApiModel = new PlatformApiModel
            {
                Id = platformApi.Id,
                Name = platformApi.Name,
                ApiType= platformApi.ApiType,
                ApiTypeName = apiManager.GetPlatformApiTypeClassName(platformApi.ApiType),
                Status = platformApi.Status,
                StatusName = EnumUtils.GetEnumName<StatusEnum>(platformApi.Status),
                Currency = platformApi.Currency,
                CurrencyName = (platformApi.Currency1 != null) ? platformApi.Currency1.Name : null,
                Config = platformApi.Config
            };

            return platformApiModel;
        }

        public static PlatformApiModel From(IPlatformApiManager apiManager, VendTech.DAL.PlatformApi platformApi, List<SelectListItem> apiTypesList)
        {
            PlatformApiModel platformApiModel = From(apiManager, platformApi);
            platformApiModel.ApiTypeList = apiTypesList;
            return platformApiModel;
        }
    }
}
