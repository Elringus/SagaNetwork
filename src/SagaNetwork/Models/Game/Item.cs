
namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaDescribedModel)]
    public class Item : MetaDescribedModel<ItemMeta>
    {
        public const string MAIN_STORAGE_ID = "MAIN_STORAGE";

        /// <summary>
        /// ID of the building the item is kept in. Null if is equiped by a character.
        /// Should not be set directly. (leaving public setter to simplify table serialization)
        /// </summary>
        public string StorageId { get; set; } = MAIN_STORAGE_ID;
        /// <summary>
        /// ID of the character that currently owns this item. Null if is not owned by any character.
        /// Should not be set directly. (leaving public setter to simplify table serialization)
        /// </summary>
        public string OwningCharacterId { get; set; }
        public bool IsOwnedByCharacter => OwningCharacterId != null;

        public Item () { }
        public Item (string metaId) : base(metaId) { }

        public void SetStorage (string storageId)
        {
            StorageId = storageId;
            OwningCharacterId = null;
        }

        public void SetOwningCharacter (string characterId)
        {
            OwningCharacterId = characterId;
            StorageId = null;
        }
    }
}
