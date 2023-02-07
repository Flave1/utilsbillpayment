using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

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

        public ActionResult Recharge(string provider = "")
        {
            //list of airtime products
            List<SelectListItem> productsSelectList = new List<SelectListItem>();

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
            if(!model.Beneficiary.StartsWith("234") && !model.Beneficiary.StartsWith("+234"))
            {
                model.Beneficiary = "234" + model.Beneficiary;
            }
            model.Currency = "SLE";

            var result = _platformTransactionManager.RechargeAirtime(model);
            if (result.ReceiptStatus.Status == "unsuccessful")
            {
                return Json(JsonConvert.SerializeObject(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message }));
            }
            if (result.ReceiptStatus.Status == "pending")
            {
                return Json(JsonConvert.SerializeObject(new { Success = false, Code = 300, Msg = result.ReceiptStatus.Message }));
            }
            if (result != null)
                return Json(JsonConvert.SerializeObject(new { Success = true, Code = 200, Msg = "Airtime recharged successfully.", Data = result }));
            return Json(JsonConvert.SerializeObject(new { Success = false, Code = 302, Msg = "Airtime recharged not successful.", Data = result }));
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult GetAirtimeReceipt(RequestObject1 requestObject)
        {
            var result = _platformTransactionManager.GetAirtimeReceipt(requestObject.Id);
            if (result.ReceiptStatus.Status == "unsuccessful")
                return Json(JsonConvert.SerializeObject(new { Success = false, Code = 302, Msg = "Airtime recharged not successful.", Data = result }));
            return Json(JsonConvert.SerializeObject(new { Success = true, Code = 200, Msg = "Airtime recharged successfully.", Data = result }));
        }
    }
}