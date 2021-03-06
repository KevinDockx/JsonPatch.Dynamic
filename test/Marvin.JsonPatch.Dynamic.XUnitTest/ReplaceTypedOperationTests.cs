﻿// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class ReplaceTypedOperationTests
    {
        [Fact]
        public void  ReplaceGuidTest()
        {
            var doc = new SimpleDTO()
            {
                GuidValue = Guid.NewGuid()
            };

            var newGuid = Guid.NewGuid();
            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("GuidValue", newGuid);

            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserizalized.ApplyTo(doc);

            Assert.Equal(newGuid, doc.GuidValue);
        }

        [Fact]
        public void SerializeAndReplaceNestedObjectTest()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    IntegerValue = 5,
                    IntegerList = new List<int>() { 1, 2, 3 }
                }
            };

            var newDTO = new SimpleDTO()
            {
                DoubleValue = 1
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTO", newDTO);

            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(1, doc.SimpleDTO.DoubleValue);
            Assert.Equal(0, doc.SimpleDTO.IntegerValue);
            Assert.Equal(null, doc.SimpleDTO.IntegerList);
        }

        [Fact]
        public void ReplaceInList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/0", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 5, 2, 3 }, doc.IntegerList);
        }


        [Fact]
        public void ReplaceInGenericList()
        {
            var doc = new SimpleDTO
            {
                IntegerGenericList = new List<int> { 1, 2, 3 }
            };

            // create patch
            var patchDoc = new JsonPatchDocument<SimpleDTO>();
            patchDoc.Replace(o => o.IntegerGenericList, 5, 0);

            patchDoc.ApplyTo(doc);

            Assert.Equal(new List<int> { 5, 2, 3 }, doc.IntegerGenericList);
        }

        [Fact]
        public void ReplaceFullList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new List<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);
        }

        [Fact]
        public void ReplaceInListInList()
        {
            var doc = new SimpleDTO()
            {
                SimpleDTOList = new List<SimpleDTO>() { 
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }}
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/0/IntegerList/0",4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);
            
            Assert.Equal(4, doc.SimpleDTOList.First().IntegerList.First());
        }

        public void ReplaceInListInListInvalidPosition()
        {
            var doc = new SimpleDTO()
            {
                SimpleDTOList = new List<SimpleDTO>() { 
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }}
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/0/IntegerList/10", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
                
            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });
        }
 
        [Fact]
        public void ReplaceFullListFromEnumerable()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new List<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);
        } 

        [Fact]
        public void ReplaceFullListWithCollection()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new Collection<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);
        }
      
        [Fact]
        public void ReplaceAtEndOfList()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/-", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2, 5 }, doc.IntegerList);
        } 

        [Fact]
        public void ReplaceInListInvalidInvalidPositionTooLarge()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/3", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() =>
            {
                deserialized.ApplyTo(doc);
            });
        } 

        [Fact]
        public void ReplaceInListInvalidPositionTooSmall()
        {
            var doc = new SimpleDTO()
            {
                IntegerList = new List<int>() { 1, 2, 3 }
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/-1", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() =>
            {
                deserialized.ApplyTo(doc);
            });
        }

        [Fact]
        public void Replace()
        {
            var doc = new SimpleDTOWithNestedDTO()
            {
                SimpleDTO = new SimpleDTO()
                {
                    StringProperty = "A",
                    DecimalValue = 10
                }
            };

            // create patch
            JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc =
                new JsonPatchDocument<SimpleDTOWithNestedDTO>();
            patchDoc.Replace<string>(o => o.SimpleDTO.StringProperty, "B");
            patchDoc.Replace(o => o.SimpleDTO.DecimalValue, 12);

            patchDoc.ApplyTo(doc);

            Assert.Equal("B", doc.SimpleDTO.StringProperty);
            Assert.Equal(12, doc.SimpleDTO.DecimalValue);
        }
    }
}
