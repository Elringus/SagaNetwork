using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    public class EditorAttributeMetasEditorController : BaseEditorController<EditorAttributeMeta>
    {
        public async Task<IActionResult> AddPredefinedKey (string metaId)
        {
            var attributeMeta = await new EditorAttributeMeta(metaId).LoadAsync();

            if (attributeMeta == null)
                return ErrorView();

            attributeMeta.PredefinedKeys.Add("NULL");
            if (!await attributeMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = attributeMeta.Id });
        }

        public async Task<IActionResult> RemovePredefinedKey (string metaId, int index)
        {
            var attributeMeta = await new EditorAttributeMeta(metaId).LoadAsync();

            if (attributeMeta == null || attributeMeta.PredefinedKeys == null ||
                attributeMeta.PredefinedKeys.Count <= index)
                return ErrorView();

            attributeMeta.PredefinedKeys.RemoveAt(index);
            if (!await attributeMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = attributeMeta.Id });
        }

        public async Task<IActionResult> AddPredefinedValue (string metaId)
        {
            var attributeMeta = await new EditorAttributeMeta(metaId).LoadAsync();

            if (attributeMeta == null)
                return ErrorView();

            attributeMeta.PredefinedValues.Add("NULL");
            if (!await attributeMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = attributeMeta.Id });
        }

        public async Task<IActionResult> RemovePredefinedValue (string metaId, int index)
        {
            var attributeMeta = await new EditorAttributeMeta(metaId).LoadAsync();

            if (attributeMeta == null || attributeMeta.PredefinedValues == null ||
                attributeMeta.PredefinedValues.Count <= index)
                return ErrorView();

            attributeMeta.PredefinedValues.RemoveAt(index);
            if (!await attributeMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = attributeMeta.Id });
        }
    }
}
