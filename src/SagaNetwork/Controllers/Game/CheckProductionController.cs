using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class CheckProductionController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string BuildingId;

        [ApiResponseData] public ProductionTask UpdatedProductionTask;
        [ApiResponseData] public List<Reward> ProducedRewards;
        [ApiResponseData] public List<Reward> UpdatedBuildingStorage;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var building = player.GetBuildingById(BuildingId);
            if (building == null) return JStatus.BuildingNotFound;

            if (building.ProductionTask == null)
                return JStatus.Fail;

            if (string.IsNullOrEmpty(building.ProductionTask.ProducedContractMetaId))
                return JStatus.ContractNotFound;

            var contractMeta = await new ContractMeta(building.ProductionTask.ProducedContractMetaId).LoadAsync();
            if (contractMeta == null) return JStatus.ContractNotFound;

            var buildingMeta = await building.GetMetaAsync();
            if (buildingMeta == null) return JStatus.MetaNotFound;

            foreach (var assignedCitizen in building.GetAssignedCitizens(player))
                foreach (var citizenAbility in assignedCitizen.Abilities)
                    await citizenAbility.ModifyProductionTaskAsync(building.ProductionTask);
            var prodCyclesSinceLastCheck = building.ProductionTask.Check();
            if (prodCyclesSinceLastCheck == 0)
            {
                UpdatedProductionTask = building.ProductionTask;
                return JStatus.NotReady;
            }

            ProducedRewards = new List<Reward>();
            for (int i = 0; i < prodCyclesSinceLastCheck; i++)
            {
                var rewardList = contractMeta.Rewards.Select(r => r.Clone()).ToList();
                foreach (var reward in rewardList)
                {
                    foreach (var assignedCitizen in building.GetAssignedCitizens(player))
                        foreach (var citizenAbility in assignedCitizen.Abilities)
                            await citizenAbility.ModifyRewardAsync(reward);
                    if (reward.RollTheDice())
                    {
                        if (building.CalculateAvailableStorage(buildingMeta.StorageLimit) < reward.CalculateVolume())
                            continue;
                        else
                        {
                            building.StoredRewards.Add(reward);
                            ProducedRewards.Add(reward);
                        }
                    }
                }
            }

            if (!contractMeta.IsLoopable) building.ProductionTask = null;
            UpdatedBuildingStorage = building.StoredRewards;
            UpdatedProductionTask = building.ProductionTask;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
