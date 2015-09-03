// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Newtonsoft.Json.Serialization;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    /// <summary>
    /// Metadata for JsonProperty.
    /// </summary>
    public class JsonPatchProperty
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public JsonPatchProperty(JsonProperty property, object parent)
        {
            Property = property;
            Parent = parent;
        }

        /// <summary>
        /// Gets or sets JsonProperty.
        /// </summary>
        public JsonProperty Property { get; set; }

        /// <summary>
        /// Gets or sets Parent.
        /// </summary>
        public object Parent { get; set; } 
    }
}
