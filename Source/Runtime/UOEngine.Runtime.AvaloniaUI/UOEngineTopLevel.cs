// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls.Embedding;
using Avalonia.Rendering;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineTopLevel: EmbeddableControlRoot
{
    internal UOEngineTopLevelImpl Impl { get; }

    private IRenderTimer? RenderTimer => AvaloniaLocator.Current.GetService<IRenderTimer>();

    public UOEngineTopLevel(UOEngineTopLevelImpl topLevelImpl): base(topLevelImpl)
    {
        Impl = topLevelImpl;
    }

    public void OnDraw(IRenderContext renderContext, Rect rect)
    {
        //RenderTimer?.Tick?.Invoke(new TimeSpan(0));

        PlatformImpl!.Paint?.Invoke(rect);
    }
}
