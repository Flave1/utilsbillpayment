using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class PlatformApiManager : BaseManager, IPlatformApiManager
    {
        private static readonly IDictionary<int, PlatformApiTypeConfig> _TYPE_CONFIGS = GetApiTypeConfigs();
        private static readonly IDictionary<int, IPlatformApi> _INSTANCES = new Dictionary<int, IPlatformApi>();
        private readonly static object _LOCK = new object();
        private static readonly List<SelectListItem> _TYPES_LIST_ITEMS = new List<SelectListItem>();

        public ActionOutput SavePlatformApi(PlatformApiModel model)
        {
            var msg = "Platform Api updated successfully.";

            var api = new VendTech.DAL.PlatformApi();

            if (model.IsNotNew())
            {
                api = Context.PlatformApis.FirstOrDefault(p => p.Id == model.Id);
                if (api == null)
                    return ReturnError("PlatformApi #" + model.Id + " does not exist");

                api = model.To(api);
                api.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                api.UpdatedAt = DateTime.UtcNow;
                api.CreatedAt = api.UpdatedAt;

                api = model.To(api);
                Context.PlatformApis.Add(api);

                msg = "Platform API created successfully.";
            }

            Context.SaveChanges();
            return ReturnSuccess(msg);
        }

        public IDictionary<string, PlatformApiConfig> GetPlatformApiConfigFieldsForApiType(int type) {
            return GetPlatformApiInstanceByType(type).GetPlatformApiConfigFields();
        }

        public bool SavePlatformApiConfigValues(int platformApiId, IDictionary<string, string> configValues)
        {
            if (platformApiId > 0 && configValues != null)
            {
                VendTech.DAL.PlatformApi api = Context.PlatformApis.FirstOrDefault(p => p.Id == platformApiId);

                if (api == null) return false;

                api.UpdatedAt = DateTime.UtcNow;
                api.Config = JsonConvert.SerializeObject(configValues);

                Context.SaveChanges();
                return true;
            }

            return false;
        }

        public bool SavePlatformPacParamsConfigValues(int platformId, int platformApiConnId, IDictionary<string, string> configValues)
        {
            if (platformId > 0 && platformApiConnId > 0 && configValues != null)
            {
                VendTech.DAL.PlatformPacParam platformPacParam = Context.PlatformPacParams.FirstOrDefault(
                    p => p.PlatformId == platformId && p.PlatformApiConnectionId == platformApiConnId);

                if (platformPacParam == null)
                {
                    platformPacParam = new VendTech.DAL.PlatformPacParam
                    {
                        CreatedAt = DateTime.UtcNow,
                        PlatformId = platformId,
                        PlatformApiConnectionId = platformApiConnId
                    };

                    Context.PlatformPacParams.Add(platformPacParam);
                }

                platformPacParam.UpdatedAt = DateTime.UtcNow;
                platformPacParam.Config = JsonConvert.SerializeObject(configValues);

                Context.SaveChanges();
                return true;
            }

            return false;
        }

        public PlatformPacParams GetPlatformPacParams(int platformId, int platformApiConnId)
        {
            if (platformId > 0 && platformApiConnId > 0)
            {
                VendTech.DAL.PlatformPacParam platformPacParam = Context.PlatformPacParams.FirstOrDefault(
                    p => p.PlatformId == platformId && p.PlatformApiConnectionId == platformApiConnId);

                if (platformPacParam != null)
                {
                    return PlatformPacParams.From(platformPacParam);
                }
            }

            return null;
        }

        public IDictionary<string, HtmlFormElement> GetConfigFieldsAsHtmlForPlatformApi(int platformApiId, bool configurePlatformApi = true)
        {
            Dictionary<string, HtmlFormElement> configHtmlDictionary = new Dictionary<string, HtmlFormElement>();

            if (platformApiId > 0)
            {
                PlatformApiModel api = GetPlatformApiById(platformApiId);

                if (api.IsNotNew())
                {
                    IDictionary<string, PlatformApiConfig> apiConfigFields = GetPlatformApiConfigFieldsForApiType(api.ApiType);

                    Dictionary<string, string> configValues = null;

                    if (!string.IsNullOrEmpty(api.Config))
                    {
                        configValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(api.Config);
                    }

                    if (configValues == null) configValues = new Dictionary<string, string>();

                    foreach (var field in apiConfigFields)
                    {
                        string fieldName = field.Key;
                        PlatformApiConfig config = field.Value;

                        string currentValue = configValues.ContainsKey(fieldName) ? configValues[fieldName] : "";
                        HtmlFormElement htmlElement;
                        string tag;

                        //Are we configuring HTML elements for Platform API Connections configuration?
                        if ((!configurePlatformApi) && (!config.IsPerPlatformProductParam))
                        {
                            tag = "span";
                            htmlElement = new HtmlFormElement(config.Name, tag, fieldName, currentValue)
                            {
                                Style = config.HtmlStyle
                            };
                        }
                        //Platform API configuration
                        else if (configurePlatformApi && config.IsPerPlatformProductParam)
                        {
                            tag = "span";
                            htmlElement = new HtmlFormElement(config.Name, tag, fieldName, "Available in Per-Platform Product configuration")
                            {
                                Style = config.HtmlStyle
                            };
                        }
                        else
                        {
                            switch (config.FieldType)
                            {
                                case FieldType.DROPDOWN:
                                    tag = "select";
                                    break;
                                case FieldType.TEXT:
                                default:
                                    tag = "input";
                                    break;
                            }

                            htmlElement = new HtmlFormElement(config.Name, tag, fieldName, currentValue)
                            {
                                Style = config.HtmlStyle
                            };
                            htmlElement.AddCssClass("plat-api-cfg-elem").AddCssClass(config.HtmlCssClass ?? "");

                            //The possible values for dropdowns. 
                            var values = config.Values;
                            if (values != null && values.Count > 0)
                            {
                                foreach (var k in values)
                                {
                                    string nameOfValue = k.Value;
                                    string valueOfValue = k.Key;
                                    htmlElement.AddOption(nameOfValue, valueOfValue);
                                }
                            }
                        }

                        configHtmlDictionary.Add(fieldName, htmlElement);
                    }
                }
            }

            return configHtmlDictionary;
        }

        public PlatformApiModel GetPlatformApiById(int platformApiId)
        {
            if (platformApiId > 0)
            {
                DAL.PlatformApi api = Context.PlatformApis.FirstOrDefault(p => p.Id == platformApiId);
                if (api != null)
                {
                    return PlatformApiModel.From(this, api, CloneSelectList());
                }
            }

            //A hack so we can return the list incase the object is to be used for form binding 
            var model = PlatformApiModel.New();
            model.ApiTypeList = CloneSelectList();
            return model;
        }

        public ICollection<PlatformApiConnectionModel> GetPlatformApiConnectionsForPlatform(int platformId)
        {
            var query = Context.PlatformApiConnections.Where(p => p.PlatformId == platformId);
            List<PlatformApiConnectionModel> apiConns = new List<PlatformApiConnectionModel>();
            foreach (DAL.PlatformApiConnection apiConnection in query)
            {
                apiConns.Add(PlatformApiConnectionModel.From(this, apiConnection));
            }

            return apiConns;
        }

        public bool SavePlatformApiConnection(PlatformApiConnectionModel connection)
        {
            if (connection != null && connection.PlatformId > 0 && connection.PlatformApiId > 0)
            {
                VendTech.DAL.PlatformApiConnection apiConn;

                if (connection.IsNotNew())
                {
                    apiConn = Context.PlatformApiConnections.FirstOrDefault(p => p.Id == connection.Id);
                    if (apiConn != null)
                    {
                        apiConn = connection.To(apiConn);
                    }
                }
                else
                {
                    apiConn = connection.To(new VendTech.DAL.PlatformApiConnection());
                    apiConn.CreatedAt = DateTime.UtcNow;
                    Context.PlatformApiConnections.Add(apiConn);
                }

                if (apiConn == null) return false;

                apiConn.UpdatedAt = DateTime.UtcNow;
                Context.SaveChanges();

                connection.Id = apiConn.Id;

                return true;
            }

            return false;
        }

        public ICollection<PlatformApiModel> GetAllPlatformApis(bool includeDeleted = false) {
            var query = Context.PlatformApis;

            if (!includeDeleted)
            {
                query.Where(p => p.Status != (int)StatusEnum.Deleted);
            }

            List<SelectListItem> selectListItems = CloneSelectList();
            List<PlatformApiModel> apis = new List<PlatformApiModel>();
            foreach (var item in query)
            {
                apis.Add(PlatformApiModel.From(this, item, selectListItems));
            }

            return apis;
        }

        public ICollection<PlatformApiConnectionModel> GetAllPlatformApiConnectionsForPlatformApi(int platformApiId, bool includeDeleted = false)
        {
            var query = Context.PlatformApiConnections.Where(p => p.PlatformApiId == platformApiId);

            if (!includeDeleted)
            {
                query.Where(p => p.Status != (int)StatusEnum.Deleted);
            }

            List<SelectListItem> selectListItems = CloneSelectList();
            List<PlatformApiConnectionModel> apiConns = new List<PlatformApiConnectionModel>();
            foreach (VendTech.DAL.PlatformApiConnection item in query)
            {
                apiConns.Add(PlatformApiConnectionModel.From(this, item));
            }

            return apiConns;
        }

        public List<PlatformApiConnection> GetAllPlatformApiConnections()
        {
            return Context.PlatformApiConnections.OrderBy(p => p.Name).ToList();
        }

        public List<SelectListItem> GetAllPlatformApiConnectionsSelectList()
        {
            var apis = GetAllPlatformApiConnections();
            var apisList = new List<SelectListItem>();
            foreach (VendTech.DAL.PlatformApiConnection item in apis)
            {
                apisList.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
            }

            return apisList;
        }
    
            public List<SelectListItem> LoadPlatformApisForDropdown()
        {
            var query = Context.PlatformApis.Where(p => p.Status != (int)StatusEnum.Deleted);
            List<SelectListItem> apis = new List<SelectListItem>();
            foreach (VendTech.DAL.PlatformApi item in query)
            {
                apis.Add(new SelectListItem { Text = GetPlatformApiTypeClassName(item.ApiType) + " :: " + item.Name, Value = item.Id.ToString() });
            }

            return apis;
        }

        public string GetPlatformApiTypeClassName(int apiTypeId)
        {
            return GetPlatformApiTypeConfigByType(apiTypeId).ClassName;
        }

        public string GetPlatformApiTypeName(int apiTypeId)
        {
            return GetPlatformApiTypeConfigByType(apiTypeId).Name;
        }

            public IPlatformApi GetPlatformApiInstanceByTypeId (int pPlatformApiType)
        {
            return GetPlatformApiInstanceByType(pPlatformApiType);
        }

        public PlatformApiConnectionModel GetPlatformApiConnectionById(int apiConnId)
        {
            if (apiConnId > 0)
            {
                VendTech.DAL.PlatformApiConnection apiConnection = Context.PlatformApiConnections.Where(p => p.Id == apiConnId).FirstOrDefault();
                if (apiConnection != null)
                {
                    VendTech.DAL.PlatformApi platformApi = Context.PlatformApis.FirstOrDefault(p => p.Id == apiConnection.PlatformApiId);
                    apiConnection.PlatformApi = platformApi;
                    return PlatformApiConnectionModel.From(this, apiConnection);
                }
            }

            return null;
        }

        private static IDictionary<int, PlatformApiTypeConfig> GetApiTypeConfigs()
        {
            IDictionary<int, PlatformApiTypeConfig> configs = new Dictionary<int, PlatformApiTypeConfig>();

            //Sochitel
            configs.Add(1, new PlatformApiTypeConfig { Id = 1, Name = "Sochitel API", ClassName = "Sochitel" });
            configs.Add(2, new PlatformApiTypeConfig { Id = 2, Name = "EDSA API", ClassName = "Edsa" });

            return configs;
        }

        private static List<SelectListItem> CloneSelectList()
        {
            return ModelUtils.CloneSelectItemList( GetApiTypesSelectListItems() );
        }

        private static List<SelectListItem> GetApiTypesSelectListItems()
        {
            initInstances();
            return _TYPES_LIST_ITEMS;
        }

        private static IDictionary<int, IPlatformApi> GetApiInstances()
        {
            initInstances();
            return _INSTANCES;
        }

        private static IDictionary<int, PlatformApiTypeConfig> GetApiConfigs()
        {
            initInstances();
            return _TYPE_CONFIGS;
        }

        private static PlatformApiTypeConfig GetPlatformApiTypeConfigByType(int type)
        {
            PlatformApiTypeConfig config = GetApiConfigs()[type];
            if (config == null) throw new Exception("Invalid API Type #" + type);
            return config;
        }

        private static IPlatformApi GetPlatformApiInstanceByType(int type)
        {
            IPlatformApi api = GetApiInstances()[type];
            if (api == null) throw new Exception("Invalid API Type #" + type);

            return api;
        }

        private static void initInstances()
        {
            if (_INSTANCES.Count < 1)
            {
                lock (_LOCK)
                {
                    if (_INSTANCES.Count < 1)
                    {
                        foreach (PlatformApiTypeConfig config in _TYPE_CONFIGS.Values)
                        {
                            IPlatformApi platformApi = CreateInstance(config);
                            if (platformApi != null)
                            {
                                _INSTANCES.Add(config.Id, platformApi);

                                _TYPES_LIST_ITEMS.Add(new SelectListItem
                                {
                                    Text = config.ClassName,
                                    Value = config.Id.ToString(),
                                }); 
                            }
                        }
                    }
                }
            }
        }

        private static IPlatformApi CreateInstance(PlatformApiTypeConfig config)
        {
            string ns = "VendTech.BLL.PlatformApi";
            string className = "PlatformApi_" + config.ClassName; ;
            string fullClassName = ns + "." + className;

            Type IPlatformApiType = typeof(IPlatformApi);

            //@todo - Cache the fetching of the types
            Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> apiClassTypes = asm.GetTypes()
                                                .Where(
                                                    type =>
                                                    (!type.IsAbstract)
                                                     && type.IsClass
                                                     && type.FullName == fullClassName
                                                     && IPlatformApiType.IsAssignableFrom(type)
                                                    //&& type.Namespace == ns
                                                    //&& type.Name == fullClassName
                                                    //&& type.IsSubclassOf(IPlatformApiType)
                                                    )
                                                .Select(type => type);
            if (apiClassTypes.Count() > 0)
            {
                try
                {
                    Type type = apiClassTypes.ElementAt(0);
                    IPlatformApi instance = (IPlatformApi)Activator.CreateInstance(type);
                    return instance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return null;
        }
    }

}
