using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface IB2bUsersManager
    {
        B2bUserAccessDTO CreateB2bUserAccesskeys(long userId);
        bool IsAccountValid(string clientKey, string apiKey);
        B2bUserAccessDTO UpdateB2bUserAccesskeys(long userId);
    }
}
