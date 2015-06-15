// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using System.Reflection;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal class GetValueResult
    {
        public bool CanGet { get; private set; }

        public bool Success { get; private set; }

        public PropertyInfo PropertyToGet { get; private set; }

        public object Value { get; private set; }

        public GetValueResult(PropertyInfo propertyToGet, bool canGet, object value, bool success)
        {
            PropertyToGet = propertyToGet;
            CanGet = canGet;
            Value = value;
            Success = success;
        }
    }
}
