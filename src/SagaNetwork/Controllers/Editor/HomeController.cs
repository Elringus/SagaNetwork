using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SagaNetwork.Models;
using System.Linq;

namespace SagaNetwork.Controllers
{
    public class HomeController : BaseEditorController<GlobalConfiguration>
    {
        public async Task<IActionResult> Index ()
        {
            var servers = await new GameServer().RetrieveAllAsync();
            var queuedInstances = await new RequestedInstance().RetrieveAllAsync();

            ViewBag.WorkingServers = servers.FindAll(gameServer => !gameServer.IsUpdating).Count;
            ViewBag.UpdatingServers = servers.Count - ViewBag.WorkingServers;
            ViewBag.ActiveInstances = servers.Sum(gameServer => gameServer.ActiveInstances.Count);
            ViewBag.QueuedInstances = queuedInstances.Count;

            return View();
        }

        public async Task<IActionResult> SetIsServiceOnline (bool isOnline)
        {
            var globalConfig = await new GlobalConfiguration().LoadAsync();

            globalConfig.IsServiceOnline = isOnline;
            if (!await globalConfig.ReplaceAsync())
                return ErrorView("Global configuration update failed. Please try again.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SetIsUtilityOperationsAllowed (bool isAllowed)
        {
            var globalConfig = await new GlobalConfiguration().LoadAsync();

            globalConfig.IsUtilityOperationsAllowed = isAllowed;
            if (!await globalConfig.ReplaceAsync())
                return ErrorView("Global configuration update failed. Please try again.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SetIsAuthEnabled (bool isEnabled)
        {
            var globalConfig = await new GlobalConfiguration().LoadAsync();

            globalConfig.IsAuthEnabled = isEnabled;
            if (!await globalConfig.ReplaceAsync())
                return ErrorView("Global configuration update failed. Please try again.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SetIsAccessKeysEnabled (bool isEnabled)
        {
            var globalConfig = await new GlobalConfiguration().LoadAsync();

            globalConfig.IsAccessKeysEnabled = isEnabled;
            if (!await globalConfig.ReplaceAsync())
                return ErrorView("Global configuration update failed. Please try again.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SetBuildVersion (string buildVersion)
        {
            var globalConfig = await new GlobalConfiguration().LoadAsync();

            globalConfig.BuildVersion = buildVersion;
            if (!await globalConfig.ReplaceAsync())
                return ErrorView("Global configuration update failed. Please try again.");

            return RedirectToAction("Index");
        }
    }
}
