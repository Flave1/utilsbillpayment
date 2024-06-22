using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface IB2bUsersManager
    {
        B2bUserAccessDTO CreateB2bUserAccesskeys(long userId);
        B2bUserAccessDTO UpdateB2bUserAccesskeys(long userId);
    }
}
