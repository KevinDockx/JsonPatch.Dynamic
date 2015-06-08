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
                        path.IndexOf('/' + positionAsInteger.ToString()));
                }
            }


            var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty);

            if (result.UseDynamicLogic)
            {
                if (result.IsValidPathForAdd)
                { 
                    if (result.Container.ContainsKeyCaseInsensitive(result.PropertyPathInContainer))
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
                                .GetValueForCaseInsensitiveKey(result.PropertyPathInContainer).GetType();

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

                                var array = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInContainer) as IList;

                                if (appendList)
                                {
                                    array.Add(conversionResult.ConvertedInstance);
                                    result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInContainer, array);
                                }
                                else
                                {
                                    // specified index must not be greater than the amount of items in the
                                    // array
                                    if (positionAsInteger <= array.Count)
                                    {
                                        array.Insert(positionAsInteger, conversionResult.ConvertedInstance);
                                        result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInContainer, array);
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

                            var typeOfPathProperty = result.Container.GetValueForCaseInsensitiveKey(result.PropertyPathInContainer).GetType();

                            // var typeOfPathProperty = containerDictionary[finalPath].GetType();

                            // can the value be converted to the actual type
                            var conversionResultTuple =
                                DynamicPropertyHelpers.ConvertToActualType(typeOfPathProperty, value);

                            // conversion successful
                            if (conversionResultTuple.CanBeConverted)
                            {
                                result.Container.SetValueForCaseInsensitiveKey(result.PropertyPathInContainer, 
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
                        result.Container.Add(result.PropertyPathInContainer, value);

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


            //// does property at path exist?
            //if (pathProperty == null)
            //{
            //    // the propertyinfo does not exist.  This means the property truly doesn't 
            //    // exist, or the pathproperty is in fact an ExpandoObject.
            //    // This is where we need to add dynamic checks - if 
            //    // the container is an ExpandoObject, we can add the property nevertheless.

            //    // - find container
            //    // - check if container is ExpandoObject
            //    // - if it is, check if we can add the property (eg: the "root" = prop-1 must exist)

            //    var containerResult = DynamicPropertyHelpers.FindContainerForPath(objectToApplyTo, actualPathToProperty);

            //    if (containerResult.IsValidContainer)
            //    {

            //        containerResult.Container.Add(containerResult.PathToPropertyInContainer, value);
            //    }
            //    else
            //    {
            //        throw new Dynamic.Exceptions.JsonPatchException(operationToReport,
            //        string.Format("Patch failed: cannot add to the parent of the property at location path: {0}.  To be able to dynamically add properties, the parent must be an ExpandoObject.", path),
            //        objectToApplyTo, 422);
            //    }
            //}
            //else
            //{


                // it exists.  If it' an array, add to that array.  If it's not, we replace.

                // is the path an array (but not a string (= char[]))?  In this case,
                // the path must end with "/position" or "/-", which we already determined before.

                if (appendList || positionAsInteger > -1)
                {

                    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
                        && typeof(IList).IsAssignableFrom(pathProperty.PropertyType);

                    // what if it's an array but there's no position??
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
                        var getResult = PropertyHelpers.GetValue(pathProperty, objectToApplyTo, actualPathToProperty);

                        IList array;

                        if (getResult.CanGet)
                        {
                            array =  getResult.Value as IList;
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
                        var setResult = PropertyHelpers.SetValue(pathProperty, objectToApplyTo, actualPathToProperty,
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
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Remove(string path, object objectToApplyTo, Operation operationToReport)
        {
            //// remove, in this implementation, CAN remove properties if the container is an
            //// ExpandoObject.

            //var removeFromList = false;
            //var positionAsInteger = -1;
            //var actualPathToProperty = path;

            //if (path.EndsWith("/-"))
            //{
            //    removeFromList = true;
            //    actualPathToProperty = path.Substring(0, path.Length - 2);
            //}
            //else
            //{
            //    positionAsInteger = PropertyHelpers.GetNumericEnd(path);

            //    if (positionAsInteger > -1)
            //    {
            //        actualPathToProperty = path.Substring(0,
            //            path.IndexOf('/' + positionAsInteger.ToString()));
            //    }
            //}

            //var pathProperty = PropertyHelpers
            //    .FindProperty(objectToApplyTo, actualPathToProperty);


            //   var result = new ObjectTreeAnalysisResult(objectToApplyTo, actualPathToProperty);

            //   if (result.UseDynamicLogic)
            //   {

            //   }

            //// does the target location exist?
            //if (pathProperty == null)
            //{
            //    throw new JsonPatchException<T>(operationToReport,
            //        string.Format("Patch failed: property at location path: {0} does not exist", path),
            //        objectToApplyTo, 422);
            //}

            //// get the property, and remove it - in this case, for DTO's, that means setting
            //// it to null or its default value; in case of an array, remove at provided index
            //// or at the end.


            //if (removeFromList || positionAsInteger > -1)
            //{

            //    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
            //        && typeof(IList).IsAssignableFrom(pathProperty.PropertyType);

            //    // what if it's an array but there's no position??
            //    if (isNonStringArray)
            //    {
            //        // now, get the generic type of the enumerable
            //        var genericTypeOfArray = PropertyHelpers.GetEnumerableType(pathProperty.PropertyType);

            //        // TODO: nested!
            //        // get value (it can be cast, we just checked that)
            //        var array = PropertyHelpers.GetValue(pathProperty, objectToApplyTo, actualPathToProperty) as IList;

            //        if (removeFromList)
            //        {
            //            array.RemoveAt(array.Count - 1);
            //        }
            //        else
            //        {
            //            if (positionAsInteger < array.Count)
            //            {
            //                array.RemoveAt(positionAsInteger);
            //            }
            //            else
            //            {
            //                throw new JsonPatchException<T>(operationToReport,
            //           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
            //           path),
            //           objectToApplyTo, 422);
            //            }
            //        }

            //    }
            //    else
            //    {
            //        throw new JsonPatchException<T>(operationToReport,
            //           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
            //           path),
            //           objectToApplyTo, 422);
            //    }
            //}
            //else
            //{

            //    // setting the value to "null" will use the default value in case of value types, and
            //    // null in case of reference types
            //    PropertyHelpers.SetValue(pathProperty, objectToApplyTo, actualPathToProperty, null);
            //}

        }



    }
}
