
namespace SagaNetwork.Models
{
    /// <summary>
    /// Global tier-specific service configuration.
    /// </summary>
    public class GlobalConfiguration : SingletonModel<GlobalConfiguration>
    {
        public bool IsServiceOnline { get; set; } = true;
        public bool IsUtilityOperationsAllowed { get; set; } = false;
        public bool IsAuthEnabled { get; set; } = true;
        public bool IsAccessKeysEnabled { get; set; } = false;
        public string BuildVersion { get; set; } = "0.0.0";

        public override string BaseTableName => "GlobalConfiguration"; 

    }
}
