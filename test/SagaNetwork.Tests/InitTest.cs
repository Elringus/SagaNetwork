using Microsoft.AspNetCore.Hosting.Internal;
using System;
using System.IO;

namespace SagaNetwork.Tests
{
    /// <summary>
    /// Base class for tests which needs to use cloud services.
    /// Handles services init.
    /// </summary>
    public abstract class InitTest
    {
        private static bool CloudServicesInitialized { get; }

        static InitTest ()
        {
            if (CloudServicesInitialized) return;

            var env = new HostingEnvironment()
            {
                EnvironmentName = "Development",
                // Path to the test project directory. There could be a better way to resolve it...
                ContentRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName
            };
            var inst = new Startup(env);

            CloudServicesInitialized = true;
        }
    }
}
