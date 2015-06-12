// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Helpers;
using Marvin.JsonPatch.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    public class DynamicObjectAdapter : IDynamicObjectAdapter
    {

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
                positionAsInteger = PropertyHelpers.GetNumericEnd(path);

                if (positionAsInteger > -1)
                {
                    actualPathToProperty = path.Substring(0,
                        path.LastIndexOf('/' + positionAsInteger.ToString()));
                }
            }


            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty);

            if (result.UseDynamicLogic)
            {
                if (result.IsValidPathForAdd)
                {
                    if (result.Container.ContainsKeyCaseInsensitive(result.PropertyPathInParent))
                    {
                        // Existing property.  
                        // If it's not an array, we need to check if the value fits the property type
                        // 
                        // If it's an array, we need to check if the value fits in that array type,
                        // and add it at the correct position (if allowed).

                        if (appendList || positionAsInteger > -1)
                        {
                            // get the actual type
                            //  var typeOfPathProperty = containerDictionary[finalPath].GetType();

                            var typeOfPathProperty = result.Container
                                .GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();

                            var isNonStringArray = !(typeOfPathProperty == typeof(string))
                                && typeof(IList).GetTypeInfo().IsAssignableFrom(typeOfPathProperty);

                            // what if it's an array but there's no position??
                            if (isNonStringArray)
                            {
                                // now, get the generic type of the enumerable
                                var genericTypeOfArray = DynamicPropertyHelpers.GetEnumerableType(typeOfPathProperty);
                                var conversionResult = DynamicPropertyHelpers.ConvertToActualType(genericTypeOfArray, value);

                                if (!conversionResult.CanBeConverted)
                                {
                                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
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
                                        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                   path),
                                   objectToApplyTo, 422);
                                    }
                                }



                            }
                            else
                            {
                                throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                                   path),
                                   objectToApplyTo, 422);
                            }
                        }
                        else
                        {
                            // get the actual type

                            var typeOfPathProperty = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();

                            // var typeOfPathProperty = containerDictionary[finalPath].GetType();

                            // can the value be converted to the actual type
                            var conversionResultTuple =
                                DynamicPropertyHelpers.ConvertToActualType(typeOfPathProperty, value);

                            // conversion successful
                            if (conversionResultTuple.CanBeConverted)
                            {
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInParent,
                                    conversionResultTuple.ConvertedInstance);
                            }
                            else
                            {
                                throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
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
                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                    string.Format("Patch failed: cannot add to the parent of the property at location path: {0}.  To be able to dynamically add properties, the parent must be an ExpandoObject.", path),
                    objectToApplyTo, 422);
                }
            }
            else
            {
                if (!result.IsValidPathForAdd)
                {
                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                      string.Format("Patch failed: the provided path is invalid: {0}.", path),
                      objectToApplyTo, 422);
                }

                var pathProperty = result.PropertyInfo;

                // If it' an array, add to that array.  If it's not, we replace.

                // is the path an array (but not a string (= char[]))?  In this case,
                // the path must end with "/position" or "/-", which we already determined before.

                if (appendList || positionAsInteger > -1)
                {

                    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
                        && typeof(IList).IsAssignableFrom(pathProperty.PropertyType);

                    if (isNonStringArray)
                    {
                        // now, get the generic type of the enumerable
                        var genericTypeOfArray = PropertyHelpers.GetEnumerableType(pathProperty.PropertyType);

                        var conversionResult = PropertyHelpers.ConvertToActualType(genericTypeOfArray, value);

                        if (!conversionResult.CanBeConverted)
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                           string.Format("Patch failed: provided value is invalid for array property type at location path: {0}",
                           path),
                           objectToApplyTo, 422);
                        }

                        // get value (it can be cast, we just checked that)
                        var getResult = PropertyHelpers.GetValue(pathProperty, result.ParentObject, result.PropertyPathInParent);

                        IList array;

                        if (getResult.CanGet)
                        {
                            array = getResult.Value as IList;
                        }
                        else
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.",
                                path),
                                objectToApplyTo, 422);
                        }




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
                                throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                           path),
                           objectToApplyTo, 422);
                            }
                        }



                    }
                    else
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                          string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                          path),
                          objectToApplyTo, 422);
                    }
                }
                else
                {
                    var conversionResultTuple = PropertyHelpers.ConvertToActualType(pathProperty.PropertyType, value);

                    // conversion successful
                    if (conversionResultTuple.CanBeConverted)
                    {
                        var setResult = PropertyHelpers.SetValue(pathProperty, result.ParentObject, result.PropertyPathInParent,
                            conversionResultTuple.ConvertedInstance);

                        if (!(setResult.CanSet))
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                           string.Format("Patch failed: property at path location cannot be set: {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).",
                           path),
                           objectToApplyTo, 422);
                        }
                    }
                    else
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                       string.Format("Patch failed: provided value is invalid for property type at location path: {0}",
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
                positionAsInteger = PropertyHelpers.GetNumericEnd(path);

                if (positionAsInteger > -1)
                {
                    actualPathToProperty = path.Substring(0,
                        path.LastIndexOf('/' + positionAsInteger.ToString()));
                }
            }


            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty);

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

                        var isNonStringArray = !(typeOfPathProperty == typeof(string))
                            && typeof(IList).IsAssignableFrom(typeOfPathProperty);

                        if (isNonStringArray)
                        {
                            // now, get the generic type of the enumerable
                            var genericTypeOfArray = DynamicPropertyHelpers.GetEnumerableType(typeOfPathProperty);
 
                            var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent) as IList;

                            if (removeFromList)
                            {
                                if (array.Count == 0)
                                {
                                    // if the array is empty, we should throw an error
                                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
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
                                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                   string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                   path),
                                   objectToApplyTo, 422);
                                }

                            }
                        }
                        else
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
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

                        // TODO gettype!
                        return actualType;

                    }

                }
                else
                {
                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                 string.Format("Patch failed: cannot remove property at location path: {0}.  To be able to dynamically remove properties, the parent must be an ExpandoObject.", path),
                 objectToApplyTo, 422);
                }
            }
            else
            {
                // not dynamic

                if (!result.IsValidPathForRemove)
                {
                    throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                      string.Format("Patch failed: the provided path is invalid: {0}.", path),
                      objectToApplyTo, 422);
                }

                var pathProperty = result.PropertyInfo;

                if (removeFromList || positionAsInteger > -1)
                {

                    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
                        && typeof(IList).IsAssignableFrom(pathProperty.PropertyType);

                    // what if it's an array but there's no position??
                    if (isNonStringArray)
                    {

                        // now, get the generic type of the enumerable
                        var genericTypeOfArray = PropertyHelpers.GetEnumerableType(pathProperty.PropertyType);

                        // get value (it can be cast, we just checked that)
                        // var getResult = PropertyHelpers.GetValue(pathProperty, objectToApplyTo, actualPathToProperty);
                        var getResult = PropertyHelpers.GetValue(pathProperty, result.ParentObject, result.PropertyPathInParent);

                        IList array;

                        if (getResult.CanGet)
                        {
                            array = getResult.Value as IList;
                        }
                        else
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                string.Format("Patch failed: cannot get property value at path {0}.  Possible cause: the property doesn't have an accessible getter.",
                                path),
                                objectToApplyTo, 422);
                        }

                        if (removeFromList)
                        {
                            if (array.Count == 0)
                            {
                                // if the array is empty, we should throw an error
                                throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
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
                                throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                                 string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
                                 path),
                                 objectToApplyTo, 422);
                            }
                        }

                    }
                    else
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
                           path),
                           objectToApplyTo, 422);
                    }
                }
                else
                {

                    var setResult = PropertyHelpers.SetValue(pathProperty, result.ParentObject, result.PropertyPathInParent,
                         null);
                    
                    if (!(setResult.CanSet))
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
                       string.Format("Patch failed: property at path location cannot be removed (set to default/null for non-dynamic properties): {0}.  Possible causes: the property may not have an accessible setter, or the property may be part of an anonymous object (and thus cannot be changed after initialization).",
                       path),
                       objectToApplyTo, 422);
                    }
                    else
                    {
                        return setResult.PropertyToSet.PropertyType;
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
                throw new Dynamic.Exceptions.JsonPatchException(operation,
                     string.Format("Patch failed: property value cannot be converted to type of path location {0}",
                     operation.path),
                     objectToApplyTo, 422);
            }
          

        }




        public void Move(Operation operation, dynamic objectToApplyTo)
        {

            // get value at from location
            object valueAtFromLocation = null;
            var positionAsInteger = -1;
            var actualFromProperty = operation.from;

            positionAsInteger = PropertyHelpers.GetNumericEnd(operation.from);

            if (positionAsInteger > -1)
            {
                actualFromProperty = operation.from.Substring(0,
                    operation.from.LastIndexOf('/' + positionAsInteger.ToString()));
            }

            // get the property at the from location.

            // first, analyze the tree.

            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualFromProperty);

            if (result.UseDynamicLogic)
            {
                // find the property
                if (result.Container.ContainsKeyCaseInsensitive(result.PropertyPathInParent))
                {
                    if (positionAsInteger > -1)
                    {
                        // get the actual type

                        var typeOfPathProperty = result.Container
                            .GetValueForCaseInsensitiveKey(result.PropertyPathInParent).GetType();

                        var isNonStringArray = !(typeOfPathProperty == typeof(string))
                            && typeof(IList).GetTypeInfo().IsAssignableFrom(typeOfPathProperty);

                        if (isNonStringArray)
                        {
                            // now, get the generic type of the enumerable
                            var genericTypeOfArray = DynamicPropertyHelpers.GetEnumerableType(typeOfPathProperty);

                            // get value
                            var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent) as IList;

                            if (positionAsInteger < array.Count)
                            {
                                valueAtFromLocation = array[positionAsInteger];
                            }
                            else
                            {
                                throw new Dynamic.Exceptions.JsonPatchException(operation,
                              string.Format("Patch failed: property at location from: {0} does not exist", operation.from),
                                objectToApplyTo, 422);
                            }

                        }
                        else
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operation,
                                  string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", operation.from),
                                    objectToApplyTo, 422);
                        }
                    }
                    else
                    {
                        // get the value
                        valueAtFromLocation =
                            result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInParent);
                    }
                }
                else
                {
                    throw new Dynamic.Exceptions.JsonPatchException(operation,
                    string.Format("Patch failed: property at location from: {0} does not exist", operation.from),
                    objectToApplyTo, 422);
                }
            }
            else
            {

                // not dynamic.

                var pathProperty = result.PropertyInfo;

                // is the path an array (but not a string (= char[]))?  In this case,
                // the path must end with "/position" or "/-", which we already determined before.

                if (positionAsInteger > -1)
                {

                    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
                        && typeof(IList).IsAssignableFrom(pathProperty.PropertyType);

                    if (isNonStringArray)
                    {
                        // now, get the generic type of the enumerable
                        var genericTypeOfArray = PropertyHelpers.GetEnumerableType(pathProperty.PropertyType);

                        // get value (it can be cast, we just checked that)
                        var getResult = PropertyHelpers.GetValue(pathProperty,
                            result.ParentObject, result.PropertyPathInParent);

                        IList array;

                        if (getResult.CanGet)
                        {
                            array = getResult.Value as IList;
                        }
                        else
                        {
                            throw new Dynamic.Exceptions.JsonPatchException(operation,
                                 string.Format("Patch failed: property at location from: {0} does not exist", operation.from),
                                   objectToApplyTo, 422);
                        }

                        // specified index must not be greater than the amount of items in the
                        // array
                        if (positionAsInteger < array.Count)
                        {
                            valueAtFromLocation = array[positionAsInteger];
                        }
                        else
                        {

                            throw new Dynamic.Exceptions.JsonPatchException(operation,
                          string.Format("Patch failed: property at location from: {0} does not exist", operation.from),
                            objectToApplyTo, 422);
                        }
                    }
                    else
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operation,
                                 string.Format("Patch failed: provided from path is invalid for array property type at location from: {0}: expected array", operation.from),
                                   objectToApplyTo, 422);
                    }


                }
                else
                {
                    var getResult = PropertyHelpers.GetValue(pathProperty,
                        result.ParentObject, result.PropertyPathInParent);

                    if (getResult.CanGet)
                    {
                        valueAtFromLocation = getResult.Value;
                    }
                    else
                    {
                        throw new Dynamic.Exceptions.JsonPatchException(operation,
                       string.Format("Patch failed: property at location from: {0} does not exist or cannot be accessed.", operation.from),
                       objectToApplyTo, 422);
                    }
                }
            }


            // remove that value

            Remove(operation.from, objectToApplyTo, operation);

            // add that value to the path location

            Add(operation.path, valueAtFromLocation, objectToApplyTo, operation);

        }
    }
}
