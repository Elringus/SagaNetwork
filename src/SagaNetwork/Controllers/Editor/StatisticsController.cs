using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SagaNetwork.Models;
using System;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// A controller for getting various DB stats.
    /// </summary>
    [Authorize]
    public class StatisticsController : Controller
    {
        public IActionResult Index ()
        {
            return View();
        }

        public async Task<string> Users ()
        {
            var players = await new Player().RetrieveAllAsync(9999);

            var output = $"PlayerID,CreationDate,LastLoginDate{Environment.NewLine}";

            foreach (var player in players)
                output += $"\"{player.Id}\",\"{player.CreationDate}\",\"{player.LastLoginDate}\"{Environment.NewLine}";

            return output;
        }
    }
}
