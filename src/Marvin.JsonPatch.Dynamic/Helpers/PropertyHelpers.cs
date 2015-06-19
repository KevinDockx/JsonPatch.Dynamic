// Kevin Dockx
//
// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch
//
// Enjoy :-)

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class PropertyHelpers
    {
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

        internal static bool IsNonStringArray(Type type)
        {
            if (GetIListType(type) != null)
            {
                return true;
            }

            return (!(type == typeof(string)) && typeof(IList)
                .GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()));
        }

        internal static Type GetIListType(Type type)
        {
            if (type == null)
                throw new ArgumentException("Parameter type cannot be null");

            if (IsGenericListType(type))
            {
                return type.GetGenericArguments()[0];
            }

            foreach (Type interfaceType in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (IsGenericListType(interfaceType))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        internal static bool IsGenericListType(Type type)
        {
            if (type == null)
                throw new ArgumentException("Parameter type cannot be null");

            if (type.GetTypeInfo().IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return true;
            }

            return false;
        }
    }
}