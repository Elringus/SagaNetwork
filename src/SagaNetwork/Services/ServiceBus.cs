using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace SagaNetwork
{
    public static class ServiceBus
    {
        public static NamespaceManager NamespaceManager { get; private set; }
        public static QueueClient InstanceRequestQueue { get; private set; }
        public static TopicClient UpdateBuildTopic { get; private set; }

        private static string connectionString => Configuration.AppSettings["ConnectionStrings:ServiceBus"]; 
        private static string instanceRequestQueueName => $"{Configuration.TierAffix.ToUpper()}-InstanceRequestQueue"; 
        private static string updateBuildTopicName => $"{Configuration.TierAffix.ToUpper()}-UpdateBuildTopic"; 

        public static void Initialize ()
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
            NamespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!NamespaceManager.QueueExists(instanceRequestQueueName))
                NamespaceManager.CreateQueue(instanceRequestQueueName);
            InstanceRequestQueue = QueueClient.CreateFromConnectionString(connectionString, instanceRequestQueueName);

            if (!NamespaceManager.TopicExists(updateBuildTopicName))
                NamespaceManager.CreateTopic(updateBuildTopicName);
            UpdateBuildTopic = TopicClient.CreateFromConnectionString(connectionString, updateBuildTopicName);
        }

        public static void SendInstanceRequestMessage(string requestedArenaMetaId)
        {
            if (!Configuration.IsTestEnvironment)
                InstanceRequestQueue.Send(new BrokeredMessage(requestedArenaMetaId));
        }

        public static void SendUpdateBuildTopic (string buildUri)
        {
            if (!Configuration.IsTestEnvironment)
                UpdateBuildTopic.Send(new BrokeredMessage(buildUri));
        }
    }
}
