using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class Building : MetaDescribedModel<BuildingMeta>
    {
        public bool IsConstructed { get; set; }
        public TimeTask ConstructionTask { get; set; }
        public ProductionTask ProductionTask { get; set; }
        public List<Reward> StoredRewards { get; set; } = new List<Reward>();
        public List<string> AssignedCitizenIds { get; set; } = new List<string>();

        public Building () { }
        public Building (string metaId) : base (metaId) { }

        public int CalculateStoredVolume () => StoredRewards.Sum(reward => reward.CalculateVolume());
        public async Task<int> GetStorageLimitAsync () => (await GetMetaAsync()).StorageLimit;
        public int GetStorageLimit (BuildingMeta buildingMeta) => buildingMeta.StorageLimit;
        public async Task<int> CalculateAvailableStorageAsync () => (await GetStorageLimitAsync()) - CalculateStoredVolume();
        public int CalculateAvailableStorage (int storageLimit) => storageLimit - CalculateStoredVolume();

        public List<Citizen> GetAssignedCitizens (Player player)
        {
            var assignedCitizens = new List<Citizen>();
            foreach (var assignedCitizenId in AssignedCitizenIds)
                assignedCitizens.Add(player.GetCitizenById(assignedCitizenId));

            return assignedCitizens;
        }
    }
}
