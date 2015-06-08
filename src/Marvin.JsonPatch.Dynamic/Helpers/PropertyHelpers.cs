// Kevin Dockx
//
// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic; 
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Marvin.JsonPatch.Helpers
{
    internal static class PropertyHelpers
    {


        public static GetValueResult GetValue(PropertyInfo propertyToGet, object targetObject, string pathToProperty)
        {
            // it is possible the path refers to a nested property.  In that case, we need to 
            // get from a different target object: the nested object.

            var splitPath = pathToProperty.Split('/');

            // skip the first one if it's empty
            var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

            for (int i = startIndex; i < splitPath.Length - 1; i++)
            {
                var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
                    , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                targetObject = propertyInfoToGet.GetValue(targetObject, null);
            }

            if (propertyToGet.CanRead)
            {
                    return new GetValueResult(propertyToGet, true, propertyToGet.GetValue(targetObject, null), true);
            }
            else
            {
                return new GetValueResult(propertyToGet, false, null, false);
            } 
        }




        public static SetValueResult SetValue(PropertyInfo propertyToSet, object targetObject, string pathToProperty, object value)
        {
            // it is possible the path refers to a nested property.  In that case, we need to 
            // set on a different target object: the nested object.


            var splitPath = pathToProperty.Split('/');

            // skip the first one if it's empty
            var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

            for (int i = startIndex; i < splitPath.Length - 1; i++)
            {
                var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
                    , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                targetObject = propertyInfoToGet.GetValue(targetObject, null);
            }

            if (propertyToSet.CanWrite)
            {
                propertyToSet.SetValue(targetObject, value, null);
                return new SetValueResult(propertyToSet, true, true);
            }
            else
            {
                return new SetValueResult(propertyToSet, false, true);
            }
                      
             
        }


        //public static object GetProperty(object o, string member)
        //{
        //    if (o == null) throw new ArgumentNullException("o");
        //    if (member == null) throw new ArgumentNullException("member");
        //    Type scope = o.GetType();
        //    IDynamicMetaObjectProvider provider = o as IDynamicMetaObjectProvider;
        //    if (provider != null)
        //    {
        //        ParameterExpression param = Expression.Parameter(typeof(object));
        //        DynamicMetaObject mobj = provider.GetMetaObject(param);
        //        GetMemberBinder binder = (GetMemberBinder)Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, member, scope, new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(0, null) });
        //        DynamicMetaObject ret = mobj.BindGetMember(binder);
        //        BlockExpression final = Expression.Block(
        //            Expression.Label(CallSiteBinder.UpdateLabel),
        //            ret.Expression
        //        );
        //        LambdaExpression lambda = Expression.Lambda(final, param);
        //        Delegate del = lambda.Compile();
        //        return del.DynamicInvoke(o);
        //    }
        //    else
        //    {
        //        return o.GetType().GetProperty(member, BindingFlags.Public | BindingFlags.Instance).GetValue(o, null);
        //    }
        //}

        public static PropertyInfo FindProperty(object targetObject, string propertyPath)
        {
            try
            {

                var splitPath = propertyPath.Split('/');

                // skip the first one if it's empty
                var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

                for (int i = startIndex; i < splitPath.Length - 1; i++)
                {
                    var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
                        , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    // todo: check if this targetobject is an expandoobject or not - if expando
                    // (or IM
                    targetObject = propertyInfoToGet.GetValue(targetObject, null);
                }


                var propertyToFind = targetObject.GetType().GetProperty(splitPath.Last(),
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                return propertyToFind;


            }
            catch (Exception)
            {
                // will result in JsonPatchException in calling class, as expected
                return null;
            }
        }


        internal static ConversionResult ConvertToActualType(Type propertyType, object value)
        {
            try
            {
                var o = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), propertyType);
                return new ConversionResult(true, o);
            }
            catch (Exception)
            {
                return new ConversionResult(false, null);
            }
        }


        internal static Type GetEnumerableType(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }
            return null;
        }



        internal static int GetNumericEnd(string path)
        {
            var possibleIndex = path.Substring(path.LastIndexOf("/") + 1);
            var castedIndex = -1;

            if (int.TryParse(possibleIndex, out castedIndex))
            {
                return castedIndex;
            }

            return -1;

        }


        private static PropertyInfo GetPropertyInfo(object targetObject, string propertyName,
        BindingFlags bindingFlags)
        {
            return targetObject.GetType().GetProperty(propertyName, bindingFlags);
        }
    }
}