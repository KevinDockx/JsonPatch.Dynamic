using Marvin.JsonPatch.Dynamic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class DynamicObjectAdapterTests
    {


        [Fact]
        public void AddNewProperty()
        {

            dynamic obj = new
            {
                Test = 1
            }; 

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add<int>("NewInt", 1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var newObject = deserialized.CreateFrom(obj);

            Assert.Equal(1, newObject.newint);
            Assert.Equal(1, newObject.Test);


        }

        [Fact]
        public void AddNewPropertyToNestedObject()
        {

            dynamic obj = new
            {
                
                Test = 1,
                nested = new {}
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add<int>("Nested/NewInt", 1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var newObject = deserialized.CreateFrom(obj);
            
            Assert.Equal(1, newObject.nested.newint);
            Assert.Equal(1, newObject.Test);

        }



        [Fact]
        public void AddNewNestedProperty()
        {

            dynamic obj = new
            {
                Test = 1
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("/Nested/NewInt", 1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var newObject = deserialized.CreateFrom(obj);

            Assert.Equal(1, newObject.nested.newint);
            Assert.Equal(1, newObject.Test);


        }



        [Fact]
        public void AddClomplexValue()
        {


            dynamic obj = new
            {
                Test = 1
            };

            dynamic valueToAdd = new { IntValue = 1, StringValue = "test", GuidValue = Guid.NewGuid() };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add<dynamic>("ComplexProperty", valueToAdd);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var newObject = deserialized.CreateFrom(obj);

            Assert.Equal(valueToAdd, newObject.complexproperty);
            Assert.Equal(1, newObject.Test);


        }


    }
}
