using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class JsonPatchPropertyTests
    {

        [Fact]
        public void HonourJsonPropertyOnSerialization()
        {          
            // create patch
            JsonPatchDocument<JsonPropertyDTO> patchDoc = new JsonPatchDocument<JsonPropertyDTO>();
            patchDoc.Add(p => p.Name, "Kevin");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            // serialized value should have "AnotherName" as path
            // deserialize to a non-generic version to check
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Equal(deserialized.Operations.First().path, "AnotherName");
        }

        [Fact]
        public void HonourJsonPropertyOnApply()
        {
            var doc = new JsonPropertyDTO()
            {
                Name = "InitialValue"
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("AnotherName", "Kevin");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal("Kevin", doc.Name);
        }
    }
}
