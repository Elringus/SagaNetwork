using System;
using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]")]
    public class TestController : BaseController
    {
        [HttpGet]
        public string Get()
        {

            return Environment.ExpandEnvironmentVariables("%WEBSITE_SITE_NAME%");
        }

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var player = await new Player("TestPlayer").LoadAsync();
            player.LastLoginDate = DateTime.UtcNow;

            return JStatus.Ok;
        }
    }
}
