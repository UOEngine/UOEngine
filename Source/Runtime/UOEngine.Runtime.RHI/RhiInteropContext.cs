// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.RHI;

public struct RhiInteropContext
{
    public readonly nint Instance { get; init; }
    public readonly nint PhysicalDevice { get; init; }
    public readonly nint Device { get; init; }
    public readonly nint GraphicsQueue { get; init; }
    public readonly uint QueueFamilyIndex { get; init; }
    public readonly Func<string, nint, nint, nint> GetProcAddress {get; init;}
}
