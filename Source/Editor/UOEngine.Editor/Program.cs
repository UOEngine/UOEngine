using UOEngine.Runtime.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        if(CommandLine.HasOption("-wait_for_debugger"))
        {
            UOEDebug.WaitForDebugger();
        }

        using (var app = new Application())
        {
            app.Start();
        }
    }
}
