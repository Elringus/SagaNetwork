using System;
using System.Collections.Generic;

namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class WardMeta : TableModel<WardMeta>
    {
        /// <summary>
        /// Spot address (unique ID in ward) -> BuildingSpotMeta ID
        /// </summary>
        public Dictionary<string, string> BuildingSpotsMap { get; set; } = new Dictionary<string, string>();
        public Requirement UnlockRequirement { get; set; } = new Requirement();
        public Reward UnlockReward { get; set; } = new Reward();
        public TimeSpan UnlockTime { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Whether this ward should be unlocked at the start of the game.
        /// </summary>
        public bool IsInitiallyUnlocked { get; set; }

        public override string BaseTableName => "WardMetas"; 

        public WardMeta () { }
        public WardMeta (string id) : base (id) { }
    }
}
