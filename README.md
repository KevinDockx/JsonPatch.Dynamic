# JSON Patch for .NET Dynamic Typing Support 
Support for dynamically typed objects for Marvin.JsonPatch (Json Patch Document RFC 6902 implementation for .NET)

Marvin.JsonPatch.Dynamic adds support for dynamically typed objects to Marvin.JsonPatch (https://github.com/KevinDockx/JsonPatch).  

Marvin.JsonPatch was built to work on staticly typed objects, which is great for most cases.  Yet sometimes you'll want to create a patch document without having a static type to start from  (for example: when integrating with a backend that's out of your control), or you'll want to apply a JsonPatchDocument to a dynamic object, or an object that has a property which isn't statically typed. 

That's what this component takes care of.  It extends Marvin.JsonPatch with new methods on JsonPatchDocument, and it allows you to apply the JsonPatchDocument to dynamically typed objects.

The fact that you can now also work with dynamics, which allows manipulating the property dictionary at runtime, means that you can now effectively add/remove properties to/from an object at runtime by applying the JsonPatchDocument to it.

