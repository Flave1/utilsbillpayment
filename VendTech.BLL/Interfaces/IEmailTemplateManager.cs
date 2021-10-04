using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Interfaces
{
    public interface IEmailTemplateManager
    {
        /// <summary>
        /// This will be used to get all template list.
        /// </summary>
        /// <returns></returns>
        PagingResult<TemplateViewModel> GetEmailTemplateList(PagingModel model);

        /// <summary>
        /// This will be used to add or update email template
        /// </summary>
        /// <param name="templateModel"></param>
        /// <returns></returns>
        ActionOutput AddUpdateEmailTemplate(AddEditEmailTemplateModel templateModel);

        /// <summary>
        /// Will be used to get template by template id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        AddEditEmailTemplateModel GetEmailTemplateByTemplateId(int templateId);
        TemplateViewModel GetEmailTemplateByTemplateType(TemplateTypes type);
    }
}
