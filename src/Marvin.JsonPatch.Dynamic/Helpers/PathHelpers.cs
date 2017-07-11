// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Exceptions;
using Marvin.JsonPatch.Helpers;
using System;

namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class PathHelpers
    {
        internal static string NormalizePath(string path, CaseTransformType caseTransformType)
        {
            // check for most common path errors on create.  This is not
            // absolutely necessary, but it allows us to already catch mistakes
            // on creation of the patch document rather than on execute.

            if (path.Contains(".") || path.Contains("//") || path.Contains(" ") || path.Contains("\\"))
            {
                throw new JsonPatchException(string.Format("Provided string is not a valid path: {0}", path), null);
            }

            if (!(path.StartsWith("/")))
            {
                path = "/" + path;
            }

            switch (caseTransformType)
            {
                case CaseTransformType.LowerCase:
                    return path.ToLowerInvariant();
                case CaseTransformType.UpperCase:
                    return path.ToUpperInvariant();
                case CaseTransformType.CamelCase:
                    if (path.Length > 1)
                    {
                        return "/" + Char.ToLowerInvariant(path[1]) + path.Substring(2);
                    }
                    else
                    {
                        return path;
                    }
                case CaseTransformType.OriginalCase:
                    return path;
                default:
                    throw new NotImplementedException();                    
            }
        }
    }
}
