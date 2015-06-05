using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal class CreateTreeResult
    {
        public bool CreatingIsAllowed { get; private set; }
        public IDictionary<String, Object> CreatedTree { get; private set; }


        public CreateTreeResult(bool creatingIsAllowed, IDictionary<String, Object> createdTree)
        {
            CreatingIsAllowed = creatingIsAllowed;
            CreatedTree = createdTree;

        }
    }
}
