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
    public class WardsUnlocking : InitTest
    {
        [Fact]
        public async void UnlockingStarted ()
        {
            var wardMetaId = "Test_UnlockingStarted";
            var playerId = "Test_UnlockingStarted";

            // Create test ward meta if it doesn't exist.
            var wardMeta = await new WardMeta(wardMetaId).LoadAsync();
            if (wardMeta == null)
            {
                wardMeta = new WardMeta(wardMetaId);
                await wardMeta.InsertAsync();
            }

            // Create test player and ward.
            var ward = new Ward(wardMetaId);
            var player = await Helpers.CreateTestPlayer(playerId, ward: ward);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'WardId':'{ward.Id}',
            }}");

            // Execute controller.
            var controller = new StartWardUnlockingController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure unlock is started.
            player = await player.LoadAsync();
            Assert.False(player.GetWardById(ward.Id).IsUnlocked);
            Assert.True(player.GetWardById(ward.Id)?.UnlockTask != null);
        }

        [Fact]
        public async void UnlockingFinished ()
        {
            var wardMetaId = "Test_UnlockingFinished";
            var unlockTime = TimeSpan.FromMilliseconds(500);
            var playerId = "Test_UnlockingFinished";

            // Create test builidng meta if it doesn't exist.
            var wardMeta = await new WardMeta(wardMetaId).LoadAsync();
            if (wardMeta == null)
            {
                wardMeta = new WardMeta(wardMetaId);
                wardMeta.UnlockTime = unlockTime;
                wardMeta.UnlockReward = new Reward() { Resources = new List<int> { 100 }, RandomCitizens = 1 };
                await wardMeta.InsertAsync();
            }

            // Create test player and ward.
            var ward = new Ward(wardMetaId);
            ward.UnlockTask = new TimeTask(wardMeta.UnlockTime);
            var player = await Helpers.CreateTestPlayer(playerId, ward: ward);

            // Wait for ward to unlock.
            Thread.Sleep(unlockTime);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'WardId':'{ward.Id}'
            }}");

            // Execute controller.
            var controller = new FinishWardUnlockingController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure building is constructed.
            player = await player.LoadAsync();
            Assert.True(player.GetWardById(ward.Id).IsUnlocked);

            // Make sure reward is awarded.
            Assert.True(player.Resources.Count > 0);
            Assert.False(player.HasFreeCitizenSlot);
        }

        [Fact]
        public async void UnlockingNotReady ()
        {
            var wardMetaId = "Test_UnlockingNotReady";
            var unlockTime = TimeSpan.FromMinutes(10);
            var playerId = "Test_UnlockingNotReady";

            // Create test builidng meta if it doesn't exist.
            var wardMeta = await new WardMeta(wardMetaId).LoadAsync();
            if (wardMeta == null)
            {
                wardMeta = new WardMeta(wardMetaId);
                wardMeta.UnlockTime = unlockTime;
                await wardMeta.InsertAsync();
            }

            // Create test player and ward.
            var ward = new Ward(wardMetaId);
            ward.UnlockTask = new TimeTask(wardMeta.UnlockTime);
            var player = await Helpers.CreateTestPlayer(playerId, ward: ward);

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'WardId':'{ward.Id}'
            }}");

            // Execute controller.
            var controller = new FinishWardUnlockingController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is NotReady.
            Assert.Equal(responseToken["Status"], JStatus.NotReady.JToken["Status"]);

            // Make sure ward is not unlocked.
            player = await player.LoadAsync();
            Assert.False(player.GetWardById(ward.Id).IsUnlocked);
        }
    }
}

