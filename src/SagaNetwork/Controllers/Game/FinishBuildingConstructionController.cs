using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class FinishBuildingConstructionController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string BuildingId;

        [ApiResponseData] public TimeTask UpdatedConstructionTask;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var building = player.GetBuildingById(BuildingId);
            if (building == null) return JStatus.BuildingNotFound;
            var buildingMeta = await building.GetMetaAsync();
            if (buildingMeta == null) return JStatus.BuildingNotFound;

            if (building.ConstructionTask == null)
                return JStatus.Fail;

            if (building.ConstructionTask.Check() == 0)
            {
                UpdatedConstructionTask = building.ConstructionTask;
                return JStatus.NotReady;
            }

            building.IsConstructed = true;
            building.ConstructionTask = null;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
