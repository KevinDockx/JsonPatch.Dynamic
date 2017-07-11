// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Dynamic.Adapters;
using Marvin.JsonPatch.Dynamic.Converters;
using Marvin.JsonPatch.Dynamic.Helpers;
using Marvin.JsonPatch.Dynamic.Operations;
using Marvin.JsonPatch.Helpers;
using Marvin.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Marvin.JsonPatch.Dynamic
{    
    [JsonConverter(typeof(JsonPatchDocumentConverter))]
    public class JsonPatchDocument : IJsonPatchDocument
    {
        public List<Operation> Operations { get; private set; }

        [JsonIgnore]
        public IContractResolver ContractResolver { get; set; }

        [JsonIgnore]
        public CaseTransformType CaseTransformType { get; set; }

      
        /// <summary>
        /// Create a new JsonPatchDocument
        /// </summary>
        public JsonPatchDocument() :
            this(new List<Operation>(), new DefaultContractResolver(), CaseTransformType.OriginalCase)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument, and pass in a custom contract resolver
        /// to use when applying the document.
        /// </summary>
        /// <param name="contractResolver">A custom IContractResolver</param>
        public JsonPatchDocument(IContractResolver contractResolver)
            : this(new List<Operation>(), contractResolver, CaseTransformType.OriginalCase)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument from a list of operations
        /// </summary>
        /// <param name="operations">A list of operations</param>
        public JsonPatchDocument(List<Operation> operations)
            : this(operations, new DefaultContractResolver(), CaseTransformType.OriginalCase)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument and pass in a CaseTransformType
        /// </summary>
        /// <param name="caseTransformType">Defines the case used when seralizing the object to JSON</param>
        public JsonPatchDocument(CaseTransformType caseTransformType)
            : this(new List<Operation>(), new DefaultContractResolver(), caseTransformType)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument from a list of operations and pass in a CaseTransformType
        /// </summary>
        /// <param name="operations">A list of operations</param>
        /// <param name="caseTransformType">Defines the case used when seralizing the object to JSON</param>
        public JsonPatchDocument(List<Operation> operations, CaseTransformType caseTransformType)
            : this(operations, new DefaultContractResolver(), caseTransformType)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument, and pass in a CaseTransformType and
        /// custom contract resolver to use when applying the document.
        /// </summary>
        /// <param name="contractResolver">A custom IContractResolver</param>
        /// <param name="caseTransformType">Defines the case used when seralizing the object to JSON</param>
        public JsonPatchDocument(IContractResolver contractResolver, CaseTransformType caseTransformType)
            : this(new List<Operation>(), contractResolver, caseTransformType)
        {
        }

        /// <summary>
        /// Create a new JsonPatchDocument from a list of operations, and pass in a custom contract resolver 
        /// to use when applying the document.
        /// </summary>
        /// <param name="operations">A list of operations</param>
        /// <param name="contractResolver">A custom IContractResolver</param>
        public JsonPatchDocument(List<Operation> operations, IContractResolver contractResolver)
            : this(operations, contractResolver, CaseTransformType.OriginalCase)
        {
        }

        public JsonPatchDocument(List<Operation> operations, IContractResolver contractResolver, CaseTransformType caseTransformType)
        {
            Operations = operations;
            ContractResolver = contractResolver;
            CaseTransformType = caseTransformType;
        }

        public JsonPatchDocument Add(string path, object value)
        {
            Operations.Add(new Operation("add", PathHelpers.NormalizePath(path, CaseTransformType), null, value));
            return this;
        }
          
        public JsonPatchDocument Remove(string path)
        {
            Operations.Add(new Operation("remove", PathHelpers.NormalizePath(path, CaseTransformType), null, null));
            return this;
        }

        public JsonPatchDocument Replace(string path, object value)
        {
            Operations.Add(new Operation("replace", PathHelpers.NormalizePath(path, CaseTransformType), null, value));
            return this;
        }
   
        public JsonPatchDocument Move(string from, string path)
        {
            Operations.Add(new Operation("move", PathHelpers.NormalizePath(path, CaseTransformType), PathHelpers.NormalizePath(from, CaseTransformType)));
            return this;
        }
                 
        public JsonPatchDocument Copy(string from, string path)
        {
            Operations.Add(new Operation("copy", PathHelpers.NormalizePath(path, CaseTransformType), PathHelpers.NormalizePath(from, CaseTransformType)));
            return this;
        } 

        public void ApplyTo<T>(T objectToApplyTo)
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver));
        } 

        /// <summary>
        /// Apply the patch document, passing in a custom IObjectAdapter<typeparamref name=">"/>. 
        /// This method will change the passed-in object.
        /// </summary>
        /// <param name="objectToApplyTo">The object to apply the JsonPatchDocument to</param>
        /// <param name="adapter">The IObjectAdapter instance to use</param>
        public void ApplyTo<T>(T objectToApplyTo, IObjectAdapter adapter)
        {
            // apply each operation in order
            foreach (var op in Operations)
            {
                op.Apply(objectToApplyTo, adapter);
            }
        } 

        // return a copy - original operations should not
        // be editable through this.
        IList<Operation> IJsonPatchDocument.GetOperations()
        {
            var allOps = new List<Operation>();

            if (Operations != null)
            {
                foreach (var op in Operations)
                {
                    var untypedOp = new Operation();

                    untypedOp.op = op.op;
                    untypedOp.value = op.value;
                    untypedOp.path = op.path;
                    untypedOp.from = op.from;

                    allOps.Add(untypedOp);
                }
            }

            return allOps;
        } 
    }
}
