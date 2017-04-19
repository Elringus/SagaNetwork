namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class AbilityMeta : TableModel<AbilityMeta>
    {
        public float Cooldown { get; set; } = 0;
        public int StaminaCost { get; set; } = 0;

        public override string BaseTableName => "AbilityMetas";

        public AbilityMeta () { }
        public AbilityMeta (string id) : base (id) { }
    }
}
