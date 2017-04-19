using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class City : MetaDescribedModel<CityMeta>
    {
        public List<Ward> Wards { get; set; } = new List<Ward>();

        public City () { }
        public City (string metaId) : base(metaId) { }
    }
}
