using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;

namespace VendTech.BLL.Managers
{
    public class CMSManager : BaseManager, ICMSManager
    {

        PagingResult<CMSPageViewModel> ICMSManager.GetCMSPageList(Models.PagingModel model)
        {
            var result = new PagingResult<CMSPageViewModel>();
            var query = Context.CMSPages.OrderBy(model.SortBy + " " + model.SortOrder);
            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(z => z.PageName.Contains(model.Search) || z.PageTitle.Contains(model.Search));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new CMSPageViewModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Page List";
            result.TotalCount = query.Count();
            return result;
        }

        ActionOutput ICMSManager.UpdatePageContent(EditCMSPageModel pageContent)
        {
            var existingPage = Context.CMSPages.FirstOrDefault(z => z.PageId == pageContent.PageId);
            if (existingPage == null)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Page not exists."
                };
            }
            else
            {
                existingPage.PageName = pageContent.PageName;
                existingPage.PageTitle = pageContent.PageTitle;
                existingPage.PageContent = pageContent.PageContent;
                existingPage.MetaTitle = pageContent.MetaTitle;
                existingPage.MetaKeywords = pageContent.MetaKeywords;
                existingPage.MetaDescription = pageContent.MetaDescription;
                existingPage.UpdatedOn = DateTime.UtcNow;
                Context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "CMS Page Updated Sucessfully."
                };
            }
           
        }

        EditCMSPageModel ICMSManager.GetPageContentByPageId(int pageId)
        {
            var existingPage = Context.CMSPages.FirstOrDefault(z => z.PageId == pageId);
            if (existingPage != null)
                return new EditCMSPageModel(existingPage);
            else
                return null;
        }
        CMSPageViewModel ICMSManager.GetPageContentByPageIdforFront(int pageId)
        {
            var existingPage = Context.CMSPages.FirstOrDefault(z => z.PageId == pageId);
            if (existingPage != null)
                return new CMSPageViewModel(existingPage);
            else
                return null;
        }
    }
}
