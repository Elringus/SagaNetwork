using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace SagaNetwork.Tests
{
    public class BuildingConstruction : InitTest
    {
        [Fact]
        public async void ConstructionOnAvailableSpotStarted ()
        {
            var size = "XXL";
            var buildingMetaId = "Test_ConstructionOnAvailableSpotStarted";
            var buildingSpotMetaId = "Test_ConstructionOnAvailableSpotStarted";
            var playerId = "Test_ConstructionOnAvailableSpotStarted";

            // Create test builidng meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.Size = size;
                await buildingMeta.InsertAsync();
            }

            // Create test builidng spot meta if it doesn't exist.
            var buildingSpotMeta = await new BuildingSpotMeta(buildingSpotMetaId).LoadAsync();
            if (buildingSpotMeta == null)
            {
                buildingSpotMeta = new BuildingSpotMeta(buildingSpotMetaId);
                buildingSpotMeta.Size = size;
                await buildingSpotMeta.InsertAsync();
            }

            // Create test player and building spot.
            var buildingSpot = new BuildingSpot(buildingSpotMetaId);
            var player = await Helpers.CreateTestPlayer(playerId, buildingSpot: buildingSpot);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'BuildingSpotId':'{buildingSpot.Id}',
                'BuildingMetaId':'{buildingMetaId}',
            }}");

            // Execute controller.
            var controller = new StartBuildingConstructionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure building construction is started.
            player = await player.LoadAsync();
            Assert.False(player.GetBuildingSpotById(buildingSpot.Id).Building.IsConstructed);
        }

        [Fact]
        public async void FinishedConstructionIsConstructed ()
        {
            var buildingMetaId = "Test_FinishedConstructionIsConstructed";
            var buildingConstructionTime = TimeSpan.FromMilliseconds(500);
            var playerId = "Test_FinishedConstructionIsConstructed";

            // Create test builidng meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.ConstructionTime = buildingConstructionTime;
                await buildingMeta.InsertAsync();
            }

            // Create test player and building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = false;
            building.ConstructionTask = new TimeTask(buildingMeta.ConstructionTime);
            var player = await Helpers.CreateTestPlayer(playerId, building: building);

            // Wait for building to construct.
            Thread.Sleep(buildingConstructionTime);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'BuildingId':'{building.Id}'
            }}");

            // Execute controller.
            var controller = new FinishBuildingConstructionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure building is constructed.
            player = await player.LoadAsync();
            Assert.True(player.GetBuildingById(building.Id).IsConstructed);
        }

        [Fact]
        public async void UnfinishedConstructionIsStillConstructing ()
        {
            var buildingMetaId = "Test_UnfinishedConstructionIsStillConstructing";
            var buildingConstructionTime = TimeSpan.FromMinutes(10);
            var playerId = "Test_UnfinishedConstructionIsStillConstructing";

            // Create test builidng meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.ConstructionTime = buildingConstructionTime;
                await buildingMeta.InsertAsync();
            }

            // Create test player and building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = false;
            building.ConstructionTask = new TimeTask(buildingMeta.ConstructionTime);
            var player = await Helpers.CreateTestPlayer(playerId, building: building);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'BuildingId':'{building.Id}'
            }}");

            // Execute controller.
            var controller = new FinishBuildingConstructionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is StillConstructing.
            Assert.Equal(responseToken["Status"], JStatus.NotReady.JToken["Status"]);

            // Make sure building is not constructed.
            player = await player.LoadAsync();
            Assert.False(player.GetBuildingById(building.Id).IsConstructed);
        }
    }
}

