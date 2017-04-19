using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class OnInstanceReadyController : BaseController
    {
        [ApiRequestData] public string Ip;
        [ApiRequestData] public string Port;
        [ApiRequestData] public string ArenaMetaId;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var requestedInstance = await new RequestedInstance(ArenaMetaId).LoadAsync();
            if (requestedInstance == null) return JStatus.RequestedInstanceNotFound;
            if (!await requestedInstance.DeleteAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            var arenaInstance = new ArenaInstance(ArenaMetaId, Port);
            arenaInstance.IsOpen = true;
            var gameServer = await new GameServer(Ip).LoadAsync();
            if (gameServer == null) return JStatus.NotFound;
            gameServer.ActiveInstances.Add(arenaInstance);
            if (!await gameServer.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
