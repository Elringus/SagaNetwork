using Newtonsoft.Json.Linq;

namespace SagaNetwork
{
    public static class Authorization
    {
        public static bool IsEnabled => Configuration.IsAuthEnabled;

        /// <summary>
        /// Retrieves the PlayerId and SessionToken from json data and checks if token is valid.
        /// Will always succeed when IsAuthEnabled is set to false or if ServerAuthKey is validated.
        /// </summary>
        /// <param name="data">Json token, should contain playerId and sessionToken fields.</param>
        /// <param name="requireServerAuth"></param>
        /// <returns>If token exists and is valid.</returns>
        public static bool CheckAuth (JToken data, bool requireServerAuth = false)
        {
            if (!IsEnabled) return true;

            var serverAuthKey = data.ParseField<string>("ServerAuthKey");
            var isServer = !string.IsNullOrWhiteSpace(serverAuthKey) &&
                serverAuthKey.Equals(Configuration.AppSettings["ServerAuthKey"]);
            if (isServer) return true;
            if (requireServerAuth) return false;

            var playerId = data.ParseField<string>("PlayerId");
            if (string.IsNullOrWhiteSpace(playerId)) return false;
            var sessiontToken = data.ParseField<string>("SessionToken");
            if (string.IsNullOrWhiteSpace(sessiontToken)) return false;
            return RedisCache.CheckKeyValue(playerId, sessiontToken);
        }

        /// <summary>
        /// Generates and adds a session token for the specified player.
        /// Token will be valid for 24 hours.
        /// </summary>
        /// <param name="playerId">ID of the player.</param>
        /// <returns>Generated session token.</returns>
        public static string AddTokenForPlayer (string playerId)
        {
            return RedisCache.SetGuidForKey(playerId);
        }
    }
}
