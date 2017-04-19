using System;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class ArenaMeta : TableModel<ArenaMeta>
    {
        public string Map { get; set; }
        public int MaxPlayers { get; set; }
        public string Mode { get; set; }
        public string Icon { get; set; }
        public string EndCondition { get; set; }
        public int? EndKillCount { get; set; }
        public TimeSpan? EndTimeOut { get; set; }
        public int? EndFlagCount { get; set; }
        public bool IsAvailable { get; set; } = true;

        public override string BaseTableName => "ArenaMetas";

        public ArenaMeta () { }
        public ArenaMeta (string id) : base (id) { }
    }
}
