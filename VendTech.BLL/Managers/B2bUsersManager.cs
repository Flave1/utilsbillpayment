using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Linq;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Text;

namespace VendTech.BLL.Managers
{
    public class B2bUsersManager : BaseManager, IB2bUsersManager
    {
        B2bUserAccessDTO IB2bUsersManager.CreateB2bUserAccesskeys(long userId)
        {
            var dbObject = Context.B2bUserAccess.FirstOrDefault(d => d.UserId == userId);
            if(dbObject != null)
            {
                throw new ArgumentException("User Keys already exist");
            }

            var apiToken =  Guid.NewGuid().ToString().Replace('-', ' ').Replace(" ", "");
            var clientToken = Guid.NewGuid().ToString().Replace('-', ' ').Replace(" ", "");
            var initializationVector1 = returnIv(apiToken);
            var initializationVector2 = returnIv(apiToken);

            dbObject = new B2bUserAccess
            {
                UserId = userId,
                APIToken = apiToken,
                ClientToken = clientToken,
                APIKey = AesEncryption.Encrypt(Utilities.GenerateByAnyLength(8), apiToken, initializationVector1),
                Clientkey = AesEncryption.Encrypt(Utilities.GenerateByAnyLength(8), apiToken, initializationVector2),
                CreatedAt = DateTime.UtcNow
            };

            Context.B2bUserAccess.Add(dbObject);
            Context.SaveChanges();
            return new B2bUserAccessDTO(dbObject);
        }

        B2bUserAccessDTO IB2bUsersManager.UpdateB2bUserAccesskeys(long userId)
        {
            var dbObject = Context.B2bUserAccess.FirstOrDefault(d => d.UserId == userId);
            if (dbObject == null)
            {
                throw new ArgumentException("User Keys already does not exist");
            }

            var apiToken = Guid.NewGuid().ToString();

            dbObject.UserId = userId;
            dbObject.APIToken = apiToken;
            dbObject.APIKey = AesEncryption.Encrypt(Utilities.GenerateByAnyLength(8), apiToken, returnIv(apiToken));
            dbObject.Clientkey = AesEncryption.Encrypt(Utilities.GenerateByAnyLength(8), apiToken, returnIv(apiToken));

            Context.SaveChanges();
            return new B2bUserAccessDTO(dbObject);
        }

        private string returnIv(string token)
        { 
            return token.Substring(16);
        }
    } 
}
