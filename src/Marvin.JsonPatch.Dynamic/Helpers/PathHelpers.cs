// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)

using Marvin.JsonPatch.Exceptions;
namespace Marvin.JsonPatch.Dynamic.Helpers
{
    internal static class PathHelpers
    {
        internal static string NormalizePath(string path)
        {
            // check for most common path errors on create.  This is not
            // absolutely necessary, but it allows us to already catch mistakes
            // on creation of the patch document rather than on execute.

            if (path.Contains(".") || path.Contains("//") || path.Contains(" ") || path.Contains("\\"))
            {
                throw new JsonPatchException(string.Format("Provided string is not a valid path: {0}", path), null, -1);
            }

            if (!(path.StartsWith("/")))
            {
                return "/" + path;
            }
            else
            {
                return path;
            }
        }
    }
}
