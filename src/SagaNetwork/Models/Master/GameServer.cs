using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a dedicated VM unit, which hosts UE4 server build instances for arena mode.
    /// </summary>
    public class GameServer : TableModel<GameServer>
    {
        public string Ip => Id; 
        public bool IsUpdating { get; set; } = false;
        public List<ArenaInstance> ActiveInstances { get; set; } = new List<ArenaInstance>();

        public override string BaseTableName => "GameServers";

        public GameServer () { }
        public GameServer (string ip) : base(ip) { }

        public static async Task<GameServer> FindGameServerByInstance (ArenaInstance instance)
        {
            var gameServers = await new GameServer().RetrieveAllAsync();

            return gameServers.Find(s => s.ActiveInstances.Exists(i => i.Port == instance.Port));
        }

        /// <summary>
        /// Searches for an active arena instance with specified arena ID.
        /// </summary>
        /// <param name="instanceIndex">If found, will contain an index of the required instance in server's ActiveInstances.</param>
        /// <param name="arenaId">ID of the required arena.</param>
        /// <param name="free">Look only for instances with free slots.</param>
        /// <param name="open">Look only for open instances.</param>
        /// <returns>A game server which contains required instance. Null if not found.</returns>
        public static GameServer FindInstanceByArenaId (out int instanceIndex, string arenaId, bool free = false, bool open = false)
        {
            instanceIndex = 0;
            GameServer hostServer;
            Predicate<ArenaInstance> comparer = (i) => { return i.MetaId == arenaId && (!free || i.FreeSlots > 0) && (!open || i.IsOpen); };

            var retrieveAllServersTask = new GameServer().RetrieveAllAsync();
            retrieveAllServersTask.Wait();
            var gameServers = retrieveAllServersTask.Result;

            // Filters the servers by predicate and gets the one with an 'oldest' instance 
            // to make sure we are filling instances in order they've been requested.
            hostServer = gameServers.FindAll(s => s.ActiveInstances.Exists(comparer)).OrderBy(s => s.ActiveInstances.Min(i => i.StartedDate)).FirstOrDefault();
            if (hostServer == null) return null;

            instanceIndex = hostServer.ActiveInstances.FindIndex(comparer);
            return hostServer;
        }

        /// <summary>
        /// Searches for an active arena instance with specified port inside server with specified IP.
        /// </summary>
        /// <param name="instanceIndex">If found, will contain an index of the required instance in server's ActiveInstances.</param>
        /// <param name="ip">IP of the server.</param>
        /// <param name="port">Port of the instance.</param>
        /// <returns></returns>
        public static GameServer FindInstanceByIpAndPort (out int instanceIndex, string ip, string port)
        {
            instanceIndex = 0;
            GameServer hostServer;

            var loadServerTask = new GameServer(ip).LoadAsync();
            loadServerTask.Wait();
            hostServer = loadServerTask.Result;
            if (hostServer == null || !hostServer.ActiveInstances.Exists(i => i.Port == port)) return null;

            instanceIndex = hostServer.ActiveInstances.FindIndex(i => i.Port == port);
            return hostServer;
        }

        /// <summary>
        /// Whether any of the active servers processing an update.
        /// </summary>
        public static async Task<bool> GetIsAnyUpdating ()
        {
            var servers = await new GameServer().RetrieveAllAsync();
            return servers.Exists(s => s.IsUpdating); 
        }
    }
}
