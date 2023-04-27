using VendTech.BLL.Models;
namespace VendTech.BLL.Interfaces
{
    public interface ITransferManager
    {
        PagingResult<AgentListingModel> GetAllAgencyAdminVendors(PagingModel model, long agency, long userId);
        PagingResult<AgentListingModel> GetOtherVendors(PagingModel model, long agency);
    }

}
