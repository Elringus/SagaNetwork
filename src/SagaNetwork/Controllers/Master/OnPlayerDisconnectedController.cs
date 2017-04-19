﻿using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class OnPlayerDisconnectedController : BaseController
    {
        [ApiRequestData] public string Ip;
        [ApiRequestData] public string Port;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            int instanceIndex;
            var hostServer = GameServer.FindInstanceByIpAndPort(out instanceIndex, Ip, Port);
            if (hostServer == null) return JStatus.NotFound;

            hostServer.ActiveInstances[instanceIndex].FreeSlots++;

            if (!await hostServer.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
