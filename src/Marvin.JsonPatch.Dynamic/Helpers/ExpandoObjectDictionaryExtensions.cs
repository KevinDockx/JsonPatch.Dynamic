using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    // return kvp;
                }
            }

            //return null;

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
