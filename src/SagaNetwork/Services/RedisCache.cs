using System;
using StackExchange.Redis;

namespace SagaNetwork
{
    public class RedisCache
    {
        public static IDatabase RedisDb { get; private set; }

        private static string connectionString => Configuration.AppSettings["ConnectionStrings:Redis"]; 

        public static void Initialize ()
        {
            if (!Authorization.IsEnabled) return;

            var connection = ConnectionMultiplexer.Connect(connectionString);
            RedisDb = connection.GetDatabase();
        }

        /// <summary>
        /// Generates and sets a new GUID value for the specified key.
        /// Record will live for 24 hours.
        /// </summary>
        /// <param name="key">Key for which to set new GUID value.</param>
        /// <returns>Generated GUID value which is set to the key.</returns>
        public static string SetGuidForKey (string key)
        {
            if (!Authorization.IsEnabled) return "";

            var guidValue = Guid.NewGuid().ToString("N");
            RedisDb.StringSet(key, guidValue, TimeSpan.FromHours(24));

            return guidValue;
        }

        /// <summary>
        /// Checks if the key exists and is equal to the value.
        /// </summary>
        public static bool CheckKeyValue (string key, string value)
        {
            if (!Authorization.IsEnabled) return true;

            string existingToken = RedisDb.StringGet(key);
            if (existingToken == null) return false;

            return existingToken == value;
        }
    }
}
