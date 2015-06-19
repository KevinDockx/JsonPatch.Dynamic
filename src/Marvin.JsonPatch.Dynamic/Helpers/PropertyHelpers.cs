// Kevin Dockx
//
// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch
//
// Enjoy :-)

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class PropertyHelpers
    {
       

        internal static GetValueResult GetValue(PropertyInfo propertyToGet, object targetObject, string pathToProperty)
        {
            // it is possible the path refers to a nested property.  In that case, we need to 
            // get from a different target object: the nested object.

            var splitPath = pathToProperty.Split('/');

            // skip the first one if it's empty
            var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

            for (int i = startIndex; i < splitPath.Length - 1; i++)
            {
                var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
                    , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                targetObject = propertyInfoToGet.GetValue(targetObject, null);
            }

            if (propertyToGet.CanRead)
            {
                    return new GetValueResult(propertyToGet, true, propertyToGet.GetValue(targetObject, null), true);
            }
            else
            {
                return new GetValueResult(propertyToGet, false, null, false);
            } 
        }

         

        internal static SetValueResult SetValue(PropertyInfo propertyToSet, object targetObject, string pathToProperty, object value)
        {
            // it is possible the path refers to a nested property.  In that case, we need to 
            // set on a different target object: the nested object.


            var splitPath = pathToProperty.Split('/');

            // skip the first one if it's empty
            var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

            for (int i = startIndex; i < splitPath.Length - 1; i++)
            {
                var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
                    , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                targetObject = propertyInfoToGet.GetValue(targetObject, null);
            }

            if (propertyToSet.CanWrite)
            {
                propertyToSet.SetValue(targetObject, value, null);
                return new SetValueResult(propertyToSet, true, true);
            }
            else
            {
                return new SetValueResult(propertyToSet, false, true);
            }
                      
             
        }

         

        internal static ConversionResult ConvertToActualType(Type propertyType, object value)
        {
            try
            {
                var o = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), propertyType);
                return new ConversionResult(true, o);
            }
            catch (Exception)
            {
                return new ConversionResult(false, null);
            }
        }


        internal static Type GetEnumerableType(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }
            return null;
        }



        internal static CheckNumericEndResult GetNumericEnd(string path)
        {
            var possibleIndex = path.Substring(path.LastIndexOf("/") + 1);
            int castedIndex = -1;
            if (int.TryParse(possibleIndex, out castedIndex))
            {
                return new CheckNumericEndResult(true, castedIndex);              
            }

            return new CheckNumericEndResult(false, null);
        }

  


        private static PropertyInfo GetPropertyInfo(object targetObject, string propertyName,
        BindingFlags bindingFlags)
        {
            return targetObject.GetType().GetProperty(propertyName, bindingFlags);
        }
    }
}