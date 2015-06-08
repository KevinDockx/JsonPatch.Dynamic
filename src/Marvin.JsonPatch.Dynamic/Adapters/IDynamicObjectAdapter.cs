// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    
    public interface IDynamicObjectAdapter
    {
        void Add(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        // void Copy(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        //void Move(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        void Remove(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        //void Replace(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
     //   void Test(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
    }
}
