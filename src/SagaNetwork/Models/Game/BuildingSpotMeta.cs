namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class BuildingSpotMeta : TableModel<BuildingSpotMeta>
    {
        public string Size { get; set; }

        public override string BaseTableName => "BuildingSpotMetas"; 

        public BuildingSpotMeta () { }
        public BuildingSpotMeta (string id) : base (id) { }
    }
}
