using Xunit;
using SagaNetwork;
using SagaNetwork.Models;
using SagaNetwork.Controllers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SagaNetwork.Tests
{
    public class CharacterEquipment : InitTest
    {
        [Fact]
        public async void AvailableItemEquipped ()
        {
            var itemMetaId = "Test_AvailableItemEquipped";
            var itemSlot = "Test_AvailableItemEquipped";
            var classMetaId = "Test_AvailableItemEquipped";
            var playerId = "Test_AvailableItemEquipped";

            // Create test class meta if it doesn't exist.
            var classMeta = await new ClassMeta(classMetaId).LoadAsync();
            if (classMeta == null)
            {
                classMeta = new ClassMeta(classMetaId);
                await classMeta.InsertAsync();
            }

            // Create test item meta if it doesn't exist.
            var itemMeta = await new ItemMeta(itemMetaId).LoadAsync();
            if (itemMeta == null)
            {
                itemMeta = new ItemMeta(itemMetaId);
                itemMeta.ClassMetaId = classMetaId;
                itemMeta.EquipmentSlot = itemSlot;
                await itemMeta.InsertAsync();
            }

            // Create test player with a character and item.
            var player = await Helpers.CreateTestPlayer(playerId);
			var character = new Character(classMetaId);
			player.Characters.Add(character);
			var item = new Item(itemMetaId);
			player.AddItem(item);
            await player.ReplaceAsync();

            // Auth player.
            var sessionToken = Helpers.GetSessionToken(playerId);

            // Mock data.
            var data = JToken.Parse($@"{{
                'PlayerId':'{playerId}',
                'SessionToken':'{sessionToken}',
                'CharacterId':'{character.Id}',
                'ItemId':'{item.Id}'
            }}");

            // Execute controller.
            var controller = new EquipItemController();
            var responseToken = await controller.HandleHttpRequestAsync(data);

            // Assert controller response status is OK.
            Assert.Equal(responseToken["Status"], JStatus.Ok.JToken["Status"]);

            // Make sure item is equiped.
            player = await player.LoadAsync();
            Assert.True(player.GetItemById(item.Id).IsOwnedByCharacter);
			Assert.True(player.GetItemById(item.Id).OwningCharacterId == character.Id);
		}
    }
}

