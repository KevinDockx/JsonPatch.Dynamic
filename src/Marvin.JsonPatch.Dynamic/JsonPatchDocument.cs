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
using Marvin.JsonPatch.Exceptions;

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

        public JsonPatchDocument Add(string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }
 
            Operations.Add(new Operation("add", checkPathResult.AdjustedPath, null, value));
            return this;
        }
          
        public JsonPatchDocument Remove(string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }
 
            Operations.Add(new Operation("remove", checkPathResult.AdjustedPath, null, null));
            return this;
        }

        public JsonPatchDocument Replace(string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }
 
            Operations.Add(new Operation("replace", checkPathResult.AdjustedPath, null, value));
            return this;
        }
   
        public JsonPatchDocument Move(string from, string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          from), null);
            }


            Operations.Add(new Operation("move", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
            return this;
        }
                 
        public JsonPatchDocument Copy(string from, string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          from), null);
            }

            Operations.Add(new Operation("copy", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
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

    }
}
