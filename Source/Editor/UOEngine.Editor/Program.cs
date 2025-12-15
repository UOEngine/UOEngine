using UOEngine.Runtime.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        UOEDebug.WaitForDebugger();

        using (var app = new Application())
        {
            app.Start();
        }
    }
}
