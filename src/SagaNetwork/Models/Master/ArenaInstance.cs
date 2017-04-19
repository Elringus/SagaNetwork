using System;

namespace SagaNetwork.Models
{
    public class ArenaInstance : MetaDescribedModel<ArenaMeta>
    {
        public string Port { get; set; }
        public int FreeSlots { get; set; }
        public DateTime StartedDate { get; set; } = DateTime.UtcNow;
        public bool IsOpen { get; set; } = true;

        public ArenaInstance () { }
        public ArenaInstance (string arenaMetaId, string port) : base(arenaMetaId)
        {
            this.Port = port;

            var getMetaTask = GetMetaAsync();
            getMetaTask.Wait();
            if (getMetaTask.Result != null)
                FreeSlots = getMetaTask.Result.MaxPlayers;
        }
    }
}
