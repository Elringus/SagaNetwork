using SagaNetwork;
using SagaNetwork.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaNetwork.Tests
{
    public static class Helpers
    {
        public static async Task<Player> CreateTestPlayer (Player player, bool replace)
        {
			player.CreationDate = DateTime.UtcNow;

            var existingPlayer = await new Player(player.Id).LoadAsync();

            if (replace)
            {
                if (existingPlayer != null) await existingPlayer.DeleteAsync();
                await player.InsertAsync();
            }
            else if (existingPlayer == null)
                await player.InsertAsync();

            return player;
        }

        public static async Task<Player> CreateTestPlayer (string playerId, string playerPassword = "Test_Password", 
            bool replace = true, Building building = null, BuildingSpot buildingSpot = null, Ward ward = null, City city = null, Character character = null)
        {
            var player = new Player(playerId);
            player.PasswordHash = PasswordHash.CreateHash(playerPassword);

            if (city == null)
                city = new City("Test_CityId");
			player.City = city;

            if (ward == null)
                ward = new Ward("Test_WardId");
            city.Wards.Add(ward);

            if (buildingSpot == null)
                buildingSpot = new BuildingSpot("Test_BuildingSpotId");
            city.Wards[0].BuildingSpots.Add(buildingSpot);

            if (building == null && buildingSpot.MetaId == "Test_BuildingSpotId")
                building = new Building("Test_BuildingId");
            buildingSpot.Building = building;

            if (character != null)
                player.Characters.Add(character);

            return await CreateTestPlayer(player, replace);
        }

        public static async Task<GameServer> CreateTestServer (GameServer server, bool replace)
        {
            var existingServer = await new GameServer(server.Ip).LoadAsync();

            if (replace)
            {
                if (existingServer != null) await existingServer.DeleteAsync();
                await server.InsertAsync();
            }
            else if (existingServer == null)
                await server.InsertAsync();

            return server;
        }

        public static async Task<GameServer> CreateTestServer (string serverIp, bool replace = true, 
            ArenaInstance instance = null, List<ArenaInstance> instances = null)
        {
            var server = new GameServer(serverIp);
            if (instances != null) server.ActiveInstances = instances;
            if (instance != null) server.ActiveInstances.Add(instance);

            return await CreateTestServer(server, replace);
        }

        public static async Task<RequestedInstance> CreateTestRequestedInstance (RequestedInstance requestedInstance, bool replace)
        {
            var existingRequestedInstance = await new RequestedInstance(requestedInstance.ArenaMetaId).LoadAsync();

            if (replace)
            {
                if (existingRequestedInstance != null) await existingRequestedInstance.DeleteAsync();
                await requestedInstance.InsertAsync();
            }
            else if (existingRequestedInstance == null)
                await requestedInstance.InsertAsync();

            return requestedInstance;
        }

        public static async Task<RequestedInstance> CreateTestRequestedInstance (string arenaMetaId, bool replace = true)
        {
            var requestedInstance = new RequestedInstance(arenaMetaId);

            return await CreateTestRequestedInstance(requestedInstance, replace);
        }

        public static string GetSessionToken (string playerId) => SagaNetwork.Authorization.AddTokenForPlayer(playerId);

        public static string JsonServerCredentials => $"'ServerAuthKey':'{Configuration.AppSettings["ServerAuthKey"]}'";

        public static string ServerAuthKey => Configuration.AppSettings["ServerAuthKey"];
    }
}
