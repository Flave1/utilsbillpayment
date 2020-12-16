using System;
using System.Collections.Generic;
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
            Context.SaveChanges();
        }
        protected ActionOutput ReturnSuccess(string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Successfull, Message = msg };
        }

        protected ActionOutput ReturnSuccess(long id, string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Successfull, Message = msg, ID = id };
        }

        protected ActionOutput ReturnError(string msg = "")
        {
            return new ActionOutput { Status = ActionStatus.Error, Message = msg };
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
