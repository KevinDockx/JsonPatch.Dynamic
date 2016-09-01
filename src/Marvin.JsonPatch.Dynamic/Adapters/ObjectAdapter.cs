// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Reflection;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    public class ObjectAdapter : IObjectAdapter
    {
        public IContractResolver ContractResolver { get; private set; }
        public ObjectAdapter()
        {
            ContractResolver = new DefaultContractResolver();
        }

        public ObjectAdapter(IContractResolver contractResolver)
        {
            ContractResolver = contractResolver;
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

            // get path result
            var pathResult = PropertyHelpers.GetActualPropertyPath(
                path,
                objectToApplyTo,
                operationToReport, 
                true);

            var appendList = pathResult.ExecuteAtEnd;
            var positionAsInteger = pathResult.NumericEnd;
            var actualPathToProperty = pathResult.PathToProperty;

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
                                    throw new JsonPatchException(
                                        new JsonPatchError(
                                            objectToApplyTo,
                                            operationToReport,
                                            string.Format("Patch failed: provided value is invalid for array property type at location path: {0}", path)),
                                        422);
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
                                    // specified index must not be greater than 
                                    // the amount of items in the array
                                    if (positionAsInteger > array.Count)
                                    {
                                        throw new JsonPatchException(
                                          new JsonPatchError(
                                              objectToApplyTo,
                                              operationToReport,
                                              string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                          422);                                     
                                    }

                                    array.Insert(positionAsInteger, conversionResult.ConvertedInstance);
                                    result.Container.SetValueForCaseInsensitiveKey(
                                        result.PropertyPathInParent, array);
                                }
                            }
                            else
                            {
                                throw new JsonPatchException(
                                             new JsonPatchError(
                                                 objectToApplyTo,
                                                 operationToReport,
                                                 string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array", path)),
                                             422);  
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
                                throw new JsonPatchException(
                                               new JsonPatchError(
                                                   objectToApplyTo,
                                                   operationToReport,
                                                   string.Format("Patch failed: provided value is invalid for property type at location path: {0}", path)),
                                               422);  
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
                    throw new JsonPatchException(
                        new JsonPatchError(
                            objectToApplyTo,
                            operationToReport,
                            string.Format("Patch failed: cannot add to the parent of the property at location path: {0}.  To be able to dynamically add properties, the parent must be an ExpandoObject.", path)),
                        422); 
                }
            }
            else
            {
                if (!result.IsValidPathForAdd)
                {
                    throw new JsonPatchException(
                           new JsonPatchError(
                               objectToApplyTo,
                               operationToReport,
                               string.Format("Patch failed: the provided path is invalid: {0}.", path)),
                           422); 
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
                            throw new JsonPatchException(
                              new JsonPatchError(
                                  objectToApplyTo,
                                  operationToReport,
                                  string.Format("Patch failed: provided value is invalid for array property type at location path: {0}", path)),
                              422);                         
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
                                    throw new JsonPatchException(
                                        new JsonPatchError(
                                          objectToApplyTo,
                                          operationToReport,
                                          string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                        422);                                    
                                }
                            }
                        }
                        else
                        {    
                            // cannot read the property
                            throw new JsonPatchException(
                                new JsonPatchError(
                                  objectToApplyTo,
                                  operationToReport,
                                  string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.", path)),
                                422); 
                        }
                    }
                    else
                    {
                        throw new JsonPatchException(
                            new JsonPatchError(
                              objectToApplyTo,
                              operationToReport,
                              string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array", path)),
                            422);
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
                            throw new JsonPatchException(
                               new JsonPatchError(
                                 objectToApplyTo,
                                 operationToReport,
                                 string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).", path)),
                               422);
                        }
                    }
                    else
                    {
                        throw new JsonPatchException(
                                  new JsonPatchError(
                                    objectToApplyTo,
                                    operationToReport,
                                    string.Format("Patch failed: property value cannot be converted to type of path location {0}.", path)),
                                  422);
                    }
                }
            }
        }

        public void Add(JsonPatch.Operations.Operation operation, object objectToApplyTo)
        {
            Add(operation.path, operation.value, objectToApplyTo, operation);
        }

        public void Remove(Operation operation, object objectToApplyTo)
        {
            Remove(operation.path, objectToApplyTo, operation);
        }

        /// <summary>
        /// Remove is used by various operations (eg: remove, move, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error.  The return value
        /// contains the type of the item that has been removed (and a bool possibly signifying an error)
        /// This can be used by other methods, like replace, to ensure that we can pass in the correctly 
        /// typed value to whatever method follows.
        /// </summary>
        private RemovedPropertyTypeResult Remove(string path, object objectToApplyTo, Operation operationToReport)
        {
            // remove, in this implementation, CAN remove properties if the container is an
            // ExpandoObject.
            var pathResult = PropertyHelpers.GetActualPropertyPath(
               path,
               objectToApplyTo,
               operationToReport, 
               true);

            var removeFromList = pathResult.ExecuteAtEnd;
            var positionAsInteger = pathResult.NumericEnd;
            var actualPathToProperty = pathResult.PathToProperty; 

            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty, ContractResolver);

            if (result.UseDynamicLogic)
            {
                if (result.IsValidPathForRemove)
                {
                    // if it's not an array, we can remove the property from
                    // the dictionary.  If it's an array, we need to check the position first.
                    if (removeFromList || positionAsInteger > -1)
                    {
                        var valueOfPathProperty =  result.Container
                                .GetValueForCaseInsensitiveKey(result.PropertyPathInParent);

                        // we cannot continue when the value is null, because to be able to
                        // continue we need to be able to check if the array is a non-string array
                        if (valueOfPathProperty == null)
                        {                      
                            throw new JsonPatchException(
                               new JsonPatchError(
                                 objectToApplyTo,
                                 operationToReport,
                                 string.Format("Patch failed: cannot determine array property type at location path: {0}.", path)),
                               422); 
                        }
                        
                        var typeOfPathProperty = valueOfPathProperty.GetType();
                        

                        if (PropertyHelpers.IsNonStringArray(typeOfPathProperty))
                        {
                            // now, get the generic type of the enumerable
                            var genericTypeOfArray = PropertyHelpers.GetIListType(typeOfPathProperty);

                            var array = (IList)result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent);

                            if (removeFromList)
                            {
                                if (array.Count == 0)
                                {
                                    // if the array is empty, we should throw an error
                                    throw new JsonPatchException(
                                       new JsonPatchError(
                                         objectToApplyTo,
                                         operationToReport,
                                         string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                       422); 
                                }

                                array.RemoveAt(array.Count - 1);
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);

                                // return the type of the value that has been removed.
                                return new RemovedPropertyTypeResult(genericTypeOfArray, false);
                            }
                            else
                            {
                                if (positionAsInteger >= array.Count)
                                {
                                    throw new JsonPatchException(
                                         new JsonPatchError(
                                           objectToApplyTo,
                                           operationToReport,
                                           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                         422);
                                }

                                array.RemoveAt(positionAsInteger);
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent, array);

                                // return the type of the value that has been removed.
                                return new RemovedPropertyTypeResult(genericTypeOfArray, false);               
                            }
                        }
                        else
                        {
                            throw new JsonPatchException(
                                new JsonPatchError(
                                  objectToApplyTo,
                                  operationToReport,
                                  string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array", path)),
                                422);
                        }
                    }
                    else
                    {
                        // get the property
                        var getResult = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent);
                                          
                        // remove the property
                        result.Container.RemoveValueForCaseInsensitiveKey(result.PropertyPathInParent);

                        // value is not null, we can determine the type
                        if (getResult != null)
                        {
                            var actualType = getResult.GetType();
                            return new RemovedPropertyTypeResult(actualType, false);
                        }
                        else
                        {
                            return new RemovedPropertyTypeResult(null, false);
                        }                        
                    }
                }
                else
                {
                    throw new JsonPatchException(
                        new JsonPatchError(
                          objectToApplyTo,
                          operationToReport,
                          string.Format("Patch failed: cannot remove property at location path: {0}.  To be able to dynamically remove properties, the parent must be an ExpandoObject.", path)),
                        422);
                }
            }
            else
            {
                // not dynamic 
                if (!result.IsValidPathForRemove)
                {
                    throw new JsonPatchException(
                           new JsonPatchError(
                             objectToApplyTo,
                             operationToReport,
                             string.Format("Patch failed: the provided path is invalid: {0}.", path)),
                           422);
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
                                    throw new JsonPatchException(
                                        new JsonPatchError(
                                          objectToApplyTo,
                                          operationToReport,
                                          string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                        422);
                                }

                                array.RemoveAt(array.Count - 1);

                                // return the type of the value that has been removed
                                return new RemovedPropertyTypeResult(genericTypeOfArray, false);                            
                            }
                            else
                            {
                                if (positionAsInteger >= array.Count)
                                {
                                    throw new JsonPatchException(
                                            new JsonPatchError(
                                              objectToApplyTo,
                                              operationToReport,
                                              string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size", path)),
                                            422);
                                }
                                 
                                array.RemoveAt(positionAsInteger);

                                // return the type of the value that has been removed
                                return new RemovedPropertyTypeResult(genericTypeOfArray, false);                                  
                            }
                        }
                        else
                        {
                            throw new JsonPatchException(
                                 new JsonPatchError(
                                   objectToApplyTo,
                                   operationToReport,
                                   string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.", path)),
                                 422);
                        }
                    }
                    else
                    {
                        throw new JsonPatchException(
                             new JsonPatchError(
                               objectToApplyTo,
                               operationToReport,
                               string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array.", path)),
                             422);
                    }
                }
                else
                {
                    if (!patchProperty.Property.Writable)
                    {
                        throw new JsonPatchException(
                               new JsonPatchError(
                                 objectToApplyTo,
                                 operationToReport,
                                 string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).", path)),
                               422);
                    }

                    // setting the value to "null" will use the default value in case of value types, and
                    // null in case of reference types
                    object value = null;

                    if (patchProperty.Property.PropertyType.GetTypeInfo().IsValueType
                        && Nullable.GetUnderlyingType(patchProperty.Property.PropertyType) == null)
                    {
                        value = Activator.CreateInstance(patchProperty.Property.PropertyType);
                    }

                    patchProperty.Property.ValueProvider.SetValue(patchProperty.Parent, value);
                    return new RemovedPropertyTypeResult(patchProperty.Property.PropertyType, false);                        
                }
            }
        }

        public void Replace(Operation operation, object objectToApplyTo)
        {
            var removeResult = Remove(operation.path, objectToApplyTo, operation);

            if (removeResult.HasError)
            {
                // return => currently not applicable, will throw exception in Remove method
            }

            if (!removeResult.HasError && removeResult.ActualType == null)
            {
                // the remove operation completed succesfully, but we could not determine the type.  
                throw new JsonPatchException(
                   new JsonPatchError(
                     objectToApplyTo,
                     operation,
                     string.Format("Patch failed: could not determine type of property at location {0}", operation.path)),
                   422); 
            }

            var conversionResult = PropertyHelpers.ConvertToActualType(removeResult.ActualType, operation.value);

            if (!conversionResult.CanBeConverted)
            {
                throw new JsonPatchException(
                   new JsonPatchError(
                     objectToApplyTo,
                     operation,
                     string.Format("Patch failed: property value cannot be converted to type of path location {0}", operation.path)),
                   422); 
            }
                       
            Add(operation.path, conversionResult.ConvertedInstance, objectToApplyTo, operation);           
        }

        public void Move(Operation operation, object objectToApplyTo)
        {
            var valueAtFromLocationResult = GetValueAtLocation(operation.from, objectToApplyTo, operation);

            if (valueAtFromLocationResult.HasError)
            {
                // currently not applicable, will throw exception in GetValueAtLocation method
            }

            // remove that value
            var removeResult = Remove(operation.from, objectToApplyTo, operation);

            if (removeResult.HasError)
            {
                // return => currently not applicable, will throw exception in Remove method
            }

            // add that value to the path location
            Add(operation.path, 
                valueAtFromLocationResult.PropertyValue, 
                objectToApplyTo, 
                operation);
        }

        public void Copy(Operation operation, object objectToApplyTo)
        {
            // get value at from location and add that value to the path location
            var valueAtFromLocationResult = GetValueAtLocation(operation.from, objectToApplyTo, operation);

            if (valueAtFromLocationResult.HasError)
            {
                // currently not applicable, will throw exception in GetValueAtLocation method
            }

            Add(operation.path, 
                valueAtFromLocationResult.PropertyValue, 
                objectToApplyTo, 
                operation);
        }

        private GetValueResult GetValueAtLocation(string location, object objectToGetValueFrom, Operation operationToReport)
        {
            // get value from "objectToGetValueFrom" at location "location"
           object valueAtLocation = null;

           var pathResult = PropertyHelpers.GetActualPropertyPath(
               location,
               objectToGetValueFrom,
               operationToReport, false);

            var positionAsInteger = pathResult.NumericEnd;
            var actualFromProperty = pathResult.PathToProperty;

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

                            if (positionAsInteger >= array.Count)
                            {
                                throw new JsonPatchException(
                                   new JsonPatchError(
                                     objectToGetValueFrom,
                                     operationToReport,
                                     string.Format("Patch failed: property at location from: {0} does not exist", location)),
                                   422); 
                            }
                                                     
                            valueAtLocation = array[positionAsInteger];                           
                        }
                        else
                        {
                            throw new JsonPatchException(
                                 new JsonPatchError(
                                   objectToGetValueFrom,
                                   operationToReport,
                                   string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", location)),
                                 422); 
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
                    throw new JsonPatchException(
                         new JsonPatchError(
                           objectToGetValueFrom,
                           operationToReport,
                           string.Format("Patch failed: property at location from: {0} does not exist.", location)),
                         422); 
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
                        if (patchProperty.Property.Readable)
                        {
                            var array = (IList)patchProperty.Property.ValueProvider
                                .GetValue(patchProperty.Parent);

                            if (positionAsInteger >= array.Count)
                            {
                                throw new JsonPatchException(
                                   new JsonPatchError(
                                     objectToGetValueFrom,
                                     operationToReport,
                                     string.Format("Patch failed: property at location from: {0} does not exist", location)),
                                   422); 
                            }
                          
                            valueAtLocation = array[positionAsInteger];                          
                        }
                        else
                        {
                            throw new JsonPatchException(
                                 new JsonPatchError(
                                   objectToGetValueFrom,
                                   operationToReport,
                                   string.Format("Patch failed: cannot get property at location from from: {0}. Possible cause: the property doesn't have an accessible getter.", location)),
                                 422);
                        }
                    }
                    else
                    {
                        throw new JsonPatchException(
                            new JsonPatchError(
                              objectToGetValueFrom,
                              operationToReport,
                               string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", location)),
                            422);
                    }
                }
                else
                {
                    if (!patchProperty.Property.Readable)
                    {
                        throw new JsonPatchException(
                                new JsonPatchError(
                                  objectToGetValueFrom,
                                  operationToReport,
                                   string.Format("Patch failed: cannot get property at location from from: {0}. Possible cause: the property doesn't have an accessible getter.", location)),
                                422);
                    } 
                    valueAtLocation = patchProperty.Property.ValueProvider
                                 .GetValue(patchProperty.Parent);
                }
            }
            return new GetValueResult(valueAtLocation, false);
        }

        public void Test(Operation operation, object objectToApplyTo)
        {
            throw new NotImplementedException("Test is currently not implemented");
        }
    }
}
