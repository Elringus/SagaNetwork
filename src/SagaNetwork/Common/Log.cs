using Microsoft.Extensions.Logging;

namespace SagaNetwork
{
    public static class Log
    {
        public static ILogger Logger { get; set; }

        public static void Info (string message)
        {
            Logger.LogInformation(message);
        }

        public static void Warning (string message)
        {
            Logger.LogWarning(message);
        }

        public static void Error (string message)
        {
            Logger.LogError(message);
        }
    }
}
