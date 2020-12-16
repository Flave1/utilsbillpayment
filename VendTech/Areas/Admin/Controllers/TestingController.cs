using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VendTech.Areas.Admin.Controllers
{
    public class TestingController : Controller
    {
        // GET: Admin/Testing
        public ActionResult Index()
        {
            return View();
        }


        public void WriteDataToExcel()
        {
            DataTable dt = getData();
            string fileName = "Sample.xlsx";
            using (XLWorkbook wb = new XLWorkbook(Server.MapPath(@"~/Content/StaticFileFormat/TempSalesReport.xlsx")))
            {
                wb.Worksheets.Add(dt);
                var ws = wb.Worksheets.FirstOrDefault();
                ws.Cell(6,1).Value = "From Query zsdfnbkjshdf sfjsdf sfhsfd sdfhsfdsdfhsfsf sfhsf sfhsf sfdhsf shs ";
                wb.SaveAs(Server.MapPath(@"~/Content/StaticFileFormat/TempSalesReport235.xlsx"));
                //wb.Save();
            }
        }


        [NonAction]
        public DataTable getData()
        {
            //Creating DataTable  
            DataTable dt = new DataTable();
            //Setiing Table Name  
            dt.TableName = "EmployeeData";
            //Add Columns  
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("City", typeof(string));
            //Add Rows in DataTable  
            dt.Rows.Add(1, "Anoop Kumar Sharma", "Delhi");
            dt.Rows.Add(2, "Andrew", "U.P.");
            dt.AcceptChanges();
            return dt;
        }
    }
}