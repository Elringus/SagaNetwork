using System;
using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class BuildingMeta : TableModel<BuildingMeta>
    {
        public Requirement ConstructionRequirement { get; set; } = new Requirement();
        public TimeSpan ConstructionTime { get; set; } = TimeSpan.Zero;
        public string Size { get; set; }
        public List<string> AvailableContractMetaIds { get; set; } = new List<string>();
        public int StorageLimit { get; set; }

        public override string BaseTableName => "BuildingMetas";

        public BuildingMeta () { }
        public BuildingMeta (string id) : base (id) { }
    }
}
