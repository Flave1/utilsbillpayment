using System;
using VendTech.DAL;

namespace VendTech.BLL
{
    public class DbContext : IDisposable
    {
        /// <summary>
        /// protected, so it only visible for inherited class
        /// </summary>
        [ThreadStatic]
        protected static VendtechEntities Context;

        /// <summary>
        /// Initialize Db Context
        /// </summary>
        public DbContext()
        {
            if (Context == null)
            {
                Context = new VendtechEntities();
            }
        }

       
        /// <summary>
        /// Dispose the context
        /// </summary>
        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();

                Context = null;
            }
        }

        /// <summary>
        /// Reinitiate the database context.
        /// </summary>
        public void ReinitiateContext()
        {
            Dispose();
            Context = new VendtechEntities();
        }
    }
}
