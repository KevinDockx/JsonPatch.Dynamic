// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Adapters;
using Marvin.JsonPatch.Operations;
using Marvin.JsonPatch.Dynamic.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Marvin.JsonPatch.Dynamic.Adapters;
using System.Linq.Expressions;
using Marvin.JsonPatch.Dynamic.Helpers;

namespace Marvin.JsonPatch.Dynamic
{
    public class JsonPatchDocument
    {

        public List<Operation> Operations { get; private set; }


        public JsonPatchDocument()
        {
            Operations = new List<Operation>();
        }


        // Create from list of operations  
        public JsonPatchDocument(List<Operation> operations)
        {
            Operations = operations;

        }

        public JsonPatchDocument Add<TProp>(string path, TProp value)
        {
            Operations.Add(new Operation("add", path, null, value));
            return this;
        }

        public JsonPatchDocument AddToArray<TProp>(string path, TProp value)
        {
            Operations.Add(new Operation("add", path + "/-", null, value));
            return this;
        }

        public JsonPatchDocument AddToArray<TProp>(string path, TProp value, int position)
        {
            Operations.Add(new Operation("add", path + "/" + position, null, value));
            return this;
        }


        public void ApplyTo<T>(T objectToApplyTo)
        {
            ApplyTo(objectToApplyTo, new DynamicObjectAdapter());
        }


        /// <summary>
        /// Apply the patch document, passing in a custom IObjectAdapter<typeparamref name=">"/>. 
        /// This method will change the passed-in object.
        /// </summary>
        /// <param name="objectToApplyTo">The object to apply the JsonPatchDocument to</param>
        /// <param name="adapter">The IObjectAdapter instance to use</param>
        public void ApplyTo<T>(T objectToApplyTo, IDynamicObjectAdapter adapter)
        {
            // apply each operation in order
            foreach (var op in Operations)
            {
                op.Apply(objectToApplyTo, adapter);
            }

        }

          

        ///// <summary>
        ///// Apply the patch document, and return a new ExpandoObject (dynamic) with the change applied.
        ///// </summary>
        ///// <param name="objectToCreateNewObjectFrom">The object to start from</param>
        //public T CreateFrom<T>(T objectToCreateNewObjectFrom)
        //{
        //    var adapter = new MixedObjectAdapter();

        //    //dynamic expandoObjectToApplyTo = new ExpandoObject();
        //    //var propertyDictionary = (IDictionary<String, Object>)(expandoObjectToApplyTo);

        //    //foreach (PropertyInfo propertyInfo in
        //    //    objectToCreateNewObjectFrom.GetType().GetProperties())
        //    //{
        //    //    propertyDictionary[propertyInfo.Name] = propertyInfo.GetValue(objectToCreateNewObjectFrom, null);
        //    //}

        //    // apply each operation in order
        //    foreach (var op in Operations)
        //    {
        //        op.Apply(objectToCreateNewObjectFrom, adapter);

        //    }

        //    return objectToCreateNewObjectFrom;
        //    //  return CreateFrom(objectToCreateNewObjectFrom, new MixedObjectAdapter());
        //}

        ///// <summary>
        ///// Apply the patch document, passing in a custom IObjectAdapter<typeparamref name=">"/>, 
        ///// and return a new ExpandoObject (dynamic) with the change applied.
        ///// </summary>
        ///// <param name="objectToCreateNewObjectFrom">The object to start from</param>
        ///// <param name="adapter">The IObjectAdapter instance to use</param>
        ///// <returns></returns>
        //public dynamic CreateFrom(dynamic objectToCreateNewObjectFrom, IDynamicObjectAdapter adapter)
        //{
        //    // clone the object that has been passed in.  This ensures all 
        //    // nested objects are converted to expandoobjects as well, which is
        //    // required to manipulate them afterwards.
          
        //    // we cannot use JsonConvert's ExpandoObject cloning - that will
        //    // remove all type information (if there is any)
        //    //dynamic clonedObject = JsonConvert.DeserializeObject<ExpandoObject>
        //    //    (JsonConvert.SerializeObject(objectToCreateNewObjectFrom));

        //    dynamic expandoObjectToApplyTo = new ExpandoObject();
        //    var propertyDictionary = (IDictionary<String, Object>)(expandoObjectToApplyTo);

        //    foreach (PropertyInfo propertyInfo in
        //        objectToCreateNewObjectFrom.GetType().GetProperties())
        //    {
        //        propertyDictionary[propertyInfo.Name] = propertyInfo.GetValue(objectToCreateNewObjectFrom, null);
        //    }             

        //    // apply each operation in order
        //    foreach (var op in Operations)
        //    {              
        //        op.Apply((ExpandoObject)expandoObjectToApplyTo, adapter);

        //    }

        //    return expandoObjectToApplyTo;
             

        //}


         

    }
}
