using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Interfaces
{
    public interface ICMSManager
    {
        /// <summary>
        /// This will be used to get all page list.
        /// </summary>
        /// <returns></returns>
        PagingResult<CMSPageViewModel> GetCMSPageList(PagingModel model);

        /// <summary>
        /// This will be used to add or update page content.
        /// </summary>
        /// <param name="templateModel"></param>
        /// <returns></returns>
        ActionOutput UpdatePageContent(EditCMSPageModel pageContent);

        /// <summary>
        /// Will be used to get page content by page id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        EditCMSPageModel GetPageContentByPageId(int pageId);
        CMSPageViewModel GetPageContentByPageIdforFront(int pageId);
    }
}
