// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal class SetValueResult
    {
        public bool CanSet { get; private set; }

        public bool Success { get; private set; }

        public PropertyInfo PropertyToSet { get; private set; }

        public SetValueResult(PropertyInfo propertyToSet, bool canSet, bool success)
        {
            PropertyToSet = propertyToSet;
            CanSet = canSet;
            Success = success;
        }


    }
}
