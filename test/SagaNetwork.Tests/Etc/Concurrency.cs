using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System.Threading;
using System;

namespace SagaNetwork.Tests
{
    public class Concurrency : InitTest
    {
        [Fact]
        public async void OccTriggers ()
        {
            var playerId = "Test_OccTriggers";

            // Create test player if it doesn't exist.
            await Helpers.CreateTestPlayer(playerId);

            var playerOld = await new Player(playerId).LoadAsync();
            var playerNew = await new Player(playerId).LoadAsync();

            // Should succeed.
            playerNew.SelectedCharacterIndex = 1;
            var newSucceed = await playerNew.ReplaceAsync();
            Assert.True(newSucceed);

            // Should fail and trigger OCC, as the entity was modified before.
            playerOld.SelectedCharacterIndex = 2;
            var oldSucceed = await playerOld.ReplaceAsync();
            Assert.False(oldSucceed);

            // Assert the data was not rewritten by old entity. 
            var player = await new Player(playerId).LoadAsync();
            Assert.Equal(player.SelectedCharacterIndex, 1);
        }
    }
}

