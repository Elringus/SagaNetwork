namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class ItemMeta : TableModel<ItemMeta>
    {
        public string ClassMetaId { get; set; }
        public string EquipmentSlot { get; set; }

        public override string BaseTableName => "ItemMetas"; 

        public ItemMeta () { }
        public ItemMeta (string id) : base (id) { }
    }
}
