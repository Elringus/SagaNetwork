using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SagaNetwork
{
    public static class JParser
    {
        public static JToken TryParse (string jsonString)
        {
            try { return JToken.Parse(jsonString); }
            catch (JsonReaderException) { return null; }
        }

        public static T ParseField<T> (this JToken jToken, string fieldName)
        {
            if (jToken[fieldName] == null)
                return default(T);

            T value;

            try { value = jToken[fieldName].ToObject<T>(); }
            catch (JsonSerializationException)
            {
                try { value = JsonConvert.DeserializeObject<T>(jToken[fieldName].ToString()); }
                catch (JsonSerializationException) { value = default(T); }
            }

            return value == null ? default(T) : value;
        }

        public static object ParseField (this JToken jToken, string fieldName, Type fieldType)
        {
            if (jToken[fieldName] == null)
                return null;

            object value;

            try { value = jToken[fieldName].ToObject(fieldType); }
            catch (JsonSerializationException)
            {
                try { value = JsonConvert.DeserializeObject(jToken[fieldName].ToString(), fieldType); }
                catch (JsonSerializationException) { value = null; }
            }

            return value == null ? null : value;
        }
    }
}
