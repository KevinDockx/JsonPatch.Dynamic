using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal class CheckPathResult
    {
        public bool IsCorrectlyFormedPath { get; set; }
        public string AdjustedPath { get; set; }

        public CheckPathResult(bool isCorrectlyFormedPath, string adjustedPath)
        {
            IsCorrectlyFormedPath = isCorrectlyFormedPath;
            AdjustedPath = adjustedPath;
        }
    }
}
