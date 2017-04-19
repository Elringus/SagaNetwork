using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireServerAuth, GenerateApi(UClassType.Request)]
    public class AddExperienceController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public int Experience;
        [ApiRequestData] public int CharacterIndex;

        [ApiResponseData] public int NewLevel;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            if (player.Characters.Count <= CharacterIndex) return JStatus.CharacterNotFound;

            var character = player.Characters[CharacterIndex];
            var classMeta = await character.GetMetaAsync();
            if (classMeta == null) return JStatus.MetaNotFound;

            if (await character.IsOnMaxLevel(classMeta)) return JStatus.MaxLevelReached;

            var earnedLevel = await player.Characters[CharacterIndex].EarnExperienceAsync(Experience, classMeta);

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            if (earnedLevel.HasValue)
                NewLevel = earnedLevel.Value;

            return JStatus.Ok;
        }
    }
}
