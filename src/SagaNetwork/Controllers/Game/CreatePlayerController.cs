using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SagaNetwork.Controllers
{
    [Route("api/[controller]"), GenerateApi(UClassType.Request)]
    public class CreatePlayerController : BaseController
    {
        [ApiRequestData] public string PlayerId;
        [ApiRequestData] public string Password;

        protected override async Task<JToken> ExecuteController (JToken data)
        {
            if (Configuration.IsAccessKeysEnabled)
            {
                var accessKey = await new AccessKey(Password).LoadAsync();
                if (accessKey == null || accessKey.IsActivated)
                    return JStatus.InvalidAccessKey;
                accessKey.Activate();
            }

            var newPlayer = new Player(PlayerId);
            if (await newPlayer.LoadAsync() != null)
                return JStatus.PlayerAlreadyExists;

            newPlayer.CreationDate = DateTime.UtcNow;
            newPlayer.PasswordHash = PasswordHash.CreateHash(Password);
            newPlayer.City = await CreateSagaCityWithWardsAndSpots();
            newPlayer.Characters = await CreateDefaultCharacters(newPlayer);
            newPlayer.SelectedCharacterIndex = -1;
            newPlayer.Resources = new List<int> { 200, 200, 200 };

            await newPlayer.InsertAsync();

            return JStatus.Ok;
        }

        /// <summary>
        /// Temp hack while we have only one city.
        /// Creates a default city with wards and spots based on the 'SagaCity' meta.
        /// </summary>
        private async Task<City> CreateSagaCityWithWardsAndSpots ()
        {
            // Create 'Saga' city meta if it doesn't already exist.
            var sagaCityMetaId = "SagaCity";
            var sagaCityMeta = await new CityMeta(sagaCityMetaId).LoadAsync();
            if (sagaCityMeta == null)
            {
                sagaCityMeta = new CityMeta(sagaCityMetaId);
                await sagaCityMeta.InsertAsync();
            }

            var city = new City(sagaCityMetaId);
            var cityMeta = await city.GetMetaAsync();

            foreach (var wardMetaId in cityMeta.WardMetaIds)
            {
                var wardMeta = await new WardMeta(wardMetaId).LoadAsync();
                if (wardMeta == null) continue;
                var ward = new Ward(wardMetaId);
                if (wardMeta.IsInitiallyUnlocked) ward.IsUnlocked = true;

                foreach (var buildingSpotAddress in wardMeta.BuildingSpotsMap)
                {
                    var newBuildingSpot = new BuildingSpot(buildingSpotAddress.Value);
                    newBuildingSpot.Address = buildingSpotAddress.Key;
                    ward.BuildingSpots.Add(newBuildingSpot);
                }

                city.Wards.Add(ward);
            }

            return city;
        }

        /// <summary>
        /// Creates a list with default character presets based on defined class metas.
        /// </summary>
        private async Task<List<Character>> CreateDefaultCharacters (Player player)
        {
            var defaultCharacters = new List<Character>();
            var classMetas = await new ClassMeta().RetrieveAllAsync();

            foreach (var classMeta in classMetas.Where(classMeta => classMeta.IsInitiallyAvailable))
            {
                var character = new Character(classMeta.Id);
                character.TalentPoints = 10; // TODO: remove or add initial talent points to the class meta 

                foreach (var defaultItemMetaId in classMeta.DefaultItems)
                {
                    var defaultItem = new Item(defaultItemMetaId);
                    defaultItem.SetOwningCharacter(character.Id);
                    player.AddItem(defaultItem);
                }

                defaultCharacters.Add(character);
            }

            return defaultCharacters;
        }
    }
}
