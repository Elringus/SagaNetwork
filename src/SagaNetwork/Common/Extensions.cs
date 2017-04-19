using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SagaNetwork
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if the string contains a valid JSON.
        /// </summary>
        public static bool IsValidJson (this string jsonString)
        {
            try { JToken.Parse(jsonString); return true; }
            catch (JsonReaderException) { return false; }
        }

        /// <summary>
        /// Retrieves file name from the content disposition headers.
        /// The property should already be exposed in the .NET Core (https://github.com/aspnet/HttpAbstractions/issues/499), but can't find it.
        /// </summary>
        public static string GetFileName (this IFormFile formFile)
        {
            return ContentDispositionHeaderValue.Parse(formFile.ContentDisposition)?.FileName?.Trim('"');
        }

        /// <summary>
        /// Swaps two collection elements.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="indexA">Index of the first element to swap.</param>
        /// <param name="indexB">Index of the second element to swap with.</param>
        /// <returns></returns>
        public static IList<T> Swap<T> (this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }

        public static void AddUnique<T> (this IList<T> list, T elementToAdd) 
        {
            if (list.Contains(elementToAdd)) return;
            list.Add(elementToAdd);
        }

        public static void AddUnique<TKey, TValue> (this IDictionary<TKey, TValue> enumerable, TKey key, TValue value)
        {
            if (enumerable.ContainsKey(key)) return;
            enumerable.Add(key, value);
        }

        /// <summary>
        /// Returns subtracted timespan, floored by zero.
        /// </summary>
        public static TimeSpan Subtract (this TimeSpan thisTimeSpan, TimeSpan timeSpanToSubtract)
        {
            var result = thisTimeSpan - timeSpanToSubtract;
            if (result.TotalMilliseconds < 0) return TimeSpan.Zero;
            else return result;
        }
    }
}
