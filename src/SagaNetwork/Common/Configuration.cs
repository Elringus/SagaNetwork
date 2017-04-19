using System;
using Microsoft.Extensions.Configuration;
using SagaNetwork.Models;

namespace SagaNetwork
{
    public enum DeploymentTier
    {
        Development,
        Test,
        Production
    }

    public static class Configuration
    {
        public static IConfigurationRoot AppSettings => Startup.Configuration; 

        public static DeploymentTier DeploymentTier
        {
            get
            {
                var domain = Environment.ExpandEnvironmentVariables("%WEBSITE_SITE_NAME%");

                if (domain.Contains("saganetwork-d")) return DeploymentTier.Development;
                if (domain.Contains("saganetwork-t")) return DeploymentTier.Test;
                if (domain.Contains("saganetwork-p")) return DeploymentTier.Production;

                return DeploymentTier.Development; // fallback when hosting locally
            }
        }

        public static string TierAffix => DeploymentTier == DeploymentTier.Production ? "p" : DeploymentTier == DeploymentTier.Test ? "t" : "d"; 

        public static bool IsTestEnvironment => AppSettings["IsTestEnvironment"].Contains("true"); 

        public static bool IsAccessKeysEnabled
        {
            get
            {
                var loadEntityTask = new GlobalConfiguration().LoadAsync();
                loadEntityTask.Wait();

                return loadEntityTask.Result.IsAccessKeysEnabled;
            }
        }

        public static bool IsServiceOnline
        {
            get
            {
                var loadEntityTask = new GlobalConfiguration().LoadAsync();
                loadEntityTask.Wait();

                return loadEntityTask.Result.IsServiceOnline;
            }
        }

        public static bool IsAuthEnabled
        {
            get
            {
                //var loadEntityTask = new GlobalConfiguration().LoadAsync();
                //loadEntityTask.Wait();

                //return loadEntityTask.Result.IsAuthEnabled;
                return AppSettings["IsAuthEnabled"].Contains("true");
            }
        }

        public static bool IsUtilityOperationsAllowed
        {
            get
            {
                var loadEntityTask = new GlobalConfiguration().LoadAsync();
                loadEntityTask.Wait();

                return loadEntityTask.Result.IsUtilityOperationsAllowed;
            }
        }

        public static string BuildVersion
        {
            get
            {
                var loadEntityTask = new GlobalConfiguration().LoadAsync();
                loadEntityTask.Wait();

                return loadEntityTask.Result.BuildVersion;
            }
        }
    }
}
