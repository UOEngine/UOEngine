//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using HarmonyLib;

//[HarmonyPatch(typeof(GraphicsDeviceManager), "CreateDevice")]
//class PreventWindowPatch
//{
//    static bool Prefix(GraphicsDeviceManager __instance)
//    {
//        // Return false to skip the original method
//        // This prevents ClassicUO from creating its own GraphicsDevice & SDL window
//        return false;
//    }
//}