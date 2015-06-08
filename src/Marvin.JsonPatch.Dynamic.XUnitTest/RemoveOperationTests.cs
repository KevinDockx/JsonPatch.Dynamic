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
        public void RemovePropertyFromExpandoObject()
        {

            dynamic obj = new ExpandoObject();
            obj.Test = 1; 

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add<int>("NewInt", 1);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserialized.ApplyTo(obj);

            Assert.Equal(1, obj.NewInt);
            Assert.Equal(1, obj.Test);


        }



    }
}
