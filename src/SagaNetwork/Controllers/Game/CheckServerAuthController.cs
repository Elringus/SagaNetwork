using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class CheckServerAuthController : BaseController
    {
        protected override async Task<JToken> ExecuteController (JToken data)
        {
            return await Task.FromResult(JStatus.Ok);
        }
    }
}
