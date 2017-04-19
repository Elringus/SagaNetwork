
namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.DbModel)]
    public class RarenessGroup : TableModel<RarenessGroup>, IWeighted
    {
        public int ProbabilityWeight { get; set; }
        public int AbilityLimit { get; set; }
        public int AbilityMaxLevel { get; set; }

        public override string BaseTableName => "RarenessGroups";

        public RarenessGroup () { }
        public RarenessGroup (string id) : base(id) { }
    }
}
