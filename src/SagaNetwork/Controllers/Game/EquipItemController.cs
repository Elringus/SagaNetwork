using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), RequireAuth, GenerateApi(UClassType.Request)]
    public class EquipItemController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string CharacterId;
        [ApiRequestData] public string ItemId;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            var character = player.GetCharacterById(CharacterId);
            if (character == null) return JStatus.CharacterNotFound;

            var item = player.GetItemById(ItemId);
            if (item == null) return JStatus.ItemNotFound;

            if (!await player.EquipItemAsync(item, character))
                return JStatus.Fail;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            return JStatus.Ok;
        }
    }
}
