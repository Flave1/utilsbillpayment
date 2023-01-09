using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;

namespace VendTech.BLL.Managers
{
    public abstract class BaseManager : DbContext
    {
        public BaseManager()
            : base()
        {

        }
        /// <summary>
        /// protected, it only visible for inherited class
        /// </summary>
        protected void SaveChanges()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
        }
        protected ActionOutput ReturnSuccess(string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Successfull, Message = msg };
        }

        protected ActionOutput ReturnSuccess(long id, string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Successfull, Message = msg, ID = id };
        }

        protected ActionOutput ReturnError(long id, string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Error, Message = msg, ID = id };
        }
        protected ActionOutput ReturnError(string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Error, Message = msg };
        }

        protected ActionOutput ReturnPending(long id, string msg = "")
        {
            return new ActionOutput { ID = id, Status = ActionStatus.Pending, Message = msg };
        }

        protected ActionOutput<T> ReturnSuccess<T>(T model, string msg = "")
        {
            return new ActionOutput<T> { Status = ActionStatus.Successfull, Message = msg, Object = model };
        }

        protected ActionOutput<T> ReturnError<T>(string msg = "")
        {
            return new ActionOutput<T> { Status = ActionStatus.Error, Message = msg };
        } 
    }


}
