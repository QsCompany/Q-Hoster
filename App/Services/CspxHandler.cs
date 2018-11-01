using System.Web;

namespace IIS
{
    public class CspxHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {

        }
    }
}