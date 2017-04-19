using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using System;
using System.Linq;

namespace SagaNetwork.Controllers
{
    public class WardMetasEditorController : BaseEditorController<WardMeta>
    {
        public override async Task<IActionResult> Edit (string entityId)
        {
            var buildingSpotMetas = await new BuildingSpotMeta().RetrieveAllAsync();
            ViewData.Add("AvailableBuildingSpotMetas", buildingSpotMetas.Select(spot => spot.Id).ToList());

            return await base.Edit(entityId);
        }

        public async Task<IActionResult> AddBuildingSpot (string wardMetaId)
        {
            var wardMeta = await new WardMeta(wardMetaId).LoadAsync();

            if (wardMeta == null)
                return ErrorView();

            wardMeta.BuildingSpotsMap.Add($"NULL#{Guid.NewGuid()}", "NULL");
            if (!await wardMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = wardMeta.Id });
        }

        public async Task<IActionResult> RemoveBuildingSpot (string wardMetaId, int buildingSpotIndex)
        {
            var wardMeta = await new WardMeta(wardMetaId).LoadAsync();

            if (wardMeta == null || wardMeta.BuildingSpotsMap == null ||
                wardMeta.BuildingSpotsMap.Count <= buildingSpotIndex)
                return ErrorView();

            wardMeta.BuildingSpotsMap.Remove(wardMeta.BuildingSpotsMap.ElementAt(buildingSpotIndex).Key);
            if (!await wardMeta.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = wardMeta.Id });
        }
    }
}
