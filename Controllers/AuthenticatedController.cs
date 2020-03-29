using System.Web.Http;

namespace AwsDemo.Controllers
{
    [Authorize]
    public class AuthenticatedController : ApiController
    {
        // GET: api/Authenticated
        public bool Post()
        {
            return true;
        }
    }
}
