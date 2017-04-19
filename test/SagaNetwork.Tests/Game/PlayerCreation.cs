using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SagaNetwork.Tests
{
    public class PlayerCreation : InitTest
    {
        [Fact]
        public async void PlayerCreated ()
        {
            var playerId = "Test_PlayerCreated";
            var playerPassword = "Test_Password";

            // Delete existing test player.
            var existingPlayer = await new Player(playerId).LoadAsync();
            if (existingPlayer != null) await existingPlayer.DeleteAsync();

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'Password':'{playerPassword}'
            }}");

            // Execute controller.
            var createPlayerController = new CreatePlayerController();
            var responseToken = await createPlayerController.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken, JStatus.Ok);

            // Load new player.
            var createdPlayer = await new Player(playerId).LoadAsync();

            // Assert new player is loaded.
            Assert.NotNull(createdPlayer);
        }

        [Fact]
        public async void PlayerWithExistingIdNotCreated ()
        {
            var playerId = "Test_PlayerWithExistingIdNotCreated";
            var playerPassword = "Test_Password";

			// Make sure test player exists.
			var existingPlayer = await Helpers.CreateTestPlayer(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'Password':'{playerPassword}'
            }}");

            // Execute controller.
            var createPlayerController = new CreatePlayerController();
            var responseToken = await createPlayerController.HandleHttpRequestAsync(data);

            // Assert controller response status is PlayerAlreadyExists.
            Assert.Equal(responseToken, JStatus.PlayerAlreadyExists);
        }

        [Fact]
        public async void PlayerCreatedWithDefaultParams ()
        {
            var playerId = "Test_PlayerCreatedWithDefaultParams";
            var itemMetaId = "Test_PlayerCreatedWithDefaultParams";
            var classMetaId = "Test_PlayerCreatedWithDefaultParams";
            var playerPassword = "Test_Password";

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                classMeta.DefaultItems = new List<string> { itemMetaId };
                classMeta.IsInitiallyAvailable = true;
                await classMeta.InsertAsync();
            }

            // Delete existing test player.
            var existingPlayer = await new Player(playerId).LoadAsync();
            if (existingPlayer != null) await existingPlayer.DeleteAsync();

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'Password':'{playerPassword}'
            }}");

            // Execute controller.
            var createPlayerController = new CreatePlayerController();
            var responseToken = await createPlayerController.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken, JStatus.Ok);

            // Load new player.
            var createdPlayer = await new Player(playerId).LoadAsync();

            // Make sure default character is added.
            Assert.True(createdPlayer.Characters.Exists(character => character.ClassMetaId == classMetaId));

            // Make sure default item is added.
            Assert.True(createdPlayer.HasItemOfMeta(itemMetaId));
        }
    }
}
