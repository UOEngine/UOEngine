using UOEngine.Runtime.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        //using var app = new UO3DApplication();

        //app.Start();

        using (var app = new Application())
        {
            app.Start();
        }
    }
}
