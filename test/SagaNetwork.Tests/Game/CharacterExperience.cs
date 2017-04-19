using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class CharacterExperience : InitTest
    {
        [Fact]
        public async void ExperienceAdded ()
        {
            var classMetaId = "Test_ExperienceAdded";
            var playerId = "Test_ExperienceAdded";
            var expToAdd = 50;
            var charIndex = 0;

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                classMeta.ExpToLevelUp = new List<int>() { 0, 0, 100, 200, 300 };
                await classMeta.InsertAsync();
            }

            // Create test player.
            var player = await Helpers.CreateTestPlayer(playerId);
            player.Characters.Add(new Character(classMetaId));
            await player.ReplaceAsync();

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                {Helpers.JsonServerCredentials},
                'Experience':'{expToAdd}',
                'CharacterIndex':'{charIndex}'
            }}");

            // Execute controller.
            var controller = new AddExperienceController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure exp is added and level not increased.
            player = await player.LoadAsync();
            Assert.Equal(player.Characters[0].Experience, expToAdd);
            Assert.Equal(player.Characters[0].Level, 1);
        }

        [Fact]
        public async void LevelIncreased ()
        {
            var classMetaId = "Test_LevelIncreased2";
            var playerId = "Test_LevelIncreased2";
            var expToAdd = 700;
            var charIndex = 0;

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                classMeta.ExpToLevelUp = new List<int>() { 0, 100, 200, 300, 500 };
                await classMeta.InsertAsync();
            }

            // Create test player.
            var player = await Helpers.CreateTestPlayer(playerId);
            player.Characters.Add(new Character(classMetaId));
            await player.ReplaceAsync();

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                {Helpers.JsonServerCredentials},
                'Experience':'{expToAdd}',
                'CharacterIndex':'{charIndex}'
            }}");

            // Execute controller.
            var controller = new AddExperienceController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Assert new level is set in response.
            Assert.Equal(responseToken["NewLevel"], 3);

            // Make sure level is added to character.
            player = await player.LoadAsync();
            Assert.Equal(player.Characters[0].Level, 3);

            // Make sure talent points are added to character.
            Assert.Equal(player.Characters[0].TalentPoints, 3);
        }
    }
}

