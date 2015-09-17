// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)


namespace Marvin.JsonPatch.Dynamic.Adapters
{    
    public interface IObjectAdapter
    {
        void Add(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);        
        void Copy(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        void Move(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        void Remove(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
        void Replace(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);    
        void Test(Marvin.JsonPatch.Operations.Operation operation, dynamic objectToApplyTo);
    }
}
