using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class PathHelpers
    {
        internal static CheckPathResult CheckPath(string pathToCheck)
        {
           string adjustedPath = pathToCheck;

           if (!(pathToCheck.StartsWith("/")))
           {
               adjustedPath = "/" + adjustedPath;
           }

           return new CheckPathResult(true, adjustedPath);
        }
    }
}
