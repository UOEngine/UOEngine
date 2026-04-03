// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Rendering;

namespace UOEngine.Runtime.AvaloniaUI;

internal class ManualRenderTimer : IRenderTimer
{
    private volatile Action<TimeSpan>? _tick;

    public bool RunsInBackground => false;

    public Action<TimeSpan>? Tick
    {
        get => _tick;
        set
        {
            _tick = value;
        }
    }

    //public event Action<TimeSpan>? Tick;
}
