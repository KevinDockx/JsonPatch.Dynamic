// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

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
        public static void Apply(this Operation operation, dynamic objectToApplyTo, IDynamicObjectAdapter adapter)
        {
            if (objectToApplyTo == null)
            {
                throw new NullReferenceException("objectToApplyTo cannot be null");
            }
            if (adapter == null)
            {
                throw new NullReferenceException("adapter cannot be null");
            }

            switch (operation.OperationType)
            {
                case OperationType.Add:
                    adapter.Add(operation, objectToApplyTo);
                    break;
                case OperationType.Remove:
                    adapter.Remove(operation, objectToApplyTo);
                    break;
                case OperationType.Replace:
                    adapter.Replace(operation, objectToApplyTo);
                    break;
                case OperationType.Move:
                    adapter.Move(operation, objectToApplyTo);
                    break;
                case OperationType.Copy:
                    adapter.Copy(operation, objectToApplyTo);
                    break;
                //case OperationType.Test:
                //    adapter.Test(this, objectToApplyTo);
                //    break;
                default:
                    break;
            }
        }
    }
}
