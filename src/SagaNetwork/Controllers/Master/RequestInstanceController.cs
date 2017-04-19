using System;
using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class RequestInstanceController : BaseController
    {
        [ApiRequestData] public string ArenaMetaId;

        [ApiResponseData] public string Ip;
        [ApiResponseData] public string Port;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            // Ensure requested arena meta exists.
            var arenaMeta = await new ArenaMeta(ArenaMetaId).LoadAsync();
            if (arenaMeta == null) return JStatus.MetaNotFound;

            // Check if arena is available.
            if (!arenaMeta.IsAvailable) return JStatus.ArenaUnavailable;

            int instanceIndex;
            var hostServer = GameServer.FindInstanceByArenaId(out instanceIndex, ArenaMetaId, true, true);
            if (hostServer == null)
            {
                // Check if instance is already requested and is activating.
                var requestedInstance = await new RequestedInstance(ArenaMetaId).LoadAsync();
                if (requestedInstance != null)
                {
                    // If requested instance is awaited too long — delete it and start over.
                    if ((DateTime.UtcNow - requestedInstance.RequestedDate) > RequestedInstance.REQUEST_INSTANCE_TIMEOUT)
                        await requestedInstance.DeleteAsync();
                    else return JStatus.Wait;
                }

                await new RequestedInstance(ArenaMetaId).InsertAsync();

                ServiceBus.SendInstanceRequestMessage(ArenaMetaId);

                return JStatus.Wait;
            }

            hostServer.ActiveInstances[instanceIndex].FreeSlots--;

            if (!await hostServer.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            Ip = hostServer.Ip;
            Port = hostServer.ActiveInstances[instanceIndex].Port;

            return JStatus.Ok;
        }
    }
}
