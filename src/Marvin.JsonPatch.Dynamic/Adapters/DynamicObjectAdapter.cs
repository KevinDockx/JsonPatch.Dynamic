using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Operations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    public class DynamicObjectAdapter : IDynamicObjectAdapter 
    {

         
        public void Add(Operation operation, dynamic objectToApplyTo)
        {
            Add(operation.path, operation.value, objectToApplyTo, operation);
        }


        /// <summary>
        /// Add is used by various operations (eg: add, copy, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Add(string path, object value, dynamic objectToApplyTo, Operation operationToReport)
        {
           
            // first up: if the path ends in a numeric value, we're inserting in a list and
            // that value represents the position; if the path ends in "-", we're appending
            //// to the list.
            //http://stackoverflow.com/questions/2594527/how-do-i-iterate-over-the-properties-of-an-anonymous-object-in-c?lq=1
            //http://blog.jorgef.net/2011/06/converting-any-object-to-dynamic.html
            //http://stackoverflow.com/questions/10241776/cast-expandoobject-to-anonymous-type

            // object to apply to can be an anonymous type, expando, dynamicob, ... we cannot manipulate
            // all of those (anon = read only, for example).  So: instead of manipulating the object, we create a new one.


            var appendList = false;
            var positionAsInteger = -1;
            var actualPathToProperty = path;

            var propertyDictionary = (IDictionary<String, Object>)(objectToApplyTo);
          //  propertyDictionary.Add(path, value);


            if (path.EndsWith("/-"))
            {
                appendList = true;
                actualPathToProperty = path.Substring(0, path.Length - 2);
            }
            else
            {
                positionAsInteger = DynamicPropertyHelpers.GetNumericEnd(path);

                if (positionAsInteger > -1)
                {
                    actualPathToProperty = path.Substring(0,
                        path.IndexOf('/' + positionAsInteger.ToString()));
                }
            }

            // we need to check if the property at the path already exists, and if 
            // it doesn't exist, we need to create it.            

            var containerDictionary = DynamicPropertyHelpers
                .FindOrCreateContainerDictionary(propertyDictionary, actualPathToProperty);
 
            // add the value to the container dictionary.  We must ensure this value
            // is an ExpandoObject as well  TODO - serialize here, or use custom jsonconverter 
            // for performance?


            string finalPath = actualPathToProperty.Split('/').Last();
            if (containerDictionary.ContainsKey(finalPath))
            {
                // Existing property.  
                // If it's not an array, we need to check if the value fits the property type
                // 
                // If it's an array, we need to check if the value fits in that array type,
                // and add it at the correct position (if allowed).

                if (appendList || positionAsInteger > -1)
                {
                    // get the actual type
                    var typeOfPathProperty = containerDictionary[finalPath].GetType();

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
                        var array = containerDictionary[finalPath] as IList;


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
                    // get the actual type
                    var typeOfPathProperty = containerDictionary[finalPath].GetType();

                    // can the value be converted to the actual type
                    var conversionResultTuple = 
                        DynamicPropertyHelpers.ConvertToActualType(typeOfPathProperty, value);

                    // conversion successful
                    if (conversionResultTuple.CanBeConverted)
                    {
                        containerDictionary[finalPath] = conversionResultTuple.ConvertedInstance;
                        //PropertyHelpers.SetValue(pathProperty, objectToApplyTo, actualPathToProperty,
                        //    conversionResultTuple.ConvertedInstance);
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
                //dynamic valueAsExpandoObject = JsonConvert.DeserializeObject<ExpandoObject>
                //    (JsonConvert.SerializeObject(value));

                containerDictionary.Add(finalPath, value);

            }


            //// does property at path exist?
            //if (pathProperty == null)
            //{
            //    throw new JsonPatchException(operationToReport,
            //        string.Format("Patch failed: property at location path: {0} does not exist", path),
            //        objectToApplyTo, 422);
            //}

            //// it exists.  If it' an array, add to that array.  If it's not, we replace.

            //// is the path an array (but not a string (= char[]))?  In this case,
            //// the path must end with "/position" or "/-", which we already determined before.

            //if (appendList || positionAsInteger > -1)
            //{

            //    var isNonStringArray = !(pathProperty.PropertyType == typeof(string))
            //        && typeof(IList).GetTypeInfo().IsAssignableFrom(pathProperty.PropertyType);

            //    // what if it's an array but there's no position??
            //    if (isNonStringArray)
            //    {
            //        // now, get the generic type of the enumerable
            //        var genericTypeOfArray = PropertyHelpers.GetEnumerableType(pathProperty.PropertyType);

            //        var conversionResult = PropertyHelpers.ConvertToActualType(genericTypeOfArray, value);

            //        if (!conversionResult.CanBeConverted)
            //        {
            //            throw new JsonPatchException(operationToReport,
            //              string.Format("Patch failed: provided value is invalid for array property type at location path: {0}",
            //              path),
            //              objectToApplyTo, 422);
            //        }

            //        // get value (it can be cast, we just checked that)
            //        var array = PropertyHelpers.GetValue(pathProperty, objectToApplyTo, actualPathToProperty) as IList;


            //        if (appendList)
            //        {
            //            array.Add(conversionResult.ConvertedInstance);
            //        }
            //        else
            //        {
            //            // specified index must not be greater than the amount of items in the
            //            // array
            //            if (positionAsInteger <= array.Count)
            //            {
            //                array.Insert(positionAsInteger, conversionResult.ConvertedInstance);
            //            }
            //            else
            //            {
            //                throw new JsonPatchException(operationToReport,
            //           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: position larger than array size",
            //           path),
            //           objectToApplyTo, 422);
            //            }
            //        }



            //    }
            //    else
            //    {
            //        throw new JsonPatchException(operationToReport,
            //           string.Format("Patch failed: provided path is invalid for array property type at location path: {0}: expected array",
            //           path),
            //           objectToApplyTo, 422);
            //    }
            //}
            //else
            //{
            //    var conversionResultTuple = PropertyHelpers.ConvertToActualType(pathProperty.PropertyType, value);

            //    // conversion successful
            //    if (conversionResultTuple.CanBeConverted)
            //    {
            //        PropertyHelpers.SetValue(pathProperty, objectToApplyTo, actualPathToProperty,
            //            conversionResultTuple.ConvertedInstance);
            //    }
            //    else
            //    {
            //        throw new JsonPatchException(operationToReport,
            //        string.Format("Patch failed: provided value is invalid for property type at location path: {0}",
            //        path),
            //        objectToApplyTo, 422);
            //    }

            //}
        }


         
    }
}
