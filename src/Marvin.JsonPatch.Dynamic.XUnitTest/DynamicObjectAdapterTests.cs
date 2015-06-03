using Marvin.JsonPatch.Dynamic;
using Marvin.JsonPatch.Dynamic.Exceptions;
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

            Assert.Equal(1, newObject.Nested.NewInt);
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

            Assert.Equal(1, newObject.Nested.NewInt);
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


            var result = deserialized.CreateFrom(obj);

            Assert.Equal(valueToAdd.IntValue, result.ComplexProperty.IntValue);
            Assert.Equal(valueToAdd.StringValue, result.ComplexProperty.StringValue);
            Assert.Equal(valueToAdd.GuidValue, result.ComplexProperty.GuidValue);
            Assert.Equal(1, result.Test); 

        }
 

        [Fact]
        public void AddResultsShouldReplace()
        {
            var doc = new 
            {
                StringProperty = "A"
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("StringProperty", "B");


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            var result = deserialized.CreateFrom(doc);

            Assert.Equal("B", result.StringProperty);

        }


     


        [Fact]
        public void AddToList()
        {
            var doc = new 
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4, 0);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var result = deserialized.CreateFrom(doc);

            Assert.Equal(new List<int>() { 4, 1, 2, 3 }, result.IntegerList);
        }

 



        [Fact]
        public void AddToListInvalidPositionTooLarge()
        {
            var doc = new 
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4, 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            Assert.Throws<JsonPatchException>(() => { deserialized.CreateFrom(doc); });
        }

 



        [Fact]
        public void AddToListAtEndWithSerialization()
        {
            var doc = new
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4, 3);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            var result = deserialized.CreateFrom(doc);

            Assert.Equal(new List<int>() { 1, 2, 3, 4 }, result.IntegerList);

        }


         


        [Fact]
        public void AddToListAtBeginning()
        {
            var doc = new 
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4, 0);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            
            var result = deserialized.CreateFrom(doc);

            Assert.Equal(new List<int>() { 4, 1, 2, 3 }, result.IntegerList);


        }

         
        [Fact]
        public void AddToListInvalidPositionTooSmall()
        {

            var doc = new  
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4, -1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => {  deserialized.CreateFrom(doc); });

        }
         



        [Fact]
        public void AddToListAppend()
        {
            var doc = new
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            var result = deserialized.CreateFrom(doc);

            Assert.Equal(new List<int>() { 1, 2, 3, 4 }, result.IntegerList);

        }

    }
}
