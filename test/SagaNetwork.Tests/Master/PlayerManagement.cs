using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class PlayerManagement : InitTest
    {
        [Fact]
        public async void SlotOfDisconnectedPlayerIsFree ()
        {
            var serverIp = "Test_SlotOfDisconnectedPlayerIsFree";
            var instancePort = "Test_SlotOfDisconnectedPlayerIsFree";
            var arenaMetaId = "Test_SlotOfDisconnectedPlayerIsFree";

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
            instance.FreeSlots--;
            var server = await Helpers.CreateTestServer(serverIp, instance: instance);

            // Mock data.
            var data = JToken.Parse($@"{{
                {Helpers.JsonServerCredentials},
                'Ip':'{serverIp}',
                'Port':'{instancePort}'
            }}");

            // Execute controller.
            var conrtoller = new OnPlayerDisconnectedController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure slot was freed.
            server = await server.LoadAsync();
            Assert.True(server.ActiveInstances[0].FreeSlots == 10);
        }

    }
}


