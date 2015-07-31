using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{ 
    internal class ActualPropertyPathResult
    { 
        public int NumericEnd { get; private set; }
        public string PathToProperty { get; set; }
        public bool ExecuteAtEnd { get; set; }

        public ActualPropertyPathResult( 
            int numericEnd, 
            string pathToProperty,
            bool executeAtEnd)
        { 
            NumericEnd = numericEnd; 
            PathToProperty = pathToProperty;
            ExecuteAtEnd = executeAtEnd;
        }
    }
}
