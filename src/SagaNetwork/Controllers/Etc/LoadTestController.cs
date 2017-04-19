using SagaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// Used for load testing. Approximates average controller behaviour.
    /// </summary>
    [Route("api/[controller]")]
    public class LoadTestController : BaseController
    {
        protected override async Task<JToken> ExecuteController (JToken data)
        {
            var randPlayerIndex = StaticRandom.Next(0, 470000).ToString();
            var player = await new Player($"TestPlayer{randPlayerIndex}").LoadAsync();

            //foreach (var character in player.Characters)
            //    await new ClassMeta(character.ClassMetaId).LoadAsync();

            ///////////////////////////////////////////////

            //var randClassIndex = StaticRandom.Next(0, player.Characters.Count);
            //var classMeta = await new ClassMeta(player.Characters[randClassIndex].ClassMetaId).LoadAsync();

            //if (classMeta.Requirement.IsFulfilledByPlayer(player))
            //    player.AddResources(new List<int> { 1, 0, 1, 0, 1 });

            //if (StaticRandom.Bool) await player.GetCharacterByClass(classMeta.Id).EarnExperience(StaticRandom.Next(100, 10000));
            //else
            //{
            //    player.GetCharacterByClass(classMeta.Id).Experience = 0;
            //    player.GetCharacterByClass(classMeta.Id).Level = 0;
            //}

            ///////////////////////////////////////////////

            if (StaticRandom.Bool) player.AddResources(new List<int> { 0, 1, 0, 1, 0 });
            else player.SpendResources(new List<int> { 0, 1, 0, 1, 0 });

            await player.ReplaceAsync();

            return JStatus.Ok; 
        }
    }
}
