using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class CheckAuthController : BaseController
    {
        protected override async Task<JToken> ExecuteController (JToken data)
        {
            // Auth checking is performed in the base class when the appropriate attribute is set.
            return await Task.FromResult(JStatus.Ok);
        }
    }
}
