using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class AddResourcesController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public List<int> Resources;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            player.AddResources(Resources);

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
