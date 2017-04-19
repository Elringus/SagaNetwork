using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class FinishWardUnlockingController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string WardId;

        [ApiResponseData] public TimeTask UpdatedUnlockTask;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;
            var ward = player.GetWardById(WardId);
            if (ward == null) return JStatus.WardNotFound;
            var wardMeta = await ward.GetMetaAsync();
            if (wardMeta == null) return JStatus.MetaNotFound;

            if (ward.UnlockTask == null)
                return JStatus.Fail;

            if (ward.UnlockTask.Check() == 0)
            {
                UpdatedUnlockTask = ward.UnlockTask;
                return JStatus.NotReady;
            }

            ward.IsUnlocked = true;

            if (wardMeta.UnlockReward != null)
                await wardMeta.UnlockReward.AwardPlayerAsync(player);

            ward.UnlockTask = null;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}

