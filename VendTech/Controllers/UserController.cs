#region Default Namespaces
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System.Reflection;
using VendTech.Areas.Admin.Controllers;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class UserController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly ICMSManager _cmsManager;


        #endregion

        public UserController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;

        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult EditProfile()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;

            var userDetails = _userManager.GetAppUserProfile(LOGGEDIN_USER.UserID);
            var countries = _authenticateManager.GetCountries();
            var countryDrpData = new List<SelectListItem>();

            foreach (var item in countries)
            {
                countryDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CountryId.ToString() });
            }
            ViewBag.countries = countryDrpData;
            ViewBag.Cities = _authenticateManager.GetCities();
            return View(userDetails);
        }
        [AjaxOnly, HttpPost]
        public ActionResult EditProfile(UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;

                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo
                           .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            } 
            return JsonResult(_userManager.UpdateUserProfile(LOGGEDIN_USER.UserID, model));
        }
    }
}