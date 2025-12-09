using System.Diagnostics;

namespace UOEngine.Runtime.Core;

public class UOEDebug
{
    public static void NotImplemented()
    {
        if(Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public static void Assert(bool condition)
    {
        if (condition)
        {
            return;
        }

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            throw new InvalidOperationException("Assert failed.");
        }
    }
}
