using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    public class ExpandoObjectContainerResult
    {    
        public bool IsValidContainer { get; private set; }
        public IDictionary<String, Object> Container { get; private set; }
        
        public string PathToPropertyInContainer {get; private set;}


        public ExpandoObjectContainerResult(bool isValidContainer, IDictionary<String, Object> container, 
            string pathToPropertyInContainer)
        {

            IsValidContainer = isValidContainer;
            Container = container;
            PathToPropertyInContainer = pathToPropertyInContainer;

        }
    }
}
