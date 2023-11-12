using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VendTech.Areas.Api.Controllers
{
    public class PdfController: Controller
    {

        public ActionResult DownloadPdf()
        {
            // Replace "your_pdf_file.pdf" with the actual file name or path
            //var pdfFilePath = Server.MapPath("~/App_Data/your_pdf_file.pdf");

            var pdfFilePath = "C:\\\\Users\\\\thispc\\\\Desktop\\\\FLAVETECH\\\\Vendtech\\\\vendtech-web\\\\VendTech\\/Receipts/30_receipt.pdf";

            // Check if the file exists
            if (!System.IO.File.Exists(pdfFilePath))
            {
                return HttpNotFound(); // Handle the case where the file is not found
            }

            // Set the content type
            var contentType = "application/pdf";

            // Return the file for download
            return File(pdfFilePath, contentType, "Your_PDF_File_Name.pdf");
        }
    }
}