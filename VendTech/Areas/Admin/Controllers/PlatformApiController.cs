using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Configuration;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;
using VendTech.DAL;

namespace VendTech.Areas.Admin.Controllers
{
    public class PlatformApiController : AdminBaseController {

        private readonly IPlatformApiManager _platformApiManager;
        private readonly ICurrencyManager _currencyManager;
        private readonly IPlatformManager _platformManager;

        public PlatformApiController(
            ICurrencyManager currencyManager,
            IPlatformApiManager platformApiManager, 
            IErrorLogManager errorLogManager,
            IPlatformManager platformManager)
        : base (errorLogManager)
        {
            _platformApiManager = platformApiManager;
            _currencyManager = currencyManager;
            _platformManager = platformManager;
        }
    
        // GET: Admin/PlatformApi
        public ActionResult Index()
        {
            ViewBag.MainPageHeader = "Platform APIs";
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;

            return View();
        }

        public ActionResult PlatformApis()
        {
            ViewBag.MainPageHeader = "Platform APIs";
            ViewBag.Title = "Platform APIs";
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;

            ICollection<PlatformApiModel> apiModels = _platformApiManager.GetAllPlatformApis();
            return View(apiModels);
        }

        [HttpGet]
        public ActionResult CreateOrEditApi(int id = 0) {
            PlatformApiModel model = _platformApiManager.GetPlatformApiById(id);
            bool isEdit = model.IsNotNew();
            ViewBag.IsEdit = isEdit;
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.MainPageHeader = isEdit ? "Edit - " + model.Name + " #" + model.Id : "Create Platform API";
            
            ViewBag.Title = (isEdit ? "Edit" : "Create") + " Platform API";

            model.StatusTypeList = ModelUtils.GetStatusEnumSelectItemList();
            model.CurrencyList = ModelUtils.GetCurrencySelectItemList(_currencyManager.GetCurrencies());

            return View(model); 
        }


        [AjaxOnly, HttpPost]
        public JsonResult UpdatePlatformApi(PlatformApiModel model)
        {
            //TODO - validate the data

            return JsonResult(_platformApiManager.SavePlatformApi(model));
        }

        [HttpGet]
        public ActionResult PlatformApiConnections(int id)
        {
            PlatformModel model = _platformManager.GetPlatformById(id);
            if (model == null) return RedirectToAction("PlatformApis");

            ICollection<PlatformApiConnectionModel> apiConns = _platformApiManager.GetPlatformApiConnectionsForPlatform(id);

            ViewBag.MainPageHeader = "API Connections for " + model.Title + " (#" + model.PlatformId + ")";
            ViewBag.Title = "API Connections";
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.PlatformName = model.Title;
            ViewBag.PlatformId = model.PlatformId;
           
            return View(apiConns);
        }

        [HttpGet]
        public ActionResult CreateApiConnection()
        {
            int id = int.Parse(Request.QueryString["pid"]);
            PlatformModel model = _platformManager.GetPlatformById(id);
            if (model == null) return RedirectToAction("PlatformApis");

            ViewBag.MainPageHeader = "Create API Connection for " + model.Title + " (#" + id + ")";
            ViewBag.Title = "Create Platform API Conn";
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.PlatformName = model.Title;
            ViewBag.PlatformId = model.PlatformId;
            ViewBag.StatusTypeList = ModelUtils.GetStatusEnumSelectItemList();
            ViewBag.ApiList = _platformApiManager.LoadPlatformApisForDropdown();

            PlatformApiConnectionModel connModel = PlatformApiConnectionModel.New();
            connModel.PlatformId = id;

            return View(connModel);
        }

        [HttpPost]
        public ActionResult CreateApiConnection(PlatformApiConnectionModel connModelForm)
        { 
            PlatformModel model = _platformManager.GetPlatformById(connModelForm.PlatformId);
            if (model == null) return RedirectToAction("PlatformApis");

            //Validate
            if (_platformApiManager.SavePlatformApiConnection(connModelForm))
            {
                Session.Add("_fromSave", "true");
                PlatformApiConnectionModel apiConn = _platformApiManager.GetPlatformApiConnectionById(connModelForm.Id);
                return RedirectToAction("EditApiConnection", "PlatformApi", new { id = connModelForm.Id });
            }

            //
            return RedirectToAction("PlatformApiConnections", "PlatformApi", new { area = "Admin", id = connModelForm.PlatformId }); ;
        }


