using System.Diagnostics;

namespace UOEngine.Runtime.Core
{
    public static class UOEngine
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            throw new Exception(message);
        }
    }
}
