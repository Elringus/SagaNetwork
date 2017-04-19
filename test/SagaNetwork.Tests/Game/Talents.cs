using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SagaNetwork.Tests
{
    public class Talents : InitTest
    {
        [Fact]
        public async void AccessibleTalentsLearned ()
        {
            await PerformAccessibleTalentLearnedTestForTier(1);
            await PerformAccessibleTalentLearnedTestForTier(-3);
        }

        [Fact]
        public async void TalentNotLearnedWhenRequirementsNotFulfilled ()
        {
            string testId = $"Test_TalentNotLearnedWhenRequirementsNotFulfilled";
            var talentsToLearn = new List<string> { testId };
            
            // Create test ability meta if it doesn't exist.
            var abilityMeta = await new AbilityMeta(testId).LoadAsync();
            if (abilityMeta == null)
            {
                abilityMeta = new AbilityMeta(testId);
                await abilityMeta.InsertAsync();
            }

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(testId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(testId);
                classMeta.Abilities.Add(abilityMeta.Id);
                await classMeta.InsertAsync();
            }

            // Create test talent meta if it doesn't exist.
            await CreateTestTalentMetaOfTier(testId, testId, 2);

            // Create test player with not fulfilled tier prereqs.
            var player = await Helpers.CreateTestPlayer(testId);
            var character = new Character(testId);
            character.TalentPoints = 10;
            player.Characters.Add(character);
            await player.ReplaceAsync();

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(testId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{testId}',
                'SessionToken':'{sessionToken}',
                'TalentMetaIds':'{JsonConvert.SerializeObject(talentsToLearn)}',
                'CharacterId':'{character.Id}'
            }}");

            // Execute controller.
            var controller = new LearnTalentsController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is RequirementNotFulfilled.
            Assert.Equal(responseToken["Status"], JStatus.RequirementNotFulfilled.JToken["Status"]);

            // Make sure talent is not learned.
            player = await player.LoadAsync();
            Assert.False(player.GetCharacterById(character.Id).IsTalentLearned(testId));
        }

        [Fact]
        public async void TalentNotLearnedWhenNotEnoughPoints ()
        {
            string testId = $"Test_TalentNotLearnedWhenNotEnoughPoints";
            var talentsToLearn = new List<string> { testId };

            // Create test ability meta if it doesn't exist.
            var abilityMeta = await new AbilityMeta(testId).LoadAsync();
            if (abilityMeta == null)
            {
                abilityMeta = new AbilityMeta(testId);
                await abilityMeta.InsertAsync();
            }

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(testId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(testId);
                classMeta.Abilities.Add(abilityMeta.Id);
                await classMeta.InsertAsync();
            }

            // Create test talent meta if it doesn't exist.
            await CreateTestTalentMetaOfTier(testId, testId, 1);

            // Create test player with zero talent points.
            var player = await Helpers.CreateTestPlayer(testId);
            var character = new Character(testId);
            character.TalentPoints = 0;
            player.Characters.Add(character);
            await player.ReplaceAsync();

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(testId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{testId}',
                'SessionToken':'{sessionToken}',
                'TalentMetaIds':'{JsonConvert.SerializeObject(talentsToLearn)}',
                'CharacterId':'{character.Id}'
            }}");

            // Execute controller.
            var controller = new LearnTalentsController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is RequirementNotFulfilled.
            Assert.Equal(responseToken["Status"], JStatus.RequirementNotFulfilled.JToken["Status"]);

            // Make sure talent is not learned.
            player = await player.LoadAsync();
            Assert.False(player.GetCharacterById(character.Id).IsTalentLearned(testId));
        }

        private async Task PerformAccessibleTalentLearnedTestForTier (int tier)
        {
            string testId = $"Test_Accessible{tier}TierTalentLearned";
            var talentsToLearn = new List<string> { testId };

            // Create test ability meta if it doesn't exist.
            var abilityMeta = await new AbilityMeta(testId).LoadAsync();
            if (abilityMeta == null)
            {
                abilityMeta = new AbilityMeta(testId);
                await abilityMeta.InsertAsync();
            }

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(testId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(testId);
                classMeta.Abilities.Add(abilityMeta.Id);
                await classMeta.InsertAsync();
            }

            // Create test talent meta if it doesn't exist.
            await CreateTestTalentMetaOfTier(testId, testId, tier);

            // Create test player.
            var player = await Helpers.CreateTestPlayer(testId);
            var character = new Character(testId);
            character.TalentPoints = 1;
            // Create prereq metas for and add talents from previous tiers.
            if (tier > 0)
            {
                for (int i = 1; i < tier; i++)
                {
                    var ic = i;
                    var prereqTalentMetaId = testId + ic;
                    await CreateTestTalentMetaOfTier(prereqTalentMetaId, testId, ic);
                    character.LearnedTalentMetaIds.Add(prereqTalentMetaId);
                }
            }
            else if (tier < 0)
            {
                for (int i = -1; i > tier; i--)
                {
                    var ic = i;
                    var prereqTalentMetaId = testId + ic;
                    await CreateTestTalentMetaOfTier(prereqTalentMetaId, testId, ic);
                    character.LearnedTalentMetaIds.Add(prereqTalentMetaId);
                }
            }
            player.Characters.Add(character);
            await player.ReplaceAsync();

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(testId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{testId}',
                'SessionToken':'{sessionToken}',
                'TalentMetaIds':'{JsonConvert.SerializeObject(talentsToLearn)}',
                'CharacterId':'{character.Id}'
            }}");

            // Execute controller.
            var controller = new LearnTalentsController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure talent is learned.
            player = await player.LoadAsync();
            Assert.True(player.GetCharacterById(character.Id).IsTalentLearned(testId));
        }

        private async Task CreateTestTalentMetaOfTier (string talentId, string affectedAbilityMetaId, int tier)
        {
            var talentMeta = await new TalentMeta(talentId).LoadAsync();
            if (talentMeta == null)
            {
                talentMeta = new TalentMeta(talentId);
                talentMeta.AbilityMetaId = affectedAbilityMetaId;
                talentMeta.Tier = tier;
                await talentMeta.InsertAsync();
            }
        }
    }
}

