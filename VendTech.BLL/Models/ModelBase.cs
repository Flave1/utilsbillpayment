using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public interface ModelBase
    {
        /**
        * Is this model for a non-persisted entity?
        */
        bool IsNew();

        bool IsNotNew();
    }

    public abstract class AbstractModelBase : ModelBase
    {
        public abstract bool IsNew();
       public abstract bool IsNotNew();
    }


    public class IntIdentifierModelBase : AbstractModelBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override bool IsNew()
        {
            return Id <= 0;
        }

        public override bool IsNotNew()
        {
            return !IsNew();
        }
    }

    public class LongIdentifierModelBase : AbstractModelBase
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override bool IsNew()
        {
            return Id <= 0;
        }

        public override bool IsNotNew()
        {
            return !IsNew();
        }
    }
}
