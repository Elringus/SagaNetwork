using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class CityMeta : TableModel<CityMeta>
    {
        public List<string> WardMetaIds { get; set; } = new List<string>();

        public override string BaseTableName => "CityMetas";

        public CityMeta () { }
        public CityMeta (string id) : base (id) { }
    }
}
