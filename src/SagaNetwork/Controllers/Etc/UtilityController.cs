using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SagaNetwork.Models;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// A controller to perform various one-time utility operations.
    /// Will only have effect when 'IsUtilityOperationsAllowed' global config is enabled.
    /// Invoke actions by requesting specified route URI. ex: https://saga-d-network.azurewebsites.net/utility/SetTalentPointsForAllPlayers/10
    /// </summary>
    public class UtilityController : Controller
    {
        const string UTILITY_OPS_DISABLED_MESSAGE = "Error: utility operations are disabled.";

        [HttpGet, Route("utility/SetTalentPointsForAllPlayers/{amount}")]
        public async Task<string> SetTalentPointsForAllPlayers (int amount)
        {
            if (!Configuration.IsUtilityOperationsAllowed) return UTILITY_OPS_DISABLED_MESSAGE;

            var opsCount = 0;
            var failCount = 0;
            foreach (var player in await new Player().RetrieveAllAsync())
            {
                foreach (var character in player.Characters)
                    character.TalentPoints = amount;
                if (await player.ReplaceAsync()) opsCount++;
                else failCount++;
            }

            return $"Operation completed.\nPlayers updated: {opsCount}\nOCC fails: {failCount}";
        }

        [HttpGet, Route("utility/CreateDummyPlayer")]
        public async Task<string> CreateDummyPlayer ()
        {
            if (!Configuration.IsUtilityOperationsAllowed) return UTILITY_OPS_DISABLED_MESSAGE;

            var createPlayerController = new CreatePlayerController();
            await createPlayerController.HandleHttpRequestAsync(JToken.Parse(@"{'PlayerId':'Dummy','Password':'123'}"));

            return $"Operation completed.\nDummy player created.";
        }

        [HttpGet, Route("utility/CreateMillionTestPlayers")]
        public async Task<string> CreateMillionTestPlayers ()
        {
            if (!Configuration.IsUtilityOperationsAllowed) return UTILITY_OPS_DISABLED_MESSAGE;

            for (int i = 0; i < 1000000; i++)
            {
                var createPlayerController = new CreatePlayerController();
                await createPlayerController.HandleHttpRequestAsync(JToken.Parse($@"{{'PlayerId':'TestPlayer{i}','Password':'123'}}"));
            }

            return $"Operation completed.\n1 million players created.";
        }

        [HttpGet, Route("utility/DeleteAllPlayers")]
        public async Task<string> DeleteAllPlayers ()
        {
            if (!Configuration.IsUtilityOperationsAllowed) return UTILITY_OPS_DISABLED_MESSAGE;

            var opsCount = 0;
            var failCount = 0;
            foreach (var player in await new Player().RetrieveAllAsync())
            {
                if (!await player.DeleteAsync())
                    failCount++;
                opsCount++;
            }

            return $"Operation completed.\nPlayers deleted: {opsCount}\nOCC fails: {failCount}";
        }

        [HttpGet, Route("utility/Test")]
        public async Task<string> Test ()
        {
            if (!Configuration.IsUtilityOperationsAllowed) return UTILITY_OPS_DISABLED_MESSAGE;

            var classMetas = await new ClassMeta().RetrieveAllAsync();

            var opsCount = 0;
            foreach (var classMeta in classMetas)
            {
                opsCount++;
                classMeta.IsInitiallyAvailable = true;
                if (!await classMeta.ReplaceAsync())
                    return "Operation failed.";
            }

            return $"Operation completed.\nMetas updated: {opsCount}";
        }
    }
}
