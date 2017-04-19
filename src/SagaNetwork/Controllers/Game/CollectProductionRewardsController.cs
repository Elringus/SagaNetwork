using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class CollectProductionRewardsController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string BuildingId;

        [ApiResponseData] public List<Reward> CollectedRewards;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var building = player.GetBuildingById(BuildingId);
            if (building == null) return JStatus.BuildingNotFound;
            var buildingMeta = await building.GetMetaAsync();
            if (buildingMeta == null) return JStatus.MetaNotFound;

            CollectedRewards = new List<Reward>();
            foreach (var reward in building.StoredRewards)
            {
                await reward.AwardPlayerAsync(player);
                CollectedRewards.Add(reward);
            }

            building.StoredRewards = new List<Reward>();

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
