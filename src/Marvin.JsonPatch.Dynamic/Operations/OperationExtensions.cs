// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Adapters;
using Marvin.JsonPatch.Operations;
using System;

namespace Marvin.JsonPatch.Dynamic.Operations
{
    public static class OperationExtensions
    {
        internal static void Apply(this Operation operation, object objectToApplyTo, IObjectAdapter adapter)
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
                case OperationType.Test:
                    throw new NotImplementedException("Test is currently not implemented.");  
                default:
                    break;
            }
        }
    }
}
