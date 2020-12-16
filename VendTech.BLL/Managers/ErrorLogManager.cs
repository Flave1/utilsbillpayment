using VendTech.BLL.Interfaces;
using VendTech.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Managers
{
    public class ErrorLogManager : BaseManager, IErrorLogManager
    {
        string IErrorLogManager.LogExceptionToDatabase(Exception exc)
        {
            var context = new VendTechEntities();
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = exc.Message;
            errorObj.StackTrace = exc.StackTrace;
            errorObj.InnerException = exc.InnerException == null ? "" : exc.InnerException.Message;
            errorObj.LoggedInDetails = "";
            errorObj.LoggedAt = DateTime.UtcNow;
            context.ErrorLogs.Add(errorObj);
            // To do
            context.SaveChanges();
            return errorObj.ErrorLogID.ToString();
        }
    }
}
