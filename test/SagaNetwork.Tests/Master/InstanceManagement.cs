using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class InstanceManagement : InitTest
    {
        [Fact]
        public async void DestroyedInstanceRemoved ()
        {
            var serverIp = "Test_DestroyedInstanceRemoved";
            var instancePort = "Test_DestroyedInstanceRemoved";
            var arenaMetaId = "Test_DestroyedInstanceRemoved";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                await arenaMeta.InsertAsync();
            }

            // Create test server with test instance.
            var instance = new ArenaInstance(arenaMetaId, instancePort);
            var server = await Helpers.CreateTestServer(serverIp, instance: instance);

            // Mock data.
            var data = JToken.Parse($@"{{
                'Ip':'{serverIp}',
                'Port':'{instancePort}', 
                {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var conrtoller = new OnInstanceDestroyedController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure instance was removed.
            server = await server.LoadAsync();
            Assert.True(server.ActiveInstances.Count == 0);
        }

        [Fact]
        public async void ReadyInstanceAdded ()
        {
            var serverIp = "Test_ReadyInstanceAdded";
            var instancePort = "Test_ReadyInstanceAdded";
            var arenaMetaId = "Test_ReadyInstanceAdded";

            // Create test arena meta if it doesn't exist.
            var arenaMeta = await new ArenaMeta(arenaMetaId).LoadAsync();
            if (arenaMeta == null)
            {
                arenaMeta = new ArenaMeta(arenaMetaId);
                await arenaMeta.InsertAsync();
            }

            // Create a test server.
            var server = await Helpers.CreateTestServer(serverIp);

            // Create a test requested instance.
            var requestedInstance = await Helpers.CreateTestRequestedInstance(arenaMetaId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{Configuration.AppSettings["ServerPlayerId"]}',
                'SessionToken':'{Configuration.AppSettings["ServerSessionToken"]}',
                'Ip':'{serverIp}',
                'Port':'{instancePort}',
                'ArenaMetaId':'{arenaMetaId}', 
                {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var conrtoller = new OnInstanceReadyController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure instance was added.
            server = await server.LoadAsync();
            Assert.True(server.ActiveInstances[0].MetaId == arenaMetaId && server.ActiveInstances[0].Port == instancePort);

            // Make sure requested instance was removed.
            requestedInstance = await requestedInstance.LoadAsync();
            Assert.Null(requestedInstance);
        }
    }
}


