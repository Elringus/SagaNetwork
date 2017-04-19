using System;
using System.Collections.Generic;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Used to define process of producing rewards over time.
    /// </summary>
    [GenerateApi(UClassType.MetaModel)]
    public class ContractMeta : TableModel<ContractMeta>
    {
        public Requirement Requirement { get; set; } = new Requirement();
        public TimeSpan ProductionTime { get; set; } = TimeSpan.Zero;
        public List<Reward> Rewards { get; set; } = new List<Reward>();
        /// <summary>
        /// Whether production cycle of the contract will loop until canceled.
        /// </summary>
        public bool IsLoopable { get; set; }

        public override string BaseTableName => "ContractMetas"; 

        public ContractMeta () { }
        public ContractMeta (string id) : base (id) { }
    }
}
