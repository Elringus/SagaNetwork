using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class StartProductionController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string BuildingId;
        [ApiRequestData] public string ContractMetaId;

        [ApiResponseData] public ProductionTask StartedProductionTask;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var building = player.GetBuildingById(BuildingId);
            if (building == null) return JStatus.BuildingNotFound;

            var buildingMeta = await building.GetMetaAsync();
            if (buildingMeta == null) return JStatus.BuildingNotFound;
            var contractMeta = await new ContractMeta(ContractMetaId).LoadAsync();
            if (contractMeta == null) return JStatus.ContractNotFound;

            if (!building.IsConstructed) return JStatus.NotReady;
            if (!buildingMeta.AvailableContractMetaIds.Contains(ContractMetaId)) return JStatus.ContractNotFound;
            if (!contractMeta.Requirement.IsFulfilledByPlayer(player)) return JStatus.RequirementNotFulfilled;

            // Shouldn't produce items player already have.
            if (contractMeta.Rewards?.Count > 0 && contractMeta.Rewards.Exists(reward => reward.ItemMetaIds?.Count > 0))
            {
                var currentlyProducedItemMetaIds = new List<string>();
                foreach (var producedContractMetaId in player.GetCurrentlyProducedContractMetaIds())
                {
                    var producedContractMeta = await new ContractMeta(producedContractMetaId).LoadAsync();
                    foreach (var reward in producedContractMeta.Rewards)
                        currentlyProducedItemMetaIds.AddRange(reward.ItemMetaIds);
                }

                foreach (var reward in contractMeta.Rewards)
                    if (reward?.ItemMetaIds != null)
                        foreach (var itemMetaId in reward.ItemMetaIds)
                            if (player.HasItemOfMeta(itemMetaId) || currentlyProducedItemMetaIds.Contains(itemMetaId))
                                return JStatus.ItemDuplication;
            }

            building.ProductionTask = new ProductionTask(contractMeta);

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            StartedProductionTask = building.ProductionTask;

            return JStatus.Ok;
        }
    }
}
