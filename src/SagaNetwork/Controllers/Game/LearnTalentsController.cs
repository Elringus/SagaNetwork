using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class LearnTalentsController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string CharacterId;
        [ApiRequestData] public List<string> TalentMetaIds;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            var character = player.GetCharacterById(CharacterId);
            if (character == null) return JStatus.CharacterNotFound;

            foreach (var talentMetaId in TalentMetaIds)
            {
                var talentMeta = await new TalentMeta(talentMetaId).LoadAsync();
                if (talentMeta == null) return JStatus.MetaNotFound;

                var talentLearned = await character.LearnTalent(talentMeta);
                if (!talentLearned) return JStatus.RequirementNotFulfilled;
            }

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
