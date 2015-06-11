using Marvin.JsonPatch.Dynamic.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class RemoveOperationTests
    {


        [Fact]
        public void RemovePropertyShouldFailIfRootIsAnonymous()
        {

            dynamic doc = new
            {
                Test = 1
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Remove("Test");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }


        [Fact]
        public void RemovePropertyShouldFailIfItDoesntExist()
        {

            dynamic doc = new ExpandoObject();
            doc.Test = 1; 

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Remove("NonExisting");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }


         
        [Fact]
        public void RemovePropertyFromExpandoObject()
        {

            dynamic obj = new ExpandoObject();
            obj.Test = 1; 

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Remove("Test");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserialized.ApplyTo(obj);

            var cont = obj as IDictionary<string, object>;

            object valueFromDictionary;

            cont.TryGetValue("Test", out valueFromDictionary);
            
            Assert.Null(valueFromDictionary);

        }



    }
}
