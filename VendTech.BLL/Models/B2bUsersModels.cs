using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class B2bUserAccessDTO
    {
        public long B2bUserAccessId { get; set; }
        public long UserId { get; set; }
        public string ClientToken { get; set; }
        public string APIToken { get; set; }
        public string Clientkey { get; set; }
        public string APIKey { get; set; }
        public string CreatedAt { get; set; }
        public B2bUserAccessDTO()
        {
            
        }
        public B2bUserAccessDTO(B2bUserAccess db)
        {
            B2bUserAccessId = db.B2bUserAccessId;
            UserId = db.UserId;
            ClientToken = db.ClientToken;
            APIToken = db.APIToken;
            Clientkey = db.Clientkey;
            APIKey = db.APIKey;
            CreatedAt = db.CreatedAt.ToString("MM/dd/yyy");

        }
    }

  
    public class UpdateB2bUserAccessKeys
    {
        public long UserId { get; set; }
        public string Clientkey { get; set; }
        public string APIKey { get; set; }
    }
}
