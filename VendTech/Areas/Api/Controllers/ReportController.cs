using System;
using System.Globalization;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class ReportController : BaseAPIController
    {
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        public ReportController(IErrorLogManager errorLogManager, IDepositManager depositManager, IMeterManager meterManager)
            : base(errorLogManager)
        {
            _depositManager = depositManager;
            _meterManager = meterManager;
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetSalesReport(ReportSearchModel model)
        {
            model.SortBy = "CreatedAt";
            model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserId;
            model.PageNo = ((model.PageNo - 1) * model.RecordsPerPage) + 1;

            //CultureInfo provider = CultureInfo.InvariantCulture;

            //if (model.From != null)
            //{
            //    model.From = DateTime.ParseExact(model.From.Value.Date.ToString(), "dd/MM/yyyy", provider);
            //}

            //if (model.To != null)
            //{
            //    model.To = DateTime.ParseExact(model.To.Value.ToString(), "dd/MM/yyyy", provider);
            //}

            var deposits = new PagingResult<MeterRechargeApiListingModelMobile>();
            deposits = _meterManager.GetUserMeterRechargesReportMobileAsync(model);
            return new JsonContent(deposits.TotalCount, deposits.Message, Status.Success, deposits.List).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetDepositReport(ReportSearchModel model)
        {
            model.SortBy = "CreatedAt";
            model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserId;
            model.PageNo = ((model.PageNo - 1) * model.RecordsPerPage) + 1;

            //CultureInfo provider = CultureInfo.InvariantCulture;
            //if (model.From != null)
            //{
            //    model.From = DateTime.ParseExact(model.From.Value.Date.ToString(), "dd/MM/yyyy", provider);
            //}

            //if (model.To != null)
            //{
            //    model.To = DateTime.ParseExact(model.To.Value.ToString(), "dd/MM/yyyy", provider);
            //}

            var deposits = new PagingResult<DepositListingModelMobile>();
            deposits = _depositManager.GetReportsMobilePagedList(model);
            return new JsonContent(deposits.TotalCount, deposits.Message, Status.Success, deposits.List).ConvertToHttpResponseOK();
        }
    }
}
