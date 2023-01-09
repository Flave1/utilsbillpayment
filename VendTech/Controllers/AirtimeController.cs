using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.Controllers
{
    public class AirtimeController: AppUserBaseController
    {
        private IPOSManager _posManager;
        private IPlatformTransactionManager _platformTransactionManager;

        public AirtimeController(
            IErrorLogManager errorLogManager,
            IPOSManager posManager,
            IPlatformTransactionManager platformTransactionManager) 
            : base(errorLogManager)
        {
            _posManager = posManager;
            _platformTransactionManager = platformTransactionManager;
        }

        public ActionResult Recharge()
        {
            //list of airtime products
            List<SelectListItem> productsSelectList = new List<SelectListItem>()
            {
                new SelectListItem { Value = "", Text = "Select Operator" }
            };

            var airtimeProducts = _platformManager.GetPlatformsByTypeForRecharge(PlatformTypeEnum.AIRTIME);
            if (airtimeProducts != null && airtimeProducts.Count > 0)
            {
                foreach(var product in airtimeProducts)
                {
                    productsSelectList.Add(new SelectListItem { Value = product.PlatformId.ToString(), Text = product.Title });
                }
            }

            ViewBag.PlatformList = productsSelectList;

            ViewBag.SelectedTab = SelectedAdminTab.BillPayment;
            PlatformTransactionModel model = new PlatformTransactionModel();
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;

            ViewBag.IsPlatformAssigned = airtimeProducts.Count > 0;

           JavaScriptSerializer js = new JavaScriptSerializer();
            var hostory_model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                PageNo = 1,
                VendorId = LOGGEDIN_USER.UserID
            };

            var deposits = _platformTransactionManager.GetUserAirtimeRechargeTransactionDetailsHistory(hostory_model);

            if (deposits.List.Count > 0)
            {
                model.History = deposits.List;
            }  
            
            if (posList.Count > 0)
                ViewBag.walletBalance = _posManager.GetPosBalance(Convert.ToInt64(posList[0].Value));
            else
                ViewBag.walletBalance = 0;

            return View(model);
        }

        [HttpPost, AjaxOnly]
        public JsonResult Recharge(PlatformTransactionModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;

            //Fetch the currency
            model.Currency = "SLE";

            return JsonResult(_platformTransactionManager.RechargeAirtime(model));
        }
    }
}