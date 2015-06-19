// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    internal class DynamicObjectAdapter : IDynamicObjectAdapter
    {
        public IContractResolver ContractResolver { get; private set; }
        public DynamicObjectAdapter()
        {
            ContractResolver = new DefaultContractResolver();
        }
        
        /// <summary>
        /// Add is used by various operations (eg: add, copy, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Add(string path, object value, object objectToApplyTo, Operation operationToReport)
        {
            // add, in this implementation, CAN add properties if the container is an
            // ExpandoObject.

            // first up: if the path ends in a numeric value, we're inserting in a list and
            // that value represents the position; if the path ends in "-", we're appending
            // to the list.

            var appendList = false;
            var positionAsInteger = -1;
            var actualPathToProperty = path;

            if (path.EndsWith("/-"))
            {
                appendList = true;
                actualPathToProperty = path.Substring(0, path.Length - 2);
            }
            else
            {
                var checkNumericEndResult = PropertyHelpers.GetNumericEnd(path);

                if (checkNumericEndResult.HasNumericEnd)
                {
                    positionAsInteger = checkNumericEndResult.NumericEnd;
                    if (positionAsInteger > -1)
                    {
                        actualPathToProperty = path.Substring(0,
                       path.LastIndexOf('/' + positionAsInteger.ToString()));
                    }
                    else
                    {
                        // negative position - invalid path
                        throw new JsonPatchException(operationToReport,
                              string.Format("Patch failed: provided path is invalid, position too small: {0}",
                              path),
                              objectToApplyTo, 422);
                    }
                }
            }


            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty, ContractResolver);

            if (result.UseDynamicLogic)
            {
                if (result.IsValidPathForAdd)
                {
                    if (result.Container.ContainsCaseInsensitiveKey(result.PropertyPathInParent))
                    {
                        // Existing property.  
                        // If it's not an array, we need to check if the value fits the property type
                        // 
                        // If it's an array, we need to check if the value fits in that array type,
                        // and add it at the correct position (if allowed).

                        if (appendList || positionAsInteger > -1)
                        {
                            // get the actual type

                            var typeOfPathProperty = result.Container
                                .GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();
  
                            if (PropertyHelpers.IsNonStringArray(typeOfPathProperty))
                            {
                                // now, get the generic type of the enumerable
                                var genericTypeOfArray = PropertyHelpers.GetIListType(typeOfPathProperty);
                                var conversionResult = PropertyHelpers.ConvertToActualType(genericTypeOfArray, value);

                                if (!conversionResult.CanBeConverted)
                                {
                                    throw new JsonPatchException(operationToReport,
                                      string.Format("Patch failed: provided value is invalid for array property type at location path: {0}",
                                      path),
                                      objectToApplyTo, 422);
                                }

                                // get value (it can be cast, we just checked that)
                                // var array = containerDictionary[finalPath] as IList;

                                var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent) as IList;

                                if (appendList)
                                {
                                    array.Add(conversionResult.ConvertedInstance);
                                    result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);
                                }
                                else
                                {
                                    // specified index must not be greater than the amount of items in the
                                    // array
                                    if (positionAsInteger <= array.Count)
                                    {
                                        array.Insert(positionAsInteger, conversionResult.ConvertedInstance);
                                        result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);
                                    }
                                    else
                                    {
                                        throw new JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                   path),
                                   objectToApplyTo, 422);
                                    }
                                }



                            }
                            else
                            {
                                throw new JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                                   path),
                                   objectToApplyTo, 422);
                            }
                        }
                        else
                        {
                            // get the actual type
                            var typeOfPathProperty = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();

                            // can the value be converted to the actual type?
                            var conversionResultTuple =
                                PropertyHelpers.ConvertToActualType(typeOfPathProperty, value);

                            // conversion successful
                            if (conversionResultTuple.CanBeConverted)
                            {
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent,
                                    conversionResultTuple.ConvertedInstance);
                            }
                            else
                            {
                                throw new JsonPatchException(operationToReport,
                                string.Format("Patch failed: provided value is invalid for property type at location path: {0}",
                                path),
                                objectToApplyTo, 422);
                            }
                        }
                    }
                    else
                    {
                        // New property - add it.  
                        result.Container.Add(result.PropertyPathInParent, value);
                    }
                }
                else
                {
                    throw new JsonPatchException(operationToReport,
                    string.Format("Patch failed: cannot add to the parent of the property at location path: {0}.  To be able to dynamically add properties, the parent must be an ExpandoObject.", path),
                    objectToApplyTo, 422);
                }
            }
            else
            {
                if (!result.IsValidPathForAdd)
                {
                    throw new JsonPatchException(operationToReport,
                      string.Format("Patch failed: the provided path is invalid: {0}.", path),
                      objectToApplyTo, 422);
                }

                // If it' an array, add to that array.  If it's not, we replace.

                // is the path an array (but not a string (= char[]))?  In this case,
                // the path must end with "/position" or "/-", which we already determined before.

                var patchProperty = result.JsonPatchProperty;

                if (appendList || positionAsInteger > -1)
                {
                    if (PropertyHelpers.IsNonStringArray(patchProperty.Property.PropertyType))
                    {
                        // now, get the generic type of the IList<> from Property type.
                        var genericTypeOfArray = PropertyHelpers.GetIListType(patchProperty.Property.PropertyType);

                        var conversionResult = PropertyHelpers.ConvertToActualType(genericTypeOfArray, value);

                        if (!conversionResult.CanBeConverted)
                        {
                            throw new JsonPatchException(operationToReport,
                           string.Format("Patch failed: provided value is invalid for array property type at location path: {0}",
                           path),
                           objectToApplyTo, 422);
                        }

                        if (patchProperty.Property.Readable)
                        {
                            var array = (IList)patchProperty.Property.ValueProvider
                                .GetValue(patchProperty.Parent);

                            if (appendList)
                            {
                                array.Add(conversionResult.ConvertedInstance);
                            }
                            else
                            {
                                // specified index must not be greater than the amount of items in the
                                // array
                                if (positionAsInteger <= array.Count)
                                {
                                    array.Insert(positionAsInteger, conversionResult.ConvertedInstance);
                                }
                                else
                                {
                                    throw new JsonPatchException(operationToReport,
                               string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                               path),
                               objectToApplyTo, 422);
                                }
                            }

                        }
                        else
                        {
                            // cannot read the property
                            throw new JsonPatchException(operationToReport,
                               string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.",
                               path),
                               objectToApplyTo, 422);
                        }

                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                          string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                          path),
                          objectToApplyTo, 422);
                    }

                }
                else
                {
                    var conversionResultTuple = PropertyHelpers.ConvertToActualType(
                    patchProperty.Property.PropertyType,
                    value);

                    if (conversionResultTuple.CanBeConverted)
                    {
                        if (patchProperty.Property.Writable)
                        {
                            patchProperty.Property.ValueProvider.SetValue(
                   patchProperty.Parent,
                   conversionResultTuple.ConvertedInstance);

                        }
                        else
                        {
                            throw new JsonPatchException(operationToReport,
                     string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).",
                     path),
                     objectToApplyTo, 422);
                        }

                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                          string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).",
                          path),
                          objectToApplyTo, 422);
                    }

                }
            }
        }


        public void Add(JsonPatch.Operations.Operation operation, dynamic objectToApplyTo)
        {
            Add(operation.path, operation.value, objectToApplyTo, operation);
        }


        public void Remove(Operation operation, dynamic objectToApplyTo)
        {
            Remove(operation.path, objectToApplyTo, operation);
        }


        /// <summary>
        /// Remove is used by various operations (eg: remove, move, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error.  The return value
        /// contains the type of the item that has been removed - this can be used by other methods, like 
        /// replace, to ensure that we can pass in the correctly typed value to whatever method follows.
        /// </summary>
        private Type Remove(string path, object objectToApplyTo, Operation operationToReport)
        {
            // remove, in this implementation, CAN remove properties if the container is an
            // ExpandoObject.

            var removeFromList = false;
            var positionAsInteger = -1;
            var actualPathToProperty = path;

            if (path.EndsWith("/-"))
            {
                removeFromList = true;
                actualPathToProperty = path.Substring(0, path.Length - 2);
            }
            else
            {
                var checkNumericEndResult = PropertyHelpers.GetNumericEnd(path);

                if (checkNumericEndResult.HasNumericEnd)
                {
                    positionAsInteger = checkNumericEndResult.NumericEnd;
                    if (positionAsInteger > -1)
                    {
                        actualPathToProperty = path.Substring(0,
                       path.LastIndexOf('/' + positionAsInteger.ToString()));
                    }
                    else
                    {
                        // negative position - invalid path
                        throw new JsonPatchException(operationToReport,
                              string.Format("Patch failed: provided path is invalid, position too small: {0}",
                              path),
                              objectToApplyTo, 422);
                    }
                }
            }


            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty, ContractResolver);

            if (result.UseDynamicLogic)
            {
                if (result.IsValidPathForRemove)
                {
                    // if it's not an array, we can remove the property from
                    // the dictionary.  If it's an array, we need to check the position first.
                    if (removeFromList || positionAsInteger > -1)
                    {

                        var typeOfPathProperty = result.Container
                                .GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();
  
                        if (PropertyHelpers.IsNonStringArray(typeOfPathProperty))
                        {
                            // now, get the generic type of the enumerable
                            var genericTypeOfArray = PropertyHelpers.GetIListType(typeOfPathProperty);

                            var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent) as IList;

                            if (removeFromList)
                            {
                                if (array.Count == 0)
                                {
                                    // if the array is empty, we should throw an error
                                    throw new JsonPatchException(operationToReport,
                                      string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                      path),
                                      objectToApplyTo, 422);
                                }

                                array.RemoveAt(array.Count - 1);
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);

                                // return the type of the value that has been removed.
                                return genericTypeOfArray;
                            }
                            else
                            {
                                if (positionAsInteger < array.Count)
                                {
                                    array.RemoveAt(positionAsInteger);
                                    result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);

                                    // return the type of the value that has been removed.
                                    return genericTypeOfArray;
                                }
                                else
                                {
                                    throw new JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                   path),
                                   objectToApplyTo, 422);
                                }

                            }
                        }
                        else
                        {
                            throw new JsonPatchException(operationToReport,
                               string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                               path),
                               objectToApplyTo, 422);
                        }
                    }
                    else
                    {
                        // get the property
                        var getResult = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent);
                        var actualType = getResult.GetType();

                        // remove the property
                        result.Container.RemoveValueForCaseInsensitiveKey(result.PropertyPathInParent);
                        return actualType;
                    }
                }
                else
                {
                    throw new JsonPatchException(operationToReport,
                 string.Format("Patch failed: cannot remove property at location path: {0}.  To be able to dynamically remove properties, the parent must be an ExpandoObject.", path),
                 objectToApplyTo, 422);
                }
            }
            else
            {
                // not dynamic 
                if (!result.IsValidPathForRemove)
                {
                    throw new JsonPatchException(operationToReport,
                      string.Format("Patch failed: the provided path is invalid: {0}.", path),
                      objectToApplyTo, 422);
                }

                var patchProperty = result.JsonPatchProperty;

                if (removeFromList || positionAsInteger > -1)
                { 
                    if (PropertyHelpers.IsNonStringArray(patchProperty.Property.PropertyType))
                    {
                        // now, get the generic type of the IList<> from Property type.
                        var genericTypeOfArray = PropertyHelpers.GetIListType(patchProperty.Property.PropertyType);

                        if (patchProperty.Property.Readable)
                        {
                            var array = (IList)patchProperty.Property.ValueProvider
                                   .GetValue(patchProperty.Parent);

                            if (removeFromList)
                            {
                                if (array.Count == 0)
                                {
                                    // if the array is empty, we should throw an error
                                    throw new JsonPatchException(operationToReport,
                                      string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                      path),
                                      objectToApplyTo, 422);
                                }

                                array.RemoveAt(array.Count - 1);

                                // return the type of the value that has been removed
                                return genericTypeOfArray;
                            }
                            else
                            {
                                if (positionAsInteger < array.Count)
                                {
                                    array.RemoveAt(positionAsInteger);

                                    // return the type of the value that has been removed
                                    return genericTypeOfArray;
                                }
                                else
                                {
                                    throw new JsonPatchException(operationToReport,
                                     string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                     path),
                                     objectToApplyTo, 422);
                                }
                            }
                        }
                        else
                        {
                            throw new JsonPatchException(operationToReport,
                             string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.",
                             path),
                             objectToApplyTo, 422);
                        }

                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                           path),
                           objectToApplyTo, 422);
                    }
                }
                else
                {

                    if (patchProperty.Property.Writable)
                    {
                        // setting the value to "null" will use the default value in case of value types, and
                        // null in case of reference types
                        object value = null;

                        if (patchProperty.Property.PropertyType.GetTypeInfo().IsValueType
                            && Nullable.GetUnderlyingType(patchProperty.Property.PropertyType) == null)
                        {
                            value = Activator.CreateInstance(patchProperty.Property.PropertyType);
                        }

                        patchProperty.Property.ValueProvider.SetValue(patchProperty.Parent, value);

                        return patchProperty.Property.PropertyType;
                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                         string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).",
                         path),
                         objectToApplyTo, 422);
                    }
                }
            }
        }


        public void Replace(Operation operation, dynamic objectToApplyTo)
        {

            var typeOfRemovedProperty = Remove(operation.path, objectToApplyTo, operation);

            var conversionResult = PropertyHelpers.ConvertToActualType(typeOfRemovedProperty, operation.value);
            if (conversionResult.CanBeConverted)
            {
                Add(operation.path, conversionResult.ConvertedInstance, objectToApplyTo, operation);
            }
            else
            {
                throw new JsonPatchException(operation,
                     string.Format("Patch failed: property value cannot be converted to type of path location {0}",
                     operation.path),
                     objectToApplyTo, 422);
            }


        }


        public void Move(Operation operation, dynamic objectToApplyTo)
        {
            var valueAtFromLocation = GetValueAtLocation(operation.from, objectToApplyTo, operation);

            // remove that value
            Remove(operation.from, objectToApplyTo, operation);

            // add that value to the path location
            Add(operation.path, valueAtFromLocation, objectToApplyTo, operation);

        }



        public void Copy(Operation operation, dynamic objectToApplyTo)
        {
            // get value at from location and add that value to the path location
            Add(operation.path, GetValueAtLocation(operation.from, objectToApplyTo, operation)
                , objectToApplyTo, operation);
        }



        private object GetValueAtLocation(string location, dynamic objectToGetValueFrom, Operation operationToReport)
        {
            // get value from "objectToGetValueFrom" at location "location"
            object valueAtLocation = null;
            var positionAsInteger = -1;
            var actualFromProperty = location;


            var checkNumericEndResult = PropertyHelpers.GetNumericEnd(location);

            if (checkNumericEndResult.HasNumericEnd)
            {
                positionAsInteger = checkNumericEndResult.NumericEnd;
                if (positionAsInteger > -1)
                {
                    actualFromProperty = location.Substring(0,
                   location.LastIndexOf('/' + positionAsInteger.ToString()));
                }
                else
                {
                    // negative position - invalid path
                    throw new JsonPatchException(operationToReport,
                          string.Format("Patch failed: provided from is invalid, position too small: {0}",
                          location),
                          objectToGetValueFrom, 422);
                }
            }

            // first, analyze the tree. 
            var result = new ObjectTreeAnalysisResult(objectToGetValueFrom, actualFromProperty, ContractResolver);

            if (result.UseDynamicLogic)
            {
                // find the property
                if (result.Container.ContainsCaseInsensitiveKey(result.PropertyPathInParent))
                {
                    if (positionAsInteger > -1)
                    {
                        // get the actual type

                        var typeOfPathProperty = result.Container
                            .GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();

                        if (PropertyHelpers.IsNonStringArray(typeOfPathProperty))
                        {
                            // now, get the generic type of the enumerable
                            var genericTypeOfArray = PropertyHelpers.GetIListType(typeOfPathProperty);

                            // get value
                            var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent) as IList;

                            if (positionAsInteger < array.Count)
                            {
                                valueAtLocation = array[positionAsInteger];
                            }
                            else
                            {
                                throw new JsonPatchException(operationToReport,
                              string.Format("Patch failed: property at location from: {0} does not exist", location),
                                objectToGetValueFrom, 422);
                            }

                        }
                        else
                        {
                            throw new JsonPatchException(operationToReport,
                                  string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", location),
                                    objectToGetValueFrom, 422);
                        }
                    }
                    else
                    {
                        // get the value
                        valueAtLocation =
                            result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent);
                    }
                }
                else
                {
                    throw new JsonPatchException(operationToReport,
                    string.Format("Patch failed: property at location from: {0} does not exist", location),
                    objectToGetValueFrom, 422);
                }
            }
            else
            {

                // not dynamic.

                var patchProperty = result.JsonPatchProperty;

                // is the path an array (but not a string (= char[]))?  In this case,
                // the path must end with "/position" or "/-", which we already determined before. 
                if (positionAsInteger > -1)
                {
                    if (PropertyHelpers.IsNonStringArray(patchProperty.Property.PropertyType))
                    {
                        // now, get the generic type of the enumerable
                        //var genericTypeOfArray = GetIListType(patchProperty.Property.PropertyType);

                        if (patchProperty.Property.Readable)
                        {
                            var array = (IList)patchProperty.Property.ValueProvider
                                .GetValue(patchProperty.Parent);

                            if (positionAsInteger < array.Count)
                            {
                                valueAtLocation = array[positionAsInteger];
                            }
                            else
                            {

                                throw new JsonPatchException(operationToReport,
                              string.Format("Patch failed: property at location from: {0} does not exist", location),
                                objectToGetValueFrom, 422);
                            }
                        }
                        else
                        {
                            throw new JsonPatchException(operationToReport,
                                string.Format("Patch failed: property at location from: {0} does not exist", location),
                                  objectToGetValueFrom, 422);
                        }


                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                                 string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", location),
                                   objectToGetValueFrom, 422);
                    }

                }
                else
                {
                    if (patchProperty.Property.Readable)
                    {
                        valueAtLocation = patchProperty.Property.ValueProvider
                                .GetValue(patchProperty.Parent);
                    }
                    else
                    {
                        throw new JsonPatchException(operationToReport,
                       string.Format("Patch failed: property at location from: {0} does not exist or cannot be accessed.", location),
                       objectToGetValueFrom, 422);
                    }
                }
            }

            return valueAtLocation;
        }


     

    }


}
