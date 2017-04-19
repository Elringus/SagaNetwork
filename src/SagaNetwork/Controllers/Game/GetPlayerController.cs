using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class GetPlayerController : BaseController
    {
        [ApiRequestData] public string PlayerId;

        [ApiResponseData] public Player Player;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            Player = await new Player(PlayerId).LoadAsync();
            if (Player == null) return JStatus.PlayerNotFound;

            return JStatus.Ok;
        }
    }
}
