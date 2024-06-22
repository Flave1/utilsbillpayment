using Microsoft.Owin;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Vendtech.Test
{
    public class LogMiddleware : OwinMiddleware
    {
        public LogMiddleware(OwinMiddleware next) : base(next)
        {

        }

        public async override Task Invoke(IOwinContext context)
        {
            Debug.WriteLine("Request begins: {0} {1}", context.Request.Method, context.Request.Uri);
            await Next.Invoke(context);
            Debug.WriteLine("Request ends : {0} {1}", context.Request.Method, context.Request.Uri);
        }
    }
}