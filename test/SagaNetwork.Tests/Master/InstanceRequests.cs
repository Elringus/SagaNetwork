using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class InstanceRequests : InitTest
    {
        [Fact]
        public async void RequisitesForAvailableInstanceProvided ()
        {
            var playerId = "Test_RequisitesForAvailableInstanceProvided";
            var serverIp = "Test_RequisitesForAvailableInstanceProvided";
            var instancePort = "Test_RequisitesForAvailableInstanceProvided";
            var arenaMetaId = "Test_RequisitesForAvailableInstanceProvided";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                arenaMeta.MaxPlayers = 10;
                await arenaMeta.InsertAsync();
            }

            // Create test server with test instance.
            var instance = new ArenaInstance(arenaMetaId, instancePort);
            var server = await Helpers.CreateTestServer(serverIp, instance: instance);

            // Create and auth test player.
            await Helpers.CreateTestPlayer(playerId);
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'ArenaMetaId':'{arenaMetaId}'
            }}");

            // Execute controller.
            var conrtoller = new RequestInstanceController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Assert returned IP and port are valid.
            Assert.Equal(responseToken["Ip"], serverIp);
            Assert.Equal(responseToken["Port"], instancePort);

            // Make sure instance free slot was removed.
            server = await server.LoadAsync();
            Assert.True(server.ActiveInstances[0].FreeSlots == 9);
        }

        [Fact]
        public async void UnavailableInstanceRequested ()
        {
            var playerId = "Test_UnavailableInstanceRequested";
            var arenaMetaId = "Test_UnavailableInstanceRequested";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                await arenaMeta.InsertAsync();
            }

            // Create and auth test player.
            await Helpers.CreateTestPlayer(playerId);
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'ArenaMetaId':'{arenaMetaId}'
            }}");

            // Execute controller.
            var conrtoller = new RequestInstanceController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is Wait.
            Assert.Equal(responseToken["Status"], JStatus.Wait.JToken["Status"]);

            // Make sure instance is requested.
            var requestedInstance = await new RequestedInstance(arenaMetaId).LoadAsync();
            Assert.NotNull(requestedInstance);
        }

        [Fact]
        public async void FullInstanceRequestedAgain ()
        {
            var playerId = "Test_FullInstanceRequestedAgain";
            var serverIp = "Test_FullInstanceRequestedAgain";
            var instancePort = "Test_FullInstanceRequestedAgain";
            var arenaMetaId = "Test_FullInstanceRequestedAgain";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                arenaMeta.MaxPlayers = 10;
                await arenaMeta.InsertAsync();
            }

            // Create test server with test instance without free slots.
            var instance = new ArenaInstance(arenaMetaId, instancePort);
            instance.FreeSlots = 0;
            var server = await Helpers.CreateTestServer(serverIp, instance: instance);

            // Create and auth test player.
            await Helpers.CreateTestPlayer(playerId);
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'ArenaMetaId':'{arenaMetaId}'
            }}");

            // Execute controller.
            var conrtoller = new RequestInstanceController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is Wait.
            Assert.Equal(responseToken["Status"], JStatus.Wait.JToken["Status"]);

            // Make sure new instance is requested.
            var requestedInstance = await new RequestedInstance(arenaMetaId).LoadAsync();
            Assert.NotNull(requestedInstance);
        }

        [Fact]
        public async void ClosedInstanceWontAcceptJoinRequests ()
        {
            var playerId = "Test_ClosedInstanceWontAcceptJoinRequests";
            var arenaMetaId = "Test_ClosedInstanceWontAcceptJoinRequests";
            var serverIp = "Test_ClosedInstanceWontAcceptJoinRequests";
            var instancePort = "Test_ClosedInstanceWontAcceptJoinRequests";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                await arenaMeta.InsertAsync();
            }

            // Create test server with a closed instance.
            var instance = new ArenaInstance(arenaMetaId, instancePort);
            instance.FreeSlots = 2;
            instance.IsOpen = false;
            var server = await Helpers.CreateTestServer(serverIp, instance: instance);

            // Create and auth test player.
            await Helpers.CreateTestPlayer(playerId);
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'ArenaMetaId':'{arenaMetaId}'
            }}");

            // Execute controller.
            var conrtoller = new RequestInstanceController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is Wait.
            Assert.Equal(responseToken["Status"], JStatus.Wait.JToken["Status"]);

            // Make sure a new instance is requested.
            var requestedInstance = await new RequestedInstance(arenaMetaId).LoadAsync();
            Assert.NotNull(requestedInstance);
        }
    }
}


