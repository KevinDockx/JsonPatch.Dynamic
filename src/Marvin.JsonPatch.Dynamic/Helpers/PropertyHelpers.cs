// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Operations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

        internal static ActualPropertyPathResult GetActualPropertyPath(string propertyPath, object objectToApplyTo, 
            Operation operationToReport, bool forPath)
        {
            if (propertyPath.EndsWith("/-"))
            {
                return new ActualPropertyPathResult(-1, propertyPath.Substring(0, propertyPath.Length - 2), true);
            }
            else
            {
                var possibleIndex = propertyPath.Substring(propertyPath.LastIndexOf("/") + 1);
                int castedIndex = -1;
                if (int.TryParse(possibleIndex, out castedIndex))
                {
                    // has numeric end.  
                    if (castedIndex > -1)
                    {
                        var pathToProperty = propertyPath.Substring(
                           0,
                           propertyPath.LastIndexOf('/' + castedIndex.ToString()));

                        return new ActualPropertyPathResult(castedIndex, pathToProperty, false);
                    }
                    else
                    {
                        string message = forPath ? 
                             string.Format("Patch failed: provided path is invalid, position too small: {0}",
                              propertyPath)
                              : string.Format("Patch failed: provided from is invalid, position too small: {0}",
                              propertyPath);

                        // negative position - invalid path
                        throw new JsonPatchException(
                                new JsonPatchError(
                                    objectToApplyTo,
                                    operationToReport,
                                    message), 
                                422);
                    } 
                }
                return new ActualPropertyPathResult(-1, propertyPath, false);
            }
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