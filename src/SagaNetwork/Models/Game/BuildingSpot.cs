
namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class BuildingSpot : MetaDescribedModel<BuildingSpotMeta>
    {
        public string Address { get; set; }
        public Building Building { get; set; }
        public bool IsOccupied => Building != null;

        public BuildingSpot () { }
        public BuildingSpot (string metaId) : base(metaId) { }
    }
}
