// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.FnaAdapter;

// What hosts our game. A window, a surface of a UI element etc. IGameWindowHost? Window behaviour focused.
public interface IHostedGameHost
{
    public IntPtr NativeWindowHandle { get; }
    public Rectangle ClientBounds { get; }

    public event Action<Rectangle>? BoundsChanged;
}
