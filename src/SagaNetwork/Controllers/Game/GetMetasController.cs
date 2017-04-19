using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]")]
    public class GetMetasController : BaseController
    {
        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var modelName = data.ParseField<string>("ModelName");
            if (modelName == null) return JStatus.RequestArgumentNotFound("ModelName");

            var modelType = Type.GetType($"SagaNetwork.Models.{modelName}");
            if (modelType == null) return JStatus.MetaNotFound;

            dynamic model = Activator.CreateInstance(modelType);

            var metas = await model.RetrieveAllAsync();

            var response = JStatus.Ok.ToObject<JObject>();
            response.Add("Metas", JToken.FromObject(metas));

            return response;
        }
    }
}
