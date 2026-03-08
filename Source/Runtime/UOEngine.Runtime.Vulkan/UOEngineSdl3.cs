// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

//[StructLayout(LayoutKind.Sequential, Size = 1)]
//public struct VkInstance_T
//{
//    public ulong Handle;
//}

//[StructLayout(LayoutKind.Sequential, Size = 1)]
//public struct VkSurfaceKHR_T
//{
//    public ulong Handle;
//}

internal static unsafe partial class UOEngineSdl3
{
    [LibraryImport("SDL3")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial byte** SDL_Vulkan_GetInstanceExtensions(uint* count);

    // Window is SDL window handle here.
    [LibraryImport("SDL3")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial SDLBool SDL_Vulkan_CreateSurface(IntPtr window, IntPtr instance, VkAllocationCallbacks* allocator, VkSurfaceKHR** surface);

    public readonly record struct SDLBool
    {
        private readonly byte value;

        internal const byte FALSE_VALUE = 0;
        internal const byte TRUE_VALUE = 1;

        internal SDLBool(byte value)
        {
            this.value = value;
        }

        public static implicit operator bool(SDLBool b)
        {
            return b.value != FALSE_VALUE;
        }

        public static implicit operator SDLBool(bool b)
        {
            return new SDLBool(b ? TRUE_VALUE : FALSE_VALUE);
        }

        public bool Equals(SDLBool other)
        {
            return other.value == value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
