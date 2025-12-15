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
            throw new AssertionFailedException($"Assert failed: {message}");
        }
    }

    [DebuggerHidden]
    public static void WaitForDebugger()
    {
        if(Debugger.IsAttached)
        {
            return;
        }

        Console.WriteLine("Waiting for debugger to attach.");

        while(Debugger.IsAttached == false)
        {
            Thread.Sleep(200);
        }

        // Break here so we break at the wait point.
        Debugger.Break();
    }

    public static void Trace(string message)
    {
        string output = $"{Application.FrameNumber}: {message}";

        Console.WriteLine(output);
        Debug.WriteLine(output);

    }

}

public class AssertionFailedException: Exception
{
    public AssertionFailedException(string message)
    : base(message) { }
}