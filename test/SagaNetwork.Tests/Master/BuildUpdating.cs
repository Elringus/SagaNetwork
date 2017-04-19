using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class BuildUpdating : InitTest
    {
        // FIXME: failing on initial run under parallel tests.
        //[Fact]
        public async void UpdateBuildRequestServed ()
        {
            var serverIp = "Test_UpdateBuildRequestServed";
            var buildUri = "Test_UpdateBuildRequestServed";

            // Create test server.
            var server = await Helpers.CreateTestServer(serverIp);

            // Mock data.
            var data = JToken.Parse($@"{{
                'BuildUri':'{buildUri}', 
                {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var conrtoller = new UpdateBuildController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure server is set to updating.
            server = await server.LoadAsync();
            Assert.True(server.IsUpdating);
        }

        [Fact]
        public async void UpdateCompleteRequestServed ()
        {
            var serverIp = "Test_UpdateCompleteRequestServed";

            // Create test server with updating status.
            var server = new GameServer(serverIp);
            server.IsUpdating = true;
            await Helpers.CreateTestServer(server, true);

            // Mock data.
            var data = JToken.Parse($@"{{
                'Ip':'{serverIp}', 
                {Helpers.JsonServerCredentials}
            }}");

            // Execute controller.
            var conrtoller = new UpdateCompleteController();
            var responseToken = await conrtoller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure server is set to not updating.
            server = await server.LoadAsync();
            Assert.False(server.IsUpdating);
        }

    }
}



