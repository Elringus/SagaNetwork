namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.MetaModel)]
    public class TalentMeta : TableModel<TalentMeta>
    {
        public string AbilityMetaId { get; set; }
        public int Tier { get; set; }

        public override string BaseTableName => "TalentMetas"; 

        public TalentMeta () { }
        public TalentMeta (string id) : base (id) { }
    }
}
