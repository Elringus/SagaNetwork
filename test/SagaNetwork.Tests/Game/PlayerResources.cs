using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SagaNetwork.Tests
{
    public class PlayerResources : InitTest
    {
        [Fact]
        public async void ResourcesAdded ()
        {
            const string playerId = "Test_ResourcesAdded";
            var resourcesToAdd = new List<int> { 100, 0, 50, 5000, 1, 0, 999 };

            // Create test player.
            var player = await Helpers.CreateTestPlayer(playerId);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                {Helpers.JsonServerCredentials},
                'Resources':'{JsonConvert.SerializeObject(resourcesToAdd)}'
            }}");

            // Execute controller.
            var controller = new AddResourcesController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure resources is added.
            player = await player.LoadAsync();
            Assert.Equal(player.Resources, resourcesToAdd);
        }
    }
}

