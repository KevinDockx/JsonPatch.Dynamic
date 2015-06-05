using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Helpers;
using Marvin.JsonPatch.Operations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Adapters
{
    public class MixedObjectAdapter : IDynamicObjectAdapter
    {

        /// <summary>
        /// Add is used by various operations (eg: add, copy, ...), yet through different operations;
        /// This method allows code reuse yet reporting the correct operation on error
        /// </summary>
        private void Add(string path, object value, object objectToApplyTo, Operation operationToReport)
        {
            // add, in this implementation, CAN add propertys if the container is an
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


            var pathProperty = PropertyHelpers
                .FindProperty(objectToApplyTo, actualPathToProperty);


            // does property at path exist?
            if (pathProperty == null)
            {
                // the propertyinfo does not exist.  This means the property truly doesn't 
                // exist, or the pathproperty is in fact an ExpandoObject.
                // This is where we need to add dynamic checks - if 
                // the container is an ExpandoObject, we can add the property nevertheless.

                // - find container
                // - check if container is ExpandoObject
                // - if it is, check if we can add the property (eg: the "root" = prop-1 must exist)

                var containerResult = DynamicPropertyHelpers.FindContainerForPath(objectToApplyTo, actualPathToProperty);

                if (containerResult.IsValidContainer)
                {

                    containerResult.Container.Add(containerResult.PathToPropertyInContainer, value);
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
                        var array = PropertyHelpers.GetValue(pathProperty, objectToApplyTo, actualPathToProperty) as IList;


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
                        PropertyHelpers.SetValue(pathProperty, objectToApplyTo, actualPathToProperty,
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
        }


        public void Add(JsonPatch.Operations.Operation operation, dynamic objectToApplyTo)
        {
            Add(operation.path, operation.value, objectToApplyTo, operation);
        }
    }
}
