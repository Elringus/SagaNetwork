using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class Authorization : InitTest
    {
        [Fact]
        public async void ValidPasswordAuthorized ()
        {
            var playerId = "Test_ValidPasswordAuthorized";
            var playerPassword = "Test_Password";

            // Create test player if it doesn't exist.
            await Helpers.CreateTestPlayer(playerId, playerPassword, false);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'Password':'{playerPassword}'
            }}");

            // Execute controller.
            var controller = new AuthPlayerController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);
        }

        [Fact]
        public async void InvalidPasswordNotAuthorized ()
        {
            var playerId = "Test_InvalidPasswordNotAuthorized";
            var validPassword = "Test_ValidPassword";
            var invalidPassword = "Test_InvalidPassword";

            // Create test player if it doesn't exist.
            await Helpers.CreateTestPlayer(playerId, validPassword, false);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'Password':'{invalidPassword}'
            }}");

            // Execute controller.
            var controller = new AuthPlayerController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is WrongPassword.
            Assert.Equal(responseToken["Status"], JStatus.WrongPassword.JToken["Status"]);
        }

        [Fact]
        public async void ValidSessionTokenAccepted ()
        {
            // Skipping test if auth is disabled.
            if (!SagaNetwork.Authorization.IsEnabled) return;

            var playerId = "Test_ValidSessionTokenAccepted";

            // Create test player if it doesn't exist.
            await Helpers.CreateTestPlayer(playerId, replace: false);

            // Acquiring a valid session token.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}'
            }}");

            // Execute controller.
            var controller = new CheckAuthController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is WrongPassword.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);
        }

        [Fact]
        public async void InvalidSessionTokenNotAccepted ()
        {
            // Skipping test if auth is disabled.
            if (!SagaNetwork.Authorization.IsEnabled) return;

            var playerId = "Test_InvalidSessionTokenNotAccepted";
            var invalidSessionToken = "Test_InvalidSessionToken";

            // Create test player if it doesn't exist.
            await Helpers.CreateTestPlayer(playerId, replace: false);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{invalidSessionToken}'
            }}");

            // Execute controller.
            var controller = new CheckAuthController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is AuthFail.
            Assert.Equal(responseToken["Status"], JStatus.AuthFail.JToken["Status"]);
        }
    }
}

