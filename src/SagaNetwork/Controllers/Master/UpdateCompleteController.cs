using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class UpdateCompleteController : BaseController
    {
        [ApiRequestData] public string Ip;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var gameServer = await new GameServer(Ip).LoadAsync();
            gameServer.IsUpdating = false;

            if (!await gameServer.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}

