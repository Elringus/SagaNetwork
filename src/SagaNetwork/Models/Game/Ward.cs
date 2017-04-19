using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class Ward : MetaDescribedModel<WardMeta>
    {
        public List<BuildingSpot> BuildingSpots { get; set; } = new List<BuildingSpot>();
        public bool IsUnlocked { get; set; }
        public TimeTask UnlockTask { get; set; }

        public Ward () { }
        public Ward (string metaId) : base(metaId) { }
    }
}
