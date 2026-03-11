// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls.Embedding;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineTopLevel: EmbeddableControlRoot
{
    internal UOEngineTopLevelImpl Impl { get; }

    public UOEngineTopLevel(UOEngineTopLevelImpl topLevelImpl): base(topLevelImpl)
    {
        Impl = topLevelImpl;
    }

    public void OnDraw(IRenderContext renderContext, Rect rect)
    {
        var renderTarget = new RhiRenderTarget("AvaloniaUI RenderTarget");

        //renderTarget.Setup(Impl.Surface.Surface.);

        // Prep vkimage
        renderContext.BeginRenderPass(new RenderPassInfo
        {
            RenderTarget = renderTarget,
            Name = "AvaloniaUI Renderpass"
        });

        PlatformImpl!.Paint?.Invoke(rect);

        renderContext.EndRenderPass();

        // Transition vkimage
    }
}
