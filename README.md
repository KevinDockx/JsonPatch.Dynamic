# JSON Patch for .NET Dynamic Typing Support 
Support for dynamically typed objects for Marvin.JsonPatch (Json Patch Document RFC 6902 implementation for .NET)

Marvin.JsonPatch.Dynamic adds support for dynamically typed objects to Marvin.JsonPatch (https://github.com/KevinDockx/JsonPatch).  

Marvin.JsonPatch was built to work on staticly typed objects, which is great for most cases.  Yet sometimes you'll want to create a patch document without having a class to start from  (for example: when integrating with a backend that's out of your control, or when you don't have a shared DTO layer), or you'll want to apply a JsonPatchDocument to a dynamically typed object.

That's what this component takes care of.  It extends Marvin.JsonPatch with new methods on JsonPatchDocument, and it allows you to apply the JsonPatchDocument to dynamically typed objects.

At client level (on full .NET), you can now create JsonPatchDocuments without knowing the class it will be applied to.  That's a typical use case when you're working with dynamically typed objects at API level: you might not have a shared DTO layer between your API and the client.


```csharp
JsonPatchDocument patchDoc = new JsonPatchDocument();
patchDoc.Add("Age", 34);

// serialize
var serializedItemToUpdate = JsonConvert.SerializeObject(patchDoc);

// create the patch request
var method = new HttpMethod("PATCH");
var request = new HttpRequestMessage(method, "api/persons/" + id)
{
    Content = new StringContent(serializedItemToUpdate,
    System.Text.Encoding.Unicode, "application/json")
};

// send it, using an HttpClient instance
client.SendAsync(request);
```

Applying the previously created JsonPatchDocument at client side at API level, using the ObjectAdapter from the Marvin.JsonPatch.Dynamic namespace, will result in an extra property on the Person object.  The non-generic JsonPatchDocument used this adapter by default, but if you wish, you can also pass in an instance of Marvin.JsonPatch.Dynamic.ObjectAdapter to the ApplyTo-method of the generic JsonPatchDocument.  

```csharp
[Route("api/expenses/{id}")]
[HttpPatch]
public IHttpActionResult Patch(int id, [FromBody]JsonPatchDocument personPatchDocument)
{
      // get the person 
      dynamic person = _repository.GetDynamicPersonWithNameAndFirstName(id);

      // apply the patch document 
      personPatchDocument.ApplyTo(person);
      
      // person now has an extra property, Age, with value 34

}
```


As Marvin.JsonPatch.Dynamic is an extension to Marvin.JsonPatch, it can also be used to target any object Marvin.JsonPatch can target.

The fact that you can now also work with dynamics, which allows manipulating the property dictionary at runtime, means that you can now effectively add/remove properties to/from an object at runtime by applying the JsonPatchDocument to it.

Implemented are Add, Remove, Move, Replace and Copy.

It also works for nested objects, arrays and objects in an array.  

For example, to add a property Street to an object Address for the second person in an array of people, this should be your patch document:

```csharp
JsonPatchDocument patchDoc = new JsonPatchDocument();
patchDoc.Add("People/1/Address/Street", "My street");
```
