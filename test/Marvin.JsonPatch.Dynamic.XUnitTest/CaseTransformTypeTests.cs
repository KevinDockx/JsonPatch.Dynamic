using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Helpers;
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
    public class CaseTransformTypeTests
    {
        [Fact]
        public void CaseTransformType_UpperCase_MustSerializeCorrectly()
        {
            dynamic doc = new ExpandoObject();
            doc.StringProperty = "A";

            JsonPatchDocument patchDoc = new JsonPatchDocument(CaseTransformType.UpperCase);
            patchDoc.Add("StringProperty", "B");
            
            var result = JsonConvert.SerializeObject(patchDoc);

            Assert.Equal("[{\"value\":\"B\",\"path\":\"/STRINGPROPERTY\",\"op\":\"add\"}]", result);
        }

        [Fact]
        public void CaseTransformType_CamelCase_MustSerializeCorrectly()
        {
            dynamic doc = new ExpandoObject();
            doc.StringProperty = "A";

            JsonPatchDocument patchDoc = new JsonPatchDocument(CaseTransformType.CamelCase);
            patchDoc.Add("StringProperty", "B");

            var result = JsonConvert.SerializeObject(patchDoc);

            Assert.Equal("[{\"value\":\"B\",\"path\":\"/stringProperty\",\"op\":\"add\"}]", result);
        }

        [Fact]
        public void CaseTransformType_OriginalCase_IsDefaultAndShouldSerializeCorrectly()
        {
            dynamic doc = new ExpandoObject();
            doc.StringProperty = "A";

            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Add("StringProperty", "B");

            var result = JsonConvert.SerializeObject(patchDoc);

            Assert.Equal("[{\"value\":\"B\",\"path\":\"/StringProperty\",\"op\":\"add\"}]", result);
        }

        [Fact]
        public void CaseTransformType_LowerCase_ShouldSerializeCorrectly()
        {
            dynamic doc = new ExpandoObject();
            doc.StringProperty = "A";

            JsonPatchDocument patchDoc = new JsonPatchDocument(CaseTransformType.LowerCase);
            patchDoc.Add("StringProperty", "B");

            var result = JsonConvert.SerializeObject(patchDoc);

            Assert.Equal("[{\"value\":\"B\",\"path\":\"/stringproperty\",\"op\":\"add\"}]", result);
        }

        [Fact]
        public void CaseTransformType_EmptyPath_ShouldCamelCaseWithoutError()
        {
            dynamic doc = new ExpandoObject();
            doc.StringProperty = "A";

            JsonPatchDocument patchDoc = new JsonPatchDocument(CaseTransformType.CamelCase);
            patchDoc.Add("", "B");

            var result = JsonConvert.SerializeObject(patchDoc);

            Assert.Equal("[{\"value\":\"B\",\"path\":\"/\",\"op\":\"add\"}]", result);            
        }

        [Fact]
        public void CaseTransformType_MustIgnoreCaseOnApply()
        {
            dynamic doc = new ExpandoObject();

            doc.StringProperty = "A";
            doc.AnotherStringProperty = "B";

            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Copy("STRINGProperty", "anotherstringproperty");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            deserialized.ApplyTo(doc);

            Assert.Equal("A", doc.AnotherStringProperty);
        }


        [Fact]
        public void CaseTransformType_UpperCase_MustIgnoreCaseOnApply()
        {
            dynamic doc = new ExpandoObject();

            doc.StringProperty = "A";
            doc.AnotherStringProperty = "B";

            JsonPatchDocument patchDoc = new JsonPatchDocument(CaseTransformType.UpperCase);
            patchDoc.Copy("STRINGProperty", "anotherstringproperty");

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            deserialized.ApplyTo(doc);

            Assert.Equal("A", doc.AnotherStringProperty);
        }
    }
}
