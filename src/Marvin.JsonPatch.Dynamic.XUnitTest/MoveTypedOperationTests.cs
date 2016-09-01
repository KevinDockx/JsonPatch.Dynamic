// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class MoveTypedOperationTests
    {       
        [Fact]
        public void Move()
        {
            var doc = new SimpleDTO()
            {
                StringProperty = "A",
                AnotherStringProperty = "B"
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("StringProperty", "AnotherStringProperty");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal("A", doc.AnotherStringProperty);
            Assert.Equal(null, doc.StringProperty);
        }

        [Fact]
        public void MoveInList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("IntegerList/0","IntegerList/1");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 2, 1, 3 }, doc.IntegerList);
        }

        [Fact]
        public void MoveFromListToEndOfList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("IntegerList/0", "IntegerList/-");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 2, 3, 1 }, doc.IntegerList);
        }

        [Fact]
        public void MoveFomListToNonList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("IntegerList/0","IntegerValue");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 2, 3 }, doc.IntegerList);
            Assert.Equal(1, doc.IntegerValue);
        }

        [Fact]
        public void MoveFromNonListToList()
        {
            var doc = new SimpleDTO()
            {
                IntegerValue = 5,
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("IntegerValue", "IntegerList/0");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(0, doc.IntegerValue);
            Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.IntegerList);
        }

        [Fact]
        public void MoveToEndOfList()
        {
            var doc = new SimpleDTO()
            {
                IntegerValue = 5,
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Move("IntegerValue","IntegerList/-");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(0, doc.IntegerValue);
            Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.IntegerList);
        }

        [Fact]
        public void MoveInGenericList()
        {
            var doc = new SimpleDTO
            {
                IntegerGenericList = new List<int> { 1, 2, 3 }
            };

            // create patch
            var patchDoc = new JsonPatchDocument<SimpleDTO>();
            patchDoc.Move(o => o.IntegerGenericList, 0, o => o.IntegerGenericList, 1);

            patchDoc.ApplyTo(doc);

            Assert.Equal(new List<int> { 2, 1, 3 }, doc.IntegerGenericList);
        }
    }
}
