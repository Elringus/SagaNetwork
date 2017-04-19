using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;

namespace SagaNetwork
{
    public static class CloudStorage
    {
        public static CloudStorageAccount StorageAccount { get; private set; }
        public static CloudTableClient TableClient { get; private set; }
        public static CloudBlobClient BlobClient { get; private set; }
        public static CloudBlobContainer BlobContainer { get; private set; }

        private static string connectionString => Configuration.AppSettings["ConnectionStrings:Storage"]; 

        public static void Initialize ()
        {
            StorageAccount = CloudStorageAccount.Parse(connectionString);

            // Disabling nagle for table improves performance: 
            // https://blogs.msdn.microsoft.com/windowsazurestorage/2010/06/25/nagles-algorithm-is-not-friendly-towards-small-requests/
            var tableServicePoint = ServicePointManager.FindServicePoint(StorageAccount.TableEndpoint);
            tableServicePoint.UseNagleAlgorithm = false; 

            TableClient = StorageAccount.CreateCloudTableClient();
            BlobClient = StorageAccount.CreateCloudBlobClient();

            BlobContainer = BlobClient.GetContainerReference($"saga-{Configuration.TierAffix}-container");
            BlobContainer.CreateIfNotExists();
        }
    }
}
