using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using VendTech.Areas.Admin.Controllers;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;

namespace VendTech.Controllers
{
    public class TopupController : AppUserBaseController
    {
        public IMeterManager _meterManager;

        public TopupController(IErrorLogManager errorLogManager, IMeterManager meterManager) 
            : base (errorLogManager)
        {
            _meterManager = meterManager;
        }

        // GET: Topup
        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Meters;
            var meters = _meterManager.GetMeters(LOGGEDIN_USER.UserID, 1, 1000000000, true);
            return View(meters);
        }
    }
}