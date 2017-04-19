using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class SelectCharacterController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public int CharacterIndex;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            if (player.Characters.Count <= CharacterIndex) return JStatus.CharacterNotFound;

            if (player.SelectedCharacterIndex >= 0) // not first-time char selection
            {
                var classMeta = await player.Characters[CharacterIndex].GetMetaAsync();
                if (!classMeta.Requirement.IsFulfilledByPlayer(player))
                    return JStatus.RequirementNotFulfilled;
            }

            player.SelectedCharacterIndex = CharacterIndex;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
