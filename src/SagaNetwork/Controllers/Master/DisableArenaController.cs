using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// Controller used by UE servers to set IsAvailable property of the arena meta to false.
    /// Used to prevent loading arena maps which doesn't exist in the build.
    /// </summary>
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class DisableArenaController : BaseController
    {
        [ApiRequestData] public string ArenaMetaId;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var arenaMeta = await new ArenaMeta(ArenaMetaId).LoadAsync();
            if (arenaMeta == null) return JStatus.MetaNotFound;

            arenaMeta.IsAvailable = false;
            if (!await arenaMeta.ReplaceAsync())
            {
                OccFailFlag = true;
                return JStatus.OccFail;
            }

            return JStatus.Ok;
        }
    }
}
