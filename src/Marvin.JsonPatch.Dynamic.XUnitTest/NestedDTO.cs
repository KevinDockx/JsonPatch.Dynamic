using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.XUnitTest
{
    public class NestedDTO
    {
        public string StringProperty { get; set; }

        public dynamic DynamicProperty { get; set; }
    }
}
