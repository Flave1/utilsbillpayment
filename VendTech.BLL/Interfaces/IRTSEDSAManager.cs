using System.Threading.Tasks;
using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface IRTSEDSAManager
    {
        Task<PagingResult<RtsedsaTransaction>> GetTransactionsAsync(UnixDateRequest model);
    }
}