        [HttpGet]
        public ActionResult EditApiConnection(int id)
        {
            PlatformApiConnectionModel model = _platformApiManager.GetPlatformApiConnectionById(id);
            if (model == null) return RedirectToAction("PlatformApis");

            ViewBag.MainPageHeader = "Edit API Connection - " + model.Name + " (#" + id + ")";
            ViewBag.Title = "Edit Platform API Connection";
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.PlatformId = model.PlatformId;
            ViewBag.StatusTypeList = ModelUtils.GetStatusEnumSelectItemList();
            ViewBag.ApiList = _platformApiManager.LoadPlatformApisForDropdown();

           string fromCreate = Session["_fromSave"] as string;

            if ( ! string.IsNullOrWhiteSpace(fromCreate) )
            {
                Session.Remove("_fromSave");
                ViewBag.Message = "Platform API Connection saved successfully";
            }

           return View(model);
        }

        [HttpPost]
        public ActionResult EditApiConnection(PlatformApiConnectionModel connModelForm)
        {
            PlatformApiConnectionModel model = _platformApiManager.GetPlatformApiConnectionById(connModelForm.Id);
            if (model == null) return RedirectToAction("PlatformApis");
            
            if (_platformApiManager.SavePlatformApiConnection(connModelForm))
            {
                Session.Add("_fromSave", "fromSave");
                PlatformApiConnectionModel apiConn = _platformApiManager.GetPlatformApiConnectionById(connModelForm.Id);
                return RedirectToAction("EditApiConnection", "PlatformApi", new { id = connModelForm.Id });
            }

            //
            return RedirectToAction("PlatformApiConnections", "PlatformApi", new { area = "Admin", id = connModelForm.PlatformId }); ;
        }

        [HttpGet]
        public ActionResult ConfigureApiConnection(int id)
        {
            PlatformApiConnectionModel model = _platformApiManager.GetPlatformApiConnectionById(id);
            if (model.IsNew()) return RedirectToAction("PlatformApis");

            int platformApiId = (int) model.PlatformApiId;

            IDictionary<string, HtmlFormElement> htmlFields = 
                _platformApiManager.GetConfigFieldsAsHtmlForPlatformApi(platformApiId, false);

            if (htmlFields.Count == 0) return RedirectToAction("PlatformApis");

            PlatformPacParams platformPacParams = _platformApiManager.GetPlatformPacParams(model.PlatformId, model.Id);
            if (platformPacParams != null && platformPacParams.ConfigDictionary != null)
            {
                foreach(var kvp in platformPacParams.ConfigDictionary)
                {
                    if (htmlFields.ContainsKey(kvp.Key))
                    {
                        HtmlFormElement elem = htmlFields[kvp.Key];
                        elem.Value = kvp.Value;
                    }
                }
            }

            PrepareConfigApiConnectionView(model);

            string flashMessage = Session["flashMessage"] as string;

            if (!string.IsNullOrEmpty(flashMessage))
            {
                ViewBag.Message = flashMessage;
                Session.Remove("flashMessage");
            }

            return View(htmlFields);
        }

        private void PrepareConfigApiConnectionView(PlatformApiConnectionModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.Title = "Configure API Connection";
            ViewBag.MainPageHeader = "Configure API Connection - " + model.Name + " (#" + model.Id + ")";
            ViewBag.PlatforApiConnId = model.Id;
            ViewBag.PlatformId = model.PlatformId;
        }

        [HttpPost]
        public ActionResult ConfigureApiConnection(FormCollection formCollection)
        {
            string id = formCollection.Get("_platformApiConnId");
            if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out int intIdValue))
            {
                PlatformApiConnectionModel model = _platformApiManager.GetPlatformApiConnectionById(intIdValue);
                if (model.IsNew()) return RedirectToAction("PlatformApis");

                int platformApiId = (int)model.PlatformApiId;
                IDictionary<string, HtmlFormElement> htmlFields = 
                    _platformApiManager.GetConfigFieldsAsHtmlForPlatformApi(platformApiId, false);

                if (htmlFields.Count == 0) return RedirectToAction("PlatformApis");

                PrepareConfigApiConnectionView(model);

                bool hasErrors = false;
                IDictionary<string, string> configValuesToSave = new Dictionary<string, string>();
                IDictionary<string, PlatformApiConfig> apiConfigFields = 
                    _platformApiManager.GetPlatformApiConfigFieldsForApiType(model.PlatformApi.ApiType);

                //Validate
                foreach (var field in htmlFields)
                {
                    string fieldName = field.Key;
                    string fieldValueInForm = formCollection.Get(fieldName);
                    PlatformApiConfig config = apiConfigFields[fieldName];

                    if (config.IsPerPlatformProductParam)
                    {
                        if ((!config.Optional) && string.IsNullOrWhiteSpace(fieldValueInForm))
                        {
                            hasErrors = true;
                            HtmlFormElement htmlFormElement = htmlFields[fieldName];
                            htmlFormElement.AddCssClass("input-validation-error");
                            //Display the error message
                            htmlFormElement.AppendHtml = "<span class='field-validation-error' style='text-align: left;'><span for='" + fieldName + "'>"
                                + config.Name + " is required" + "</span></span>";
                        }
                        else
                        {
                            configValuesToSave.Add(fieldName, fieldValueInForm);
                        }
                    }
                }

                if (hasErrors)
                {
                    return View(htmlFields);
                }

                if (_platformApiManager.SavePlatformPacParamsConfigValues(model.PlatformId, model.Id, configValuesToSave))
                {
                    //Put the message
                    Session.Add("flashMessage", "Platform PAC Params configured successfully!");
                }

                return RedirectToAction("ConfigureApiConnection", new { id = intIdValue }); ;
            }

