using System.Reflection;
using HarmonyLib;
using UOEngine.Runtime;

namespace UOEngine2;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        using (var app = new Application())
        {
            //app.RegisterPlugin<ClientPlugin>();
            //app.RegisterPlugin<ServerPlugin>();
            //app.RegisterPlugin<EditorPlugin>();
            //app.RegisterPlugin<UIPlugin>();

            app.Start();
        }
        //Task clientTask = Task.Run(() =>
        //{
        //    var asm = Assembly.LoadFrom("cuo.dll");
        //    var type = asm.GetType("ClassicUO.Bootstrap");

        //    var method = type.GetMethod("Main");

        //    var result = method.Invoke(null, new object[] { args });
        //});

        //Patcher.Patch(typeof(Server.Core), typeof(PatchServUO), nameof(PatchServUO.PatchExePath), "ExePath");

        ////Server.Core.Main(args);

        //Task serverTask = Task.Run(() => Server.Core.Main(args));

        //Task.WaitAll(clientTask, serverTask);
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        Console.WriteLine("Failed to resolve assembly: " + args.Name);
        // You can attempt to load it manually if you want:
        // return Assembly.LoadFrom("path_to_dll");
        return null;
    }

}

public static class PatchServUO
{
    public static bool PatchExePath(ref string __result)
    {
        __result = "D:\\UODev\\UOEngine2\\ThirdParty\\ServUO\\ServUO.exe";

        return false;
    }
}

public static class Patcher
{
    public static readonly Harmony harmony = new Harmony("UOEngine");

    public static void PatchInternal(string typeToPatch, Type patcherClass, string patcherMethod, string typeToPatchTarget)
    {
        Type internalType = AccessTools.TypeByName(typeToPatch);

        Patch(internalType, patcherClass, patcherMethod, typeToPatchTarget);
    }

    public static void Patch(Type typeToPatch, Type patcherClass, string PatcherMethod, string typeToPatchTarget)
    {
        var original = AccessTools.PropertyGetter(typeToPatch, typeToPatchTarget);
        var prefix = patcherClass.GetMethod(PatcherMethod);

        harmony.Patch(original, prefix: new HarmonyMethod(prefix));
    }
}