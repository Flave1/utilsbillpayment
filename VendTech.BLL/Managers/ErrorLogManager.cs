using VendTech.BLL.Interfaces;
using VendTech.DAL;
using System;

namespace VendTech.BLL.Managers
{
    public class ErrorLogManager : BaseManager, IErrorLogManager
    {
        string IErrorLogManager.LogExceptionToDatabase(Exception exc)
        {
            var context = new VendtechEntities();
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
