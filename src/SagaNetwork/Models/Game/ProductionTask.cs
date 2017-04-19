namespace SagaNetwork.Models
{
    /// <summary>
    /// Represents contract production process.
    /// </summary>
    [GenerateApi(UClassType.Struct)]
    public class ProductionTask : TimeTask
    {
        public string ProducedContractMetaId { get; set; }
        public bool IsLoopable { get; set; }

        public ProductionTask () { }
        public ProductionTask (ContractMeta contractMeta) : base(contractMeta.ProductionTime)
        {
            ProducedContractMetaId = contractMeta.Id;
            IsLoopable = contractMeta.IsLoopable;
        }

        public override int Check ()
        {
            var prodCycles = base.Check();

            if (prodCycles > 1 && !IsLoopable)
                return 1;

            return prodCycles;
        }
    }
}
