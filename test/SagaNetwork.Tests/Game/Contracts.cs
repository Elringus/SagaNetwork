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
    public class Contracts : InitTest
    {
        [Fact]
        public async void AvailableContractSelected ()
        {
            var buildingMetaId = "Test_AvailableContractSelected";
            var contractMetaId = "Test_AvailableContractSelected";
            var playerId = "Test_AvailableContractSelected";

            // Create test building meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.AvailableContractMetaIds = new List<string> { contractMetaId };
                await buildingMeta.InsertAsync();
            }

            // Create test contract meta if it doesn't exist.
            var contractMeta = await new ContractMeta(contractMetaId).LoadAsync();
            if (contractMeta == null)
            {
                contractMeta = new ContractMeta(contractMetaId);
                contractMeta.Requirement = new Requirement() { BuildingMetaIds = new List<string> { buildingMetaId } };
                await contractMeta.InsertAsync();
            }

            // Create test player and building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = true;
            var player = await Helpers.CreateTestPlayer(playerId, building: building);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'BuildingId':'{building.Id}',
                'ContractMetaId':'{contractMetaId}'
            }}");

            // Execute controller.
            var controller = new StartProductionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure contract is selected.
            player = await player.LoadAsync();
            Assert.True(player.GetBuildingById(building.Id).ProductionTask.ProducedContractMetaId == contractMetaId);
        }

        [Fact]
        public async void UnavailableContractNotSelected ()
        {
            var requiredBuildingMetaId = "Test_UnavailableContractNotSelected_Required";
            var buildingMetaId = "Test_UnavailableContractNotSelected";
            var contractMetaId = "Test_UnavailableContractNotSelected";
            var playerId = "Test_UnavailableContractNotSelected";

            // Create required test building meta if it doesn't exist.
            var requiredBuildingMeta = await new BuildingMeta(requiredBuildingMetaId).LoadAsync();
            if (requiredBuildingMeta == null)
            {
                requiredBuildingMeta = new BuildingMeta(requiredBuildingMetaId);
                await requiredBuildingMeta.InsertAsync();
            }

            // Create 'wrong' test building meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.AvailableContractMetaIds = new List<string> { contractMetaId };
                await buildingMeta.InsertAsync();
            }

            // Create test contract meta if it doesn't exist.
            var contractMeta = await new ContractMeta(contractMetaId).LoadAsync();
            if (contractMeta == null)
            {
                contractMeta = new ContractMeta(contractMetaId);
                contractMeta.Requirement = new Requirement() { BuildingMetaIds = new List<string> { requiredBuildingMetaId } };
                await contractMeta.InsertAsync();
            }

            // Create test player with 'wrong' building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = true;
            var player = await Helpers.CreateTestPlayer(playerId, building: building);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'BuildingId':'{building.Id}',
                'ContractMetaId':'{contractMetaId}'
            }}");

            // Execute controller.
            var controller = new StartProductionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is RequirementNotFulfilled.
            Assert.Equal(responseToken["Status"], JStatus.RequirementNotFulfilled.JToken["Status"]);

            // Make sure contract is not selected.
            player = await player.LoadAsync();
            Assert.Null(player.GetBuildingById(building.Id).ProductionTask);
        }

        [Fact]
        public async void RewardForCompletedContractCollected ()
        {
            var contractMetaId = "Test_RewardForCompletedContractCollected";
            var contractReward = new Reward() { Resources = new List<int> { 0, 0, 111 } };
            var contractProductionTime = TimeSpan.FromMilliseconds(500);
            var buildingMetaId = "Test_RewardForCompletedContractCollected";
            var playerId = "Test_RewardForCompletedContractCollected";

            // Create test contract meta if it doesn't exist.
            var contractMeta = await new ContractMeta(contractMetaId).LoadAsync();
            if (contractMeta == null)
            {
                contractMeta = new ContractMeta(contractMetaId);
                contractMeta.Rewards.Add(contractReward);
                contractMeta.ProductionTime = contractProductionTime;
                await contractMeta.InsertAsync();
            }

            // Create test building meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.StorageLimit = 1000;
                await buildingMeta.InsertAsync();
            }

            // Create test player and building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = true;
            building.ProductionTask = new ProductionTask(contractMeta);
            var player = await Helpers.CreateTestPlayer(playerId, building: building);

            // Wait for contract to complete.
            Thread.Sleep(contractProductionTime);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
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

        [Fact]
        public async void RewardForNotCompletedContractNotCollected ()
        {
            var contractMetaId = "Test_RewardForNotCompletedContractNotCollected";
            var contractReward = new Reward() { Resources = new List<int> { 0, 0, 111 } };
            var contractProductionTime = TimeSpan.FromMinutes(10);
            var buildingMetaId = "Test_RewardForNotCompletedContractNotCollected";
            var playerId = "Test_RewardForNotCompletedContractNotCollected";

            // Create test contract meta if it doesn't exist.
            var contractMeta = await new ContractMeta(contractMetaId).LoadAsync();
            if (contractMeta == null)
            {
                contractMeta = new ContractMeta(contractMetaId);
                contractMeta.Rewards.Add(contractReward);
                contractMeta.ProductionTime = contractProductionTime;
                await contractMeta.InsertAsync();
            }

            // Create test building meta if it doesn't exist.
            var buildingMeta = await new BuildingMeta(buildingMetaId).LoadAsync();
            if (buildingMeta == null)
            {
                buildingMeta = new BuildingMeta(buildingMetaId);
                buildingMeta.StorageLimit = 1000;
                await buildingMeta.InsertAsync();
            }

            // Create test player and building.
            var building = new Building(buildingMetaId);
            building.IsConstructed = true;
            building.ProductionTask = new ProductionTask(contractMeta);
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
            var controller = new CheckProductionController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is ContractNotReady.
            Assert.Equal(responseToken["Status"], JStatus.NotReady.JToken["Status"]);

            // Make sure reward is not collected.
            var collectController = new CollectProductionRewardsController();
            responseToken = await collectController.HandleHttpRequestAsync(data);
            player = await player.LoadAsync();
            Assert.True(player.Resources.Count == 0);
        }
    }
}

