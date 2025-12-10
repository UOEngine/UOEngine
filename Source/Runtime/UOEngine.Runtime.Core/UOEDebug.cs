using System.Diagnostics;

namespace UOEngine.Runtime.Core;

public class UOEDebug
{
    [DebuggerHidden]
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

    [DebuggerHidden]
    public static void Assert(bool condition, string? message = default)
    {
        if (condition)
        {
            return;
        }

        if(message != null)
        {
            Console.WriteLine($"Assert: {message}");
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
