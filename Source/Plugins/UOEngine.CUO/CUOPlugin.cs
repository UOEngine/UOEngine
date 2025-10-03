using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using UOEngine.Plugin;

namespace UOEngine.CUO;

public class CUOPlugin : IPlugin
{
    private readonly string? _cuoDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private readonly IsolatedLoadContext cuoFNAContext;

    public CUOPlugin()
    {
        cuoFNAContext = new IsolatedLoadContext(_cuoDirectory!);
    }

    public async void Startup()
    {
        LoadNativeLibrary("SDL2.dll");

        //Assembly cuoFNA = cuoFNAContext.LoadFromAssemblyPath(Path.Combine(_cuoDirectory, "FNA.dll"));

        await StartClientAsync([]);
    }

    public async Task StartClientAsync(string[] args)
    {
        Task clientTask = Task.Run(() =>
        {
            var cuo = Path.Combine(_cuoDirectory, "cuo.dll");

            //var asm = cuoFNAContext.LoadFromAssemblyPath(cuo);
            var asm = Assembly.LoadFrom(cuo);
            var type = asm.GetType("ClassicUO.Bootstrap");

            var method = type.GetMethod("Main");

            var client = asm.GetType("ClassicUO.Client");

            var gameControllerType = client.GetMethod("GameController");

            var result = method.Invoke(null, [args]);
        });

        await clientTask;
    }

    public void Update(float deltaSeconds)
    {

    }

    private void LoadNativeLibrary(string dllName)
    {
        IntPtr handle = NativeLibrary.Load(Path.Combine(_cuoDirectory, dllName));

        Debug.Assert(handle != IntPtr.Zero);
    }
}

public class IsolatedLoadContext : AssemblyLoadContext
{
    private readonly string _directory;

    public IsolatedLoadContext(string directory, bool isCollectible = false)
        : base(isCollectible)
    {
        _directory = directory;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Try to load dependency from the custom folder
        string path = Path.Combine(_directory, assemblyName.Name + ".dll");
        if (File.Exists(path))
        {
            return LoadFromAssemblyPath(path);
        }

        // Fallback to default context (system assemblies)
        return null;
    }
}