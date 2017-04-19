using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class StartWardUnlockingController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string WardId;

        [ApiResponseData] public TimeTask StartedUnlockTask;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var ward = player.GetWardById(WardId);
            if (ward == null) return JStatus.WardNotFound;
            var wardMeta = await ward.GetMetaAsync();
            if (wardMeta == null) return JStatus.MetaNotFound;

            if (ward.IsUnlocked)
                return JStatus.AlreadyUnlocked;
            if (ward.UnlockTask != null)
                return JStatus.NotReady;
            if (!wardMeta.UnlockRequirement.IsFulfilledByPlayer(player))
                return JStatus.RequirementNotFulfilled;

            player.SpendResources(wardMeta.UnlockRequirement.Resources);
            ward.UnlockTask = new TimeTask(wardMeta.UnlockTime);

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            StartedUnlockTask = ward.UnlockTask;

            return JStatus.Ok;
        }
    }
}
