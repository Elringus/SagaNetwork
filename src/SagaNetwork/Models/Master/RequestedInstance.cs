using System;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a requested arena instance, 
    /// which is currently in a queue to be started on one of the game servers.
    /// </summary>
    public class RequestedInstance : TableModel<RequestedInstance>
    {
        public static readonly TimeSpan REQUEST_INSTANCE_TIMEOUT = TimeSpan.FromMinutes(1);

        public string ArenaMetaId => Id; 
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public override string BaseTableName => "RequestedInstances"; 

        public RequestedInstance () { }
        public RequestedInstance (string arenaMetaId) : base(arenaMetaId) { }
    }
}
