using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SagaNetwork.Models;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]")]
    public class GetJsonTextController : BaseController
    {
        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var jsonBlobId = data.ParseField<string>("Id");
            if (jsonBlobId == null) return JStatus.RequestArgumentNotFound("Id");

            var jsonBlob = await new JsonBlob(jsonBlobId).LoadAsync();
            if (jsonBlob == null) return JStatus.NotFound;

            var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
            if (!await blockBlob.ExistsAsync()) return JStatus.NotFound;

            var jsonText = await blockBlob.DownloadTextAsync();

            var response = JStatus.Ok.ToObject<JObject>();
            response.Add("JsonText", jsonText);

            return response;
        }
    }
}
