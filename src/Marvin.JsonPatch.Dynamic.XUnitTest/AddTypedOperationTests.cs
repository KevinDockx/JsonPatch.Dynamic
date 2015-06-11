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
            patchDoc.AddToArray("IntegerList", 4, -1);


            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
             
            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });

        }

    }
}
