using System;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), GenerateApi(UClassType.Request)]
    public class AuthPlayerController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string Password;

        [ApiResponseData] public string SessionToken;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player(PlayerId).LoadAsync();
            if (player == null) return JStatus.PlayerNotFound;

            if (!PasswordHash.ValidatePassword(Password, player.PasswordHash))
                return JStatus.WrongPassword;

            player.LastLoginDate = DateTime.UtcNow;

            if (!await player.ReplaceAsync()) { OccFailFlag = true; return JStatus.OccFail; }

            SessionToken = Authorization.AddTokenForPlayer(PlayerId);

            return JStatus.Ok;
        }
    }
}
