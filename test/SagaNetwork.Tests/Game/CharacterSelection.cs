using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SagaNetwork.Tests
{
    public class CharacterSelection : InitTest
    {
        [Fact]
        public async void AvailableCharacterSelected ()
        {
            var buildingMetaId = "Test_AvailableCharacterSelected";
            var classMetaId = "Test_AvailableCharacterSelected";
            var playerId = "Test_AvailableCharacterSelected";
            var characterIndex = 0;

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                classMeta.Requirement = new Requirement() { BuildingMetaIds = new List<string> { buildingMetaId } };
                await classMeta.InsertAsync();
            }

            // Create test player, character and required building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = true;
            var character = new Character(classMetaId);
            var player = await Helpers.CreateTestPlayer(playerId, building: building, character: character);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'CharacterIndex':{characterIndex}
            }}");

            // Execute controller.
            var controller = new SelectCharacterController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure char is selected for the first time (req-free).
            player = await player.LoadAsync();
            Assert.True(player.SelectedCharacterIndex == characterIndex);

            // Make sure char is selected for the second time (with req checked).
            responseToken = await controller.HandleHttpRequestAsync(data);
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);
        }

        [Fact]
        public async void UnavailableCharacterNotSelected ()
        {
            var buildingMetaId = "Test_UnavailableCharacterNotSelected";
            var classMetaId = "Test_UnavailableCharacterNotSelected";
            var playerId = "Test_UnavailableCharacterNotSelected";
            var characterIndex = 0;

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                classMeta.Requirement = new Requirement() { BuildingMetaIds = new List<string> { buildingMetaId } };
                await classMeta.InsertAsync();
            }

            // Create test player, character, but without the required building.
            var character = new Character(classMetaId);
            var player = await Helpers.CreateTestPlayer(playerId, character: character);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'CharacterIndex':{characterIndex}
            }}");

            // Execute controller.
            var controller = new SelectCharacterController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure char is selected for the first time (req-free).
            player = await player.LoadAsync();
            Assert.True(player.SelectedCharacterIndex == characterIndex);

            // Make sure char is not selected for the second time (with req checked).
            responseToken = await controller.HandleHttpRequestAsync(data);
            Assert.Equal(responseToken["Status"], JStatus.RequirementNotFulfilled.JToken["Status"]);
        }
    }
}

