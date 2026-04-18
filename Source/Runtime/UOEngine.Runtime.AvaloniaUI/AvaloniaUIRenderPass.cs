// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderPass))]
internal class AvaloniaUIRenderPass : IRenderPass
{
    public string Name => "AvaloniaUI";

    public RenderPassStage Stage => RenderPassStage.OverlayBuild;

    public int Order => 0;

    RenderPassBuilder IRenderPass.Rpb => _rbp;

    private readonly AvaloniaControl _rootControl;
    private readonly IRenderer _rhiRenderer;

    private readonly RenderPassBuilder _rbp = new()
    {
        RequiresPreviousSubmit = true
    };

    public AvaloniaUIRenderPass(AvaloniaControl rootControl, IRenderer rhiRenderer)
    {
        _rootControl = rootControl;
        _rhiRenderer = rhiRenderer;
    }

    public void Execute(IRenderContext context, RenderSystem renderSystem)
    {
        MarkRenderPoint("Avalonia Start");

        _rootControl.Draw(context);

        MarkRenderPoint("Avalonia End");
    }

    private void MarkRenderPoint(string name)
    {
        var avaloniaStart = _rhiRenderer.CreateRenderContext(name);
        avaloniaStart.InsertMarker(name, Colour.Blue);
        avaloniaStart.Flush();
    }

}
