using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class ServerManagement : InitTest
    {
        [Fact]
        public async void RegisteredServerIsAdded ()
        {
            var serverIp = "Test_RegisteredServerIsAdded";

            // Mock data.
            var data = JToken.Parse($@"{{
                'Ip':'{serverIp}', 
                {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var conrtoller = new RegisterGameServerController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure server was added.
            var server = await new GameServer(serverIp).LoadAsync();
            Assert.NotNull(server);
        }

    }
}


