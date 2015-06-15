// Any comments, input: @KevinDockx
// Any issues, requests: https://github.com/KevinDockx/JsonPatch.Dynamic
//
// Enjoy :-)


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
