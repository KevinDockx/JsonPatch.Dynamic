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
        public bool CanSet { get; set; }

        public bool Success { get; set; }

        public PropertyInfo PropertyToSet { get; set; }

        public SetValueResult(PropertyInfo propertyToSet, bool canSet, bool success)
        {
            PropertyToSet = propertyToSet;
            CanSet = canSet;
            Success = success;
        }


    }
}
