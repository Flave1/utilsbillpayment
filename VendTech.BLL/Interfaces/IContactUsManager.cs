using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Interfaces
{
    public interface IContactUsManager
    {
        ActionOutput SaveContactUsRequest(ContactUsModel model);
    }
    
}
