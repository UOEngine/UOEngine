//using System.Reflection;

//using HarmonyLib;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using UOEngine.Plugin;

//namespace UOEngine.Client;

//public class ClientPlugin : IPlugin
//{
//    public async void Startup()
//    {
//        var harmony = new Harmony("UOEngine");

//        harmony.PatchAll();

//        await StartClientAsync([]);
//    }

//    public async Task StartClientAsync(string[] args)
//    {
//        Task clientTask = Task.Run(() =>
//        {
//            var asm = Assembly.LoadFrom("cuo.dll");
//            var type = asm.GetType("ClassicUO.Bootstrap");

//            var method = type.GetMethod("Main");

//            var result = method.Invoke(null, [args]);
//        });

//        await clientTask;
//    }
//}



//public static class DeviceInjector
//{
//    public static GraphicsDevice MyGraphicsDevice;
//}