            return RedirectToAction("PlatformApis"); ;
        }


        [HttpGet]
        public ActionResult ConfigureApi(int id)
        {
            PlatformApiModel model = _platformApiManager.GetPlatformApiById(id);
            if (model.IsNew()) return RedirectToAction("PlatformApis");

            IDictionary<string, HtmlFormElement> htmlFields = _platformApiManager.GetConfigFieldsAsHtmlForPlatformApi(id, true);
            if (htmlFields.Count == 0) return RedirectToAction("PlatformApis");

            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.Title = "Configure API";
            ViewBag.MainPageHeader = "Configure API - " + model.Name + " (#" + id + ")";
            ViewBag.platforApiId = id;

            string flashMessage = Session["flashMessage"] as string;

            if (! string.IsNullOrEmpty(flashMessage))
            {
                ViewBag.Message = flashMessage;
                Session.Remove("flashMessage");
            }

            return View(htmlFields);
        }

        [HttpPost]
        public ActionResult ConfigureApi(FormCollection formCollection)
        {
            string id = formCollection.Get("_platformApiId");
            if (!string.IsNullOrEmpty(id) && Int32.TryParse(id, out int intIdValue))
            {
                PlatformApiModel model = _platformApiManager.GetPlatformApiById(intIdValue);
                if (model.IsNew()) return RedirectToAction("PlatformApis");

                IDictionary<string, HtmlFormElement> htmlFields = _platformApiManager.GetConfigFieldsAsHtmlForPlatformApi(intIdValue, true);
                if (htmlFields.Count == 0) return RedirectToAction("PlatformApis");

                ViewBag.SelectedTab = SelectedAdminTab.Platforms;
                ViewBag.Title = "Configure API";
                ViewBag.MainPageHeader = "Configure API - " + model.Name + " (#" + id + ")";
                ViewBag.platforApiId = id;

                bool hasErrors = false;
                IDictionary<string, string> configValuesToSave = new Dictionary<string, string>();
                IDictionary<string, PlatformApiConfig> apiConfigFields = _platformApiManager.GetPlatformApiConfigFieldsForApiType(model.ApiType);

                foreach (var field in htmlFields)
                {
                    string fieldName = field.Key;
                    string fieldValueInForm = formCollection.Get(fieldName);
                    PlatformApiConfig config = apiConfigFields[fieldName];

                    //Validate fields.
                    //Other complex forms of validation will be included subsequently
                    if ( ! config.IsPerPlatformProductParam)
                    {
                        if ((!config.Optional) && string.IsNullOrWhiteSpace(fieldValueInForm))
                        {
                            hasErrors = true;
                            HtmlFormElement htmlFormElement = htmlFields[fieldName];
                            htmlFormElement.AddCssClass("input-validation-error");
                            htmlFormElement.AppendHtml = "<span class='field-validation-error' style='text-align: left;'><span for='" + fieldName + "' class>"
                                + config.Name + " is required" + "</span></span>";
                        }
                        else
                        {
                            configValuesToSave.Add(fieldName, fieldValueInForm);
                        }
                    }
                }

                if (hasErrors)
                {
                    return View(htmlFields);
                }

                if (_platformApiManager.SavePlatformApiConfigValues(intIdValue, configValuesToSave))
                {
                    //Put the message
                    Session.Add("flashMessage", "Platform API configured successfully!");
                }

                return RedirectToAction("ConfigureApi", new { id = intIdValue }); ;
            }

            return RedirectToAction("PlatformApis"); ;
        }
    }
}