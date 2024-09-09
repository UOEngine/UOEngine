using System.Diagnostics;

namespace UOEngine.Runtime.Core
{
    public static class UOE
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message = "")
        {
            if(condition == false)
            {
                throw new Exception(message);
            }
        }
    }
}
