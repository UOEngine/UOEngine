// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Rendering;

namespace UOEngine.Runtime.AvaloniaUI;

internal class ManualRenderTimer : IRenderTimer
{
    public bool RunsInBackground => false;

    public event Action<TimeSpan>? Tick;
}
