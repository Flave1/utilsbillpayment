using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;

namespace VendTech.BLL.Managers
{
    public class ContactUsManager : BaseManager, IContactUsManager
    {

        ActionOutput IContactUsManager.SaveContactUsRequest(ContactUsModel model)
        {
            var dbContactUs = new ContactU();
            dbContactUs.CreatedAt = DateTime.UtcNow;
            dbContactUs.Subject = model.Subject;
            dbContactUs.Message = model.Message;
            dbContactUs.IsDeleted = false;
            dbContactUs.Status = 0;
            dbContactUs.UserId = model.UserId;
            Context.ContactUs.Add(dbContactUs);
            Context.SaveChanges();
            return ReturnSuccess("Your request sent successfully.");
        }
    }


}
