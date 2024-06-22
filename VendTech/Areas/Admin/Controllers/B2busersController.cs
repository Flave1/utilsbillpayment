using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;

namespace VendTech.Areas.Admin.Controllers
{
    public class B2busersController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IAgencyManager _agentManager;
        private readonly IVendorManager _vendorManager;
        private readonly IPOSManager _posManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IB2bUsersManager _b2bUsersManager;
        #endregion


        public B2busersController(IUserManager userManager,
            IAuthenticateManager authenticateManager,
            IErrorLogManager errorLogManager,
            IEmailTemplateManager templateManager,
            IAgencyManager agentManager,
            IVendorManager vendorManager,
            IPOSManager posManager,
            IB2bUsersManager b2bUsersManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _agentManager = agentManager;
            _vendorManager = vendorManager;
            _posManager = posManager;
            _authenticateManager = authenticateManager;
            _b2bUsersManager = b2bUsersManager;
        }


        #region Variable Declaration


        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var users = _userManager.GetB2BUserPagedList(PagingModel.DefaultModel("Name", "Asc"), "");
            return View(users);
        }

        [HttpGet]
        public ActionResult account(long? id = 0)
        {

            var model = new AddUserModel();
            var countryDrpData = new List<SelectListItem>();
            var countries = _authenticateManager.GetCountries();
            var cities = _authenticateManager.GetCities();
            var cityDrpData = new List<SelectListItem>();
            if (id > 0)
            {
                ViewBag.Title = "Edit B2B User";
                model = _userManager.GetB2bUserDetailsByUserId(id.Value);
                model.ModuleList = _userManager.GetAllModules(id.Value);
                model.PlatformList = _userManager.GetAllPlatforms(id.Value);
                model.WidgetList = _userManager.GetAllWidgets(id.Value);
                foreach (var item in countries)
                {
                    var selected = model.CountryId == item.CountryId;
                    countryDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CountryId.ToString(), Selected = selected });
                }
                foreach (var item in cities)
                {
                    var selected = model.City == item.CityId;
                    cityDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CityId.ToString(), Selected = selected });
                }
            }
            else
            {
                ViewBag.Title = "Add B2B User"; 
                model.PlatformList = _userManager.GetAllPlatforms(0);
                model.ResetUserPassword = true;
                model.ModuleList = _userManager.GetAllModules(0);
                model.WidgetList = _userManager.GetAllWidgets(0);
                foreach (var item in countries)
                {
                    countryDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CountryId.ToString() });
                }
                foreach (var item in cities)
                {
                    cityDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CityId.ToString() });
                }
            }
            
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.Agencies = _agentManager.GetAgentsSelectList();
            ViewBag.Pos = _posManager.GetPOSSelectList();
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            ViewBag.Cities = cityDrpData;
            ViewBag.countries = countryDrpData;

            return View(model);
        }



        [AjaxOnly, HttpPost]
        public JsonResult account(AddUserModel model)
        {
            try
            {
                ViewBag.SelectedTab = SelectedAdminTab.Users;
                ActionOutput result= null;
                if (model.ImagefromWeb != null)
                {
                    var file = model.ImagefromWeb;
                    var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    model.Image = (HttpPostedFile)constructorInfo
                               .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
                }

                model.UserType = (int)AppUserTypeEnum.B2B;
                model.AgentId = _userManager.GetVendtechAgencyId();

                if(model.UserId == 0)
                    result = _userManager.AddAppUserDetails(model);
                else
                    result = _userManager.UpdateAppUserDetails(model);

                if (result.Status == ActionStatus.Successfull)
                {
                    if (model.UserId == 0)
                        _b2bUsersManager.CreateB2bUserAccesskeys(result.ID);
                }
                return JsonResult(result);
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion
    }
}