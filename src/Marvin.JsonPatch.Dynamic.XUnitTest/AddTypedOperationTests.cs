using Marvin.JsonPatch.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class AddTypedOperationTests
    {


        [Fact]
        public void AddToListNegativePosition()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("IntegerList/-1", 4);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
             
            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }


        [Fact]
        public void AddToListInList()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                ListOfSimpleDTO = new List<SimpleDTO>()
                {
                     new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
                }
            };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("ListOfSimpleDTO/0/IntegerList/0", 4);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 1, 2, 3 }, doc.ListOfSimpleDTO[0].IntegerList);
        }



        [Fact]
        public void AddToListInListInvalidPositionTooSmall()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                ListOfSimpleDTO = new List<SimpleDTO>()
                {
                     new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
                }
            };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("ListOfSimpleDTO/-1/IntegerList/0", 4);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });
        }


        [Fact]
        public void AddToListInListInvalidPositionTooLarge()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                ListOfSimpleDTO = new List<SimpleDTO>()
                {
                     new SimpleDTO()
                {
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
                }
            };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("ListOfSimpleDTO/20/IntegerList/0", 4);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });
        }

    }
}
