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
    public class RemoveTypedOperationTests
    {

       

        [Fact]
        public void Remove()
        {
            var doc = new SimpleDTO()
            {
                StringProperty = "A"
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Remove("StringProperty");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(null, doc.StringProperty);

        }

         


        [Fact]
        public void RemoveFromList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("IntegerList", 2);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2 }, doc.IntegerList);
        }

 

        [Fact]
        public void RemoveFromListInvalidPositionTooLarge()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("IntegerList", 3);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }
 
        [Fact]
        public void RemoveFromListInvalidPositionTooSmall()
        {

            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("IntegerList", -1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }

 

        [Fact]
        public void RemoveFromEndOfList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("IntegerList");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2 }, doc.IntegerList);

        }

 


        [Fact]
        public void NestedRemove()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    StringProperty = "A"
                }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Remove("SimpleDTO/StringProperty");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserialized.ApplyTo(doc);

            Assert.Equal(null, doc.SimpleDTO.StringProperty);

        }



        

        [Fact]
        public void NestedRemoveFromList()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
            };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("SimpleDTO/IntegerList", 2);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);
        }

 

        [Fact]
        public void NestedRemoveFromListInvalidPositionTooLarge()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("SimpleDTO/IntegerList", 3);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }


 
        [Fact]
        public void NestedRemoveFromListInvalidPositionTooSmall()
        {

            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
            }
              ;

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("SimpleDTO/IntegerList", -1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            
            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }
         

        [Fact]
        public void NestedRemoveFromEndOfList()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.RemoveFromArray("SimpleDTO/IntegerList");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);

        }


    }
}
