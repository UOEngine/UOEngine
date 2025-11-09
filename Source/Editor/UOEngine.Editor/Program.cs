using System.Runtime.InteropServices;
using UO3D;

public static class Program
{
    [DllImport("kernel32", EntryPoint = "LoadLibrary")]
    private static extern IntPtr LoadLibrary(string fileName);

    public static void Main(string[] args)
    {
        using var app = new UO3DApplication();

        app.Start();
    }
}
