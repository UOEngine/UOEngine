// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Platform;

public interface IWindow
{
    public event Action<IWindow>? OnResized;

    public IntPtr Handle { get; }
    public uint Width { get; }
    public uint Height { get; }

    public uint RenderTargetWidth { get; }
    public uint RenderTargetHeight { get;}

    public string WindowTitle { get; set; }

}
