// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOEngine.Runtime.Vulkan;

internal static unsafe partial class UOEngineSdl3
{
    [LibraryImport("SDL3")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial byte** SDL_Vulkan_GetInstanceExtensions(uint* count);
}
