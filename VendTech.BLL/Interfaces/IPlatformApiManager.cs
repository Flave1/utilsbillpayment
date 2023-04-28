using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;
using System.Web.Mvc;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IPlatformApiManager
    {
        IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFieldsForApiType(int type);
        ActionOutput SavePlatformApi(PlatformApiModel model);
        ICollection<PlatformApiModel> GetAllPlatformApis(bool includeDeleted = false);
        PlatformApiModel GetPlatformApiById(int PlatformApiId);
        string GetPlatformApiTypeName(int apiTypeId);
        string GetPlatformApiTypeClassName(int apiTypeId);
        IPlatformApi GetPlatformApiInstanceByTypeId(int pPlatformApiType);
        PlatformApiConnectionModel GetPlatformApiConnectionById(int apiConnId);
        IDictionary<string, HtmlFormElement> GetConfigFieldsAsHtmlForPlatformApi(int platformApiId, bool configurePlatformApi = false);
        bool SavePlatformApiConfigValues(int platformApiId, IDictionary<string, string> configValues);
        ICollection<PlatformApiConnectionModel> GetAllPlatformApiConnectionsForPlatformApi(int platformApiId, bool includeDeleted = false);
        List<SelectListItem> LoadPlatformApisForDropdown();
        ICollection<PlatformApiConnectionModel> GetPlatformApiConnectionsForPlatform(int platformId);
        bool SavePlatformApiConnection(PlatformApiConnectionModel connection);
        bool SavePlatformPacParamsConfigValues(int platformId, int platformApiConnId, IDictionary<string, string> configValues);

        PlatformPacParams GetPlatformPacParams(int platformId, int platformApiConnId);
        List<PlatformApiConnection> GetAllPlatformApiConnections();
        List<SelectListItem> GetAllPlatformApiConnectionsSelectList();
    }
}
