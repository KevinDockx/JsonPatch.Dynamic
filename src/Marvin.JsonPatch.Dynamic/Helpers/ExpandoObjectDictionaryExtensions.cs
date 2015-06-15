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
        internal static void SetValueForCaseInsensitiveKey(this IDictionary<String, Object> propertyDictionary,
       string key, object value)
        {
            key = key.ToLower();
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (kvp.Key.ToLower() == key)
                {
                    propertyDictionary[kvp.Key] = value;
                    break;
 
                }
            } 
        }


        internal static void RemoveValueForCaseInsensitiveKey(this IDictionary<String, Object> propertyDictionary,
      string key)
        {
            string realKey = "";
            key = key.ToLower();
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (kvp.Key.ToLower() == key)
                {
                    realKey = kvp.Key; 
                    break;
                    // return kvp;
                }
            }
            
            if (realKey != "")
            {
                propertyDictionary.Remove(realKey);
            }

             

        }


        internal static object GetValueForCaseInsensitiveKey(this IDictionary<String, Object> propertyDictionary,
          string key)
        {
            key = key.ToLower();
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (kvp.Key.ToLower() == key)
                {
                    return kvp.Value;

                }
            }

            throw new ArgumentException("Key not found in dictionary");
        }

        


             internal static object GetValueForCaseInsensitiveKeyTest(this IDictionary<String, Object> propertyDictionary,
          string key)
        {
            key = key.ToLower();
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (kvp.Key.ToLower() == key)
                {
                    return kvp.Value;

                }
            }
                 return null;

        }


        internal static bool ContainsKeyCaseInsensitive(this IDictionary<String, Object> propertyDictionary,
       string key)
        {
            key = key.ToLower();
            foreach (KeyValuePair<string, object> kvp in propertyDictionary)
            {
                if (kvp.Key.ToLower() == key)
                {
                    return true;
                    // return kvp;
                }
            }
            return false;

        }


    }
}
