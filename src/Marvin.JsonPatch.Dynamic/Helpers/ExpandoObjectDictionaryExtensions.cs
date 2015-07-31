// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using System;
using System.Collections.Generic;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class ExpandoObjectDictionaryExtensions
    {
        internal static void SetValueForCaseInsensitiveKey(this IDictionary<string, object> propertyDictionary,
            string key, object value)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    propertyDictionary[kvp.Key] = value;
                    break;
                }
            }
        }

        internal static void RemoveValueForCaseInsensitiveKey(this IDictionary<string, object> propertyDictionary,
            string key)
        {
            string realKey = null;
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    realKey = kvp.Key;
                    break;
                }
            }

            if (realKey != null)
            {
                propertyDictionary.Remove(realKey);
            }
        }


        internal static object GetValueForCaseInsensitiveKey(this IDictionary<string, object> propertyDictionary,
          string key)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            throw new ArgumentException("Key not found in dictionary");
        }


        internal static bool ContainsCaseInsensitiveKey(this IDictionary<string, object> propertyDictionary,
            string key)
        {
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }         
    }
}
