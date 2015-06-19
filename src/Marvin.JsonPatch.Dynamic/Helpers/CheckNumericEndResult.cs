using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal class CheckNumericEndResult
    {
        public bool HasNumericEnd { get; private set; }
        public int NumericEnd { get; private set; }

        public CheckNumericEndResult(bool hasNumericEnd, int? numericEnd)
        {
            HasNumericEnd = hasNumericEnd;
            if (hasNumericEnd)
            {
                NumericEnd = (int)numericEnd;
            }
        }

    }
}
