using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class RegisterGameServerController : BaseController
    {
        [ApiRequestData] public string Ip;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            await new GameServer(Ip).InsertAsync();

            return JStatus.Ok;
        }
    }
}
