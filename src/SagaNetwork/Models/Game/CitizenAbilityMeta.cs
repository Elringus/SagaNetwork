using System;
using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class CitizenAbilityMeta : TableModel<CitizenAbilityMeta>, IWeighted
    {
        public int ProbabilityWeight { get; set; }
        public List<int> ExpToLevelUp { get; set; } = new List<int>();
        public List<TimeSpan> ReduceTaskDuration { get; set; } = new List<TimeSpan>();
        public List<float> IncreaseRewardProbability { get; set; } = new List<float>();

        public override string BaseTableName => "CitizenAbilityMetas";

        public CitizenAbilityMeta () { }
        public CitizenAbilityMeta (string id) : base (id) { }

    }
}
