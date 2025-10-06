using System.Reflection;
using HarmonyLib;
using UOEngine.Runtime;

namespace UOEngine.Editor;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        using (var app = new Application())
        {
            app.Start();
        }
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        Console.WriteLine("Failed to resolve assembly: " + args.Name);

        return null;
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