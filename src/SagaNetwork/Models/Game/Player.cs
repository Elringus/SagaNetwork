using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents a player account, contains all the data to define player state.
    /// </summary>
    [GenerateApi(UClassType.DbModel)]
    public class Player : TableModel<Player>
    {
        public string PasswordHash { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
        public City City { get; set; }
        public List<int> Resources { get; set; } = new List<int>(10);
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Character> Characters { get; set; } = new List<Character>();
        public int SelectedCharacterIndex { get; set; } = -1; // -1 means default char is not yeat selected
        public List<Citizen> Citizens { get; set; } = new List<Citizen>();
        public int CitizenSlots { get; set; } = 1;

        public override string BaseTableName => "Players"; 

        public Player () { }
        public Player (string id) : base(id) { }

        public bool CanAfford (List<int> resources)
        {
            if (resources == null) return true;
            if (Resources.Count < resources.Count) return false;

            for (int i = 0; i < resources.Count; i++)
                if (Resources[i] < resources[i]) return false;

            return true;
        }

        public void SpendResources (List<int> resources)
        {
            if (resources == null) return;
            if (!CanAfford(resources)) return;

            for (int i = 0; i < resources.Count; i++)
                Resources[i] -= resources[i];
        }

        public void AddResources (List<int> resources)
        {
            if (resources == null) return;

            for (int i = 0; i < resources.Count; i++)
            {
                if (Resources.Count > i) Resources[i] += resources[i];
                else Resources.Add(resources[i]);
            }
        }

        /// <summary>
        /// Returns currently selected character or null if none is selected.
        /// </summary>
        public Character GetSelectedCharacter () => SelectedCharacterIndex >= 0 ? Characters[SelectedCharacterIndex] : null;
        public Character GetCharacterByClass (string classMetaId) => Characters.Find(c => c.ClassMetaId == classMetaId);
        public Character GetCharacterById (string characterId) => Characters.Find(c => c.Id == characterId);

        public bool HasCharacterOfClass (string classMetaId) => Characters.Exists(c => c.ClassMetaId == classMetaId);

        public bool HasFreeCitizenSlot => CitizenSlots > Citizens.Count;
        public Citizen GetCitizenById (string citizenId) => Citizens.Find(c => c.Id == citizenId);

        public List<Building> GetAllConstructedBuildings ()
        {
            var buildings = new List<Building>();

            foreach (var ward in City.Wards)
                foreach (var spot in ward.BuildingSpots)
                    if (spot.Building != null && spot.Building.IsConstructed)
                        buildings.Add(spot.Building);

            return buildings;
        }

        public List<string> GetCurrentlyProducedContractMetaIds ()
        {
            return GetAllConstructedBuildings().Where(constructedBuilding => constructedBuilding.ProductionTask != null)
                    .Select(producingBuilding => producingBuilding.ProductionTask.ProducedContractMetaId).ToList();
        }

        public List<string> GetAllItemMetaIds ()
        {
            var itemsIds = new List<string>();

            foreach (var item in Items)
                itemsIds.Add(item.MetaId);

            return itemsIds;
        }

        /// <summary>
        /// Adds an item to the players' Items list.
        /// </summary>
        public bool AddItem (Item itemToAdd)
        {
            if (Items.Exists(item => item.MetaId == itemToAdd.MetaId)) // Currently can only have one unique item.
                return false;

            Items.Add(itemToAdd);

            return true;
        }

        public bool HasItemOfMeta (string itemMetaId) => GetAllItemMetaIds().Contains(itemMetaId);

        public Item GetItemById (string itemId) => Items.Find(item => item.Id == itemId);

        /// <summary>
        /// Attempts to equip an item to specified character. 
        /// If an item with the same slot is already equiped -- swaps the items.
        /// </summary>
        /// <returns>If the item was equiped.</returns>
        public async Task<bool> EquipItemAsync (Item item, Character character)
        {
            if (item.IsOwnedByCharacter) return false; // Can't equip item that is already used by other character.

            var equippingItemMeta = await item.GetMetaAsync();
            if (equippingItemMeta == null) return false;

            if (equippingItemMeta.ClassMetaId != character.ClassMetaId)
                return false;

            var equipedItem = await GetEquipedItemForSlotAsync(character, equippingItemMeta.EquipmentSlot);
            if (equipedItem != null)
                equipedItem.SetStorage(item.StorageId);

            item.SetOwningCharacter(character.Id);

            return true;
        }

        public List<Item> GetEquipedItemsForCharacter (Character character)
        {
            var equipedItems = new List<Item>();

            foreach (var item in Items)
                if (item.StorageId == character.Id)
                    equipedItems.Add(item);

            return equipedItems;
        }

        public async Task<Item> GetEquipedItemForSlotAsync (Character character, string slot)
        {
            foreach (var item in GetEquipedItemsForCharacter(character))
            {
                var itemMeta = await new ItemMeta(item.MetaId).LoadAsync();
                if (itemMeta.EquipmentSlot == slot) return item;
            }

            return null;
        }

        public Building GetBuildingById (string buildingId)
        {
            foreach (var ward in City.Wards)
                foreach (var spot in ward.BuildingSpots)
                    if (spot.Building != null && spot.Building.Id == buildingId)
                        return spot.Building;

            return null;
        }

        public BuildingSpot GetBuildingSpotById (string buildingSpotId)
        {
            foreach (var ward in City.Wards)
                foreach (var spot in ward.BuildingSpots)
                    if (spot.Id == buildingSpotId) return spot;

            return null;
        }

        public Ward GetWardById (string wardId) => City.Wards.Find(ward => ward.Id == wardId);
    }
}
