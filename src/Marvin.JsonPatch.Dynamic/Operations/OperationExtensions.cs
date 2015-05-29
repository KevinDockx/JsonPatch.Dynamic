using Marvin.JsonPatch.Adapters;
using Marvin.JsonPatch.Dynamic.Adapters;
using Marvin.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Operations
{
    public static class OperationExtensions
    {
        public static void Apply(this Operation operation, dynamic dynamic, IDynamicObjectAdapter adapter)
        {
            // todo
        }
    }
}
