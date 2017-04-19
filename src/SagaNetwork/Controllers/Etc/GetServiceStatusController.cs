using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), GenerateApi(UClassType.Request)]
    public class GetServiceStatusController : BaseController
    {
        [ApiResponseData] public string BuildVersion;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            BuildVersion = Configuration.BuildVersion;

            if (await GameServer.GetIsAnyUpdating()) return JStatus.Updating;

            return Configuration.IsServiceOnline ? JStatus.Ok : JStatus.Offline;
        }
    }
}

