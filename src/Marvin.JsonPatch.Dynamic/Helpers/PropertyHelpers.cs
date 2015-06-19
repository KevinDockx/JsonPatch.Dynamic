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
    }
}