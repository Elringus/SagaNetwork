using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;

namespace SagaNetwork.Controllers
{
    public class JsonBlobsEditorController : BaseEditorController<JsonBlob>
    {
        [HttpPost]
        public async Task<IActionResult> UploadJsonBlobs (ICollection<IFormFile> files)
        {
            var issues = new List<string>();

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (!CheckIfJsonFileIsValid(file, issues)) continue;

                var fileName = file.GetFileName();
                var entityId = fileName.Replace(".json", string.Empty);
                var jsonBlob = new JsonBlob(entityId);
                if (!await jsonBlob.InsertAsync())
                {
                    issues.Add($"File: {fileName}  Issue: JsonBlob with ID {entityId} already exists. You may overwrite it on the edit entity page.");
                }
                else
                {
                    var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
                    using (var fileStream = file.OpenReadStream())
                        blockBlob.UploadFromStream(fileStream); // not working with async
                }
            }

            if (issues.Count > 0)
                return JsonUploadErrorView(issues);

            return RedirectToAction("Browse");
        }

        [HttpPost]
        public async Task<IActionResult> OverwriteJsonBlob (IFormFile file, string entityId)
        {
            var jsonBlob = await new JsonBlob(entityId).LoadAsync();
            if (jsonBlob == null)
                return ErrorView($"JsonBlob with ID {entityId} not found.");

            var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
            if (!await blockBlob.ExistsAsync())
                return ErrorView($"Blob file {jsonBlob.BlobPath} not found.");

            var issues = new List<string>();
            if (!CheckIfJsonFileIsValid(file, issues))
                return JsonUploadErrorView(issues);

            using (var fileStream = file.OpenReadStream())
                blockBlob.UploadFromStream(fileStream);  // not working with async

            return RedirectToAction("Edit", new { entityId = jsonBlob.Id });
        }

        public override async Task<IActionResult> Edit (string entityId)
        {
            var jsonBlob = await new JsonBlob(entityId).LoadAsync();
            if (jsonBlob == null)
                return ErrorView($"JsonBlob with ID {entityId} not found.");

            var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
            if (!await blockBlob.ExistsAsync())
                return ErrorView($"Blob file {jsonBlob.BlobPath} not found.");

            ViewBag.JsonText = await blockBlob.DownloadTextAsync();

            return await base.Edit(entityId);
        }

        public override async Task<IActionResult> Delete (string entityId)
        {
            var jsonBlob = await new JsonBlob(entityId).LoadAsync();
            if (jsonBlob == null)
                return ErrorView($"JsonBlob with ID {entityId} not found.");

            var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
            if (!await blockBlob.ExistsAsync())
                return ErrorView($"Blob file {jsonBlob.BlobPath} not found.");

            await blockBlob.DeleteAsync();

            return await base.Delete(entityId);
        }

        public override Task<IActionResult> Duplicate (string sourceEntityId, string newEntityId)
        {
            return Task.FromResult<IActionResult>(ErrorView("JsonBlobs duplication is not implemented."));
        }

        public override async Task<IActionResult> Save (JsonBlob entity)
        {
            if (!entity.JsonText.IsValidJson())
                return ErrorView($"JSON validation failed.");

            var jsonBlob = await new JsonBlob(entity.Id).LoadAsync();
            if (jsonBlob == null)
                return ErrorView($"JsonBlob with ID {entity.Id} not found.");

            var blockBlob = CloudStorage.BlobContainer.GetBlockBlobReference(jsonBlob.BlobPath);
            if (!await blockBlob.ExistsAsync())
                return ErrorView($"Blob file {jsonBlob.BlobPath} not found.");

            await blockBlob.UploadTextAsync(entity.JsonText);

            return await base.Save(entity);
        }

        private bool CheckIfJsonFileIsValid (IFormFile jsonFile, List<string> validationIssues = null)
        {
            var fileName = jsonFile.GetFileName();

            if (fileName == null)
            {
                validationIssues?.Add("Issue: Failed to evaluate file name.");
                return false;
            }

            if (!fileName.Contains(".json"))
            {
                validationIssues?.Add($"File: {fileName}  Issue: Wrong file format.");
                return false;
            }

            if (!fileName.Replace(".json", string.Empty).All(char.IsLetter))
            {
                validationIssues?.Add($"File: {fileName}  Issue: Wrong file name (only letters are allowed).");
                return false;
            }

            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                var jsonString = reader.ReadToEnd();
                if (!jsonString.IsValidJson())
                {
                    validationIssues?.Add($"File: {fileName}  Issue: JSON validation failed.");
                    return false;
                }
            }

            return true;
        }
        
        private ViewResult JsonUploadErrorView (List<string> issues)
        {
            var errorMessage = $@"Some files were not uploaded:<br />";
            foreach (var issue in issues)
                errorMessage += $@" - {issue}<br />";
            return ErrorView(errorMessage);
        }
    }
}
