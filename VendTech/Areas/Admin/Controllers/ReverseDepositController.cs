using System.Collections.Generic;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class ReverseDepositController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IDepositManager _depositManager;
        #endregion

        public ReverseDepositController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IDepositManager depositManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _depositManager = depositManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageDepositReverse()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            ViewBag.Balance = _depositManager.GetPendingDepositTotal();
            var deposits = _depositManager.GetReleasedDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true);
            return View(deposits);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetDepositReversePagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var modal = _depositManager.GetReleasedDepositPagedList(model, true);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_depositReverseListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult ApproveReverseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ReverseDepositStatus(depositId, DepositPaymentStatusEnum.Reversed, LOGGEDIN_USER.UserID));
        }
        [AjaxOnly, HttpPost]
        public JsonResult RejectReverseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Rejected));
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
                body = body.Replace("Approve", "Reverse");
                body = body.Replace("%USER%", LOGGEDIN_USER.FirstName); 

                Utilities.SendEmail(User.Identity.Name, emailTemplate.EmailSubject, body);
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        [AjaxOnly, HttpPost]
        public JsonResult ChangeDepositStatus(ReverseDepositModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.ChangeMultipleDepositStatusOnReverse(model, LOGGEDIN_USER.UserID);
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        #endregion
    }
}