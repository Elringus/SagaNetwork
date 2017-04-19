using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class UpdateBuildController : BaseController
    {
        [ApiRequestData] public string BuildUri;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            foreach (var requestedInstance in await new RequestedInstance().RetrieveAllAsync())
                if (!await requestedInstance.DeleteAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            foreach (var gameServer in await new GameServer().RetrieveAllAsync())
            {
                gameServer.ActiveInstances.Clear();
                gameServer.IsUpdating = true;
                if (!await gameServer.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }
            }

            ServiceBus.SendUpdateBuildTopic(BuildUri);

            return JStatus.Ok;
        }
    }
}

