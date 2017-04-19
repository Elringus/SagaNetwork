using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SagaNetwork.Models;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    public class PlayersEditorController : BaseEditorController<Player>
    {
        public async Task<IActionResult> CreatePlayer (string idAndPswd)
        {
            var splittedString = idAndPswd.Split(';');
            var playerId = splittedString[0];
            var password = splittedString.Length > 1 ? splittedString[1] : string.Empty;

            var data = JToken.Parse($@"{{'PlayerId':'{playerId}','Password':'{password}'}}");
            var createPlayerController = new CreatePlayerController();
            var responseToken = await createPlayerController.HandleHttpRequestAsync(data);

            return RedirectToAction("Edit", new { entityId = playerId });
        }
    }
}
