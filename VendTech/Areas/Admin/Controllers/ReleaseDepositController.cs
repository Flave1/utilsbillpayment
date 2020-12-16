using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Common;
using System.Web.Configuration;

namespace VendTech.Areas.Admin.Controllers
{
    public class ReleaseDepositController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IDepositManager _depositManager;
        #endregion

        public ReleaseDepositController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IDepositManager depositManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _depositManager = depositManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageDepositRelease()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            ViewBag.Balance = _depositManager.GetPendingDepositTotal();
            var deposits = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"),true);
            return View(deposits);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetDepositReleasePagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var modal = _depositManager.GetDepositPagedList(model,true);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_depositReleaseListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult ApproveReleaseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Released,LOGGEDIN_USER.UserID));
        }
        [AjaxOnly, HttpPost]
        public JsonResult RejectReleaseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Rejected,LOGGEDIN_USER.UserID));
        }
        [AjaxOnly, HttpPost]
        public JsonResult SendOTP()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.SendOTP();
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositOTP);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%otp%", result.Object);
                Utilities.SendEmail(WebConfigurationManager.AppSettings["AdminEmail"].ToString(), emailTemplate.EmailSubject, body);
            }
            return JsonResult(new ActionOutput { Message=result.Message,Status=result.Status});
        }
        [AjaxOnly, HttpPost]
        public JsonResult ChangeDepositStatus(ReleaseDepositModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.ChangeMultipleDepositStatus(model,LOGGEDIN_USER.UserID);
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        #endregion
    }
}