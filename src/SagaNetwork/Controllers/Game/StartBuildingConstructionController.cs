using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class StartBuildingConstructionController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string BuildingSpotId;
        [ApiRequestData] public string BuildingMetaId;

        [ApiResponseData] public Building NewBuilding;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var buildingSpot = player.GetBuildingSpotById(BuildingSpotId);
            if (buildingSpot == null) return JStatus.BuildingSpotNotFound;
            var buildingSpotMeta = await buildingSpot.GetMetaAsync();
            if (buildingSpotMeta == null) return JStatus.BuildingSpotNotFound;
            var buildingMeta = await new BuildingMeta(BuildingMetaId).LoadAsync();
            if (buildingMeta == null) return JStatus.BuildingNotFound;

            if (buildingMeta.Size != buildingSpotMeta.Size)
                return JStatus.TypeMismatch;
            if (buildingSpot.IsOccupied)
                return JStatus.BuildingSpotOccupied;
            if (!buildingMeta.ConstructionRequirement.IsFulfilledByPlayer(player))
                return JStatus.RequirementNotFulfilled;

            var building = new Building(buildingMeta.Id);
            building.IsConstructed = false;
            building.ConstructionTask = new TimeTask(buildingMeta.ConstructionTime);

            buildingSpot.Building = building;

            player.SpendResources(buildingMeta.ConstructionRequirement.Resources);

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            NewBuilding = building;

            return JStatus.Ok;
        }
    }
}
