using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Collections.Generic;

namespace SagaNetwork.Tests
{
    public class Citizens : InitTest
    {
        [Fact]
        public async void RandomCitizenGenerated ()
        {
            var testId = "Test_RandomCitizenGenerated";

            // Create test rarity group meta if it doesn't exist.
            var rarityGroup = await new RarenessGroup(testId).LoadAsync();
            if (rarityGroup == null)
            {
                rarityGroup = new RarenessGroup(testId);
                rarityGroup.AbilityLimit = 1;
                rarityGroup.AbilityMaxLevel = 5;
                rarityGroup.ProbabilityWeight = 50;
                await rarityGroup.InsertAsync();
            }

            // Create test appearance meta if it doesn't exist.
            var appearance = await new AppearanceElementMeta(testId).LoadAsync();
            if (appearance == null)
            {
                appearance = new AppearanceElementMeta(testId);
                appearance.AppearanceGroup = AppearanceGroup.Face;
                await appearance.InsertAsync();
            }

            // Create test citizen ability meta if it doesn't exist.
            var abilityMeta = await new CitizenAbilityMeta(testId).LoadAsync();
            if (abilityMeta == null)
            {
                abilityMeta = new CitizenAbilityMeta(testId);
                abilityMeta.ProbabilityWeight = 50;
                abilityMeta.ExpToLevelUp = new List<int>() { 10, 30, 80 };
                await abilityMeta.InsertAsync();
            }

            // Create test player.
            var player = await Helpers.CreateTestPlayer(testId);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(testId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{testId}',
                 {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var controller = new AddRandomCitizenController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Assert citizen is generated and added.
            player = await new Player(testId).LoadAsync();
            Assert.True(player.Citizens.Count > 0);
            Assert.True(player.Citizens[0].RarenessGroupId == testId);
            Assert.True(player.Citizens[0].Appearance[0].MetaId == testId);
        }

        [Fact]
        public async void CitizenAbilityModifiedProduction ()
        {
            var testId = "Test_CitizenAbilityModifiedProduction";
            var contractReward = new Reward() { Resources = new List<int> { 0, 0, 111 } };
            var contractProductionTime = TimeSpan.FromSeconds(10);

            // Create test contract meta if it doesn't exist.
            var contractMeta = await new ContractMeta(testId).LoadAsync();
            if (contractMeta == null)
            {
                contractMeta = new ContractMeta(testId);
                contractMeta.Rewards.Add(contractReward);
                contractMeta.ProductionTime = contractProductionTime;
                await contractMeta.InsertAsync();
            }

            // Create test building meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(testId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(testId);
                buildingMeta.StorageLimit = 1000;
                await buildingMeta.InsertAsync();
            }

            // Create test citizen ability meta if it doesn't exist.
            var abilityMeta = await new CitizenAbilityMeta(testId).LoadAsync();
            if (abilityMeta == null)
            {
                abilityMeta = new CitizenAbilityMeta(testId);
                abilityMeta.ReduceTaskDuration = new List<TimeSpan> { TimeSpan.FromSeconds(9) };
                await abilityMeta.InsertAsync();
            }

            // Create test player, building and citizen.
            var building = new Building(testId);
            building.IsConstructed = true;
            building.ProductionTask = new ProductionTask(contractMeta);
            var player = await Helpers.CreateTestPlayer(testId, building: building);
            var citizen = new Citizen();
            citizen.Abilities.Add(new CitizenAbility(testId));
            player.Citizens.Add(citizen);
            building.AssignedCitizenIds.Add(citizen.Id);
            await player.ReplaceAsync();

            // Wait for contract to complete (contractProductionTime - ReduceTaskDuration + .5f).
            Thread.Sleep(TimeSpan.FromSeconds(1.5f));

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(testId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{testId}',
                'SessionToken':'{sessionToken}',
                'BuildingId':'{building.Id}'
            }}");

            // Execute controller.
            var controller = new CheckProductionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Test reward collection.
            var collectController = new CollectProductionRewardsController();
            var collectResponseToken = await collectController.HandleHttpRequestAsync(data);
            Assert.Equal(collectResponseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure reward is added.
            player = await player.LoadAsync();
            Assert.True(player.Resources[2] > 0);
        }
    }
}

