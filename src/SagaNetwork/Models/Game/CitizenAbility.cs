using System;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class CitizenAbility : MetaDescribedModel<CitizenAbilityMeta>
    {
        public int Level { get; set; }
        public int Experience { get; set; }

        public CitizenAbility () { }
        public CitizenAbility (string metaId) : base (metaId) { }

        public async Task ModifyRewardAsync (Reward reward)
        {
            var abilityMeta = await GetMetaAsync();

            if (abilityMeta.IncreaseRewardProbability.Count > Level)
                reward.Probability = Math.Min(1f, reward.Probability + abilityMeta.IncreaseRewardProbability[Level]);
        }

        public async Task ModifyProductionTaskAsync (ProductionTask productionTask)
        {
            var abilityMeta = await GetMetaAsync();

            if (abilityMeta.ReduceTaskDuration.Count > Level)
            {
                productionTask.TaskDuration = productionTask.TaskDuration.Subtract(abilityMeta.ReduceTaskDuration[Level]);
                productionTask.RemainingTime = productionTask.RemainingTime.Subtract(abilityMeta.ReduceTaskDuration[Level]);
            }
        }
    }
}
