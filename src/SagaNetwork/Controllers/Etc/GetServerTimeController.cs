using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), GenerateApi(UClassType.Request)]
    public class GetServerTimeController : BaseController
    {
        [ApiResponseData] public DateTime ServerTime;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            ServerTime = DateTime.UtcNow;
            return await Task.FromResult(JStatus.Ok);
        }
    }
}
