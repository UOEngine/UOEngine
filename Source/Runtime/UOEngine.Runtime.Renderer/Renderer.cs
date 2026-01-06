// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

[Service(UOEServiceLifetime.Singleton)]
public class RenderSystem
{
    public event Action<IRenderContext>? OnFrameBegin;
    public event Action<IRenderContext>? OnFrameEnd;

    public RhiRenderTarget GBufferDiffuse => _gBufferDiffuse ?? throw new InvalidOperationException("GBufferDiffuse not set");
    public RhiRenderTarget UIOverlay = new();

    public IRenderContext CurrentRenderContext => _context ?? throw new InvalidOperationException("Not initialized");

    private IRenderContext? _context;
    private readonly IRenderer _rhiRenderer;
    private readonly IRenderResourceFactory _resourceFactory;

    private RenderPassInfo _mainPass;

    private RhiRenderTarget? _gBufferDiffuse;

    private uint _frameNumber = 0;

    public RenderSystem(IRenderer rhiRenderer, IRenderResourceFactory resourceFactory)
    {
        _rhiRenderer = rhiRenderer;
        _resourceFactory = resourceFactory;
    }

    public void Startup()
    {
        _context = _rhiRenderer.CreateRenderContext();

        UIOverlay = new RhiRenderTarget();

        var uiTexture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = 1920,
            Height = 1080,
            Usage = RhiRenderTextureUsage.ColourTarget
        });

        UIOverlay.Setup(uiTexture);
    }

    public void FrameBegin()
    {
        _rhiRenderer.FrameBegin();

        //CurrentRenderContext.BeginRecording();

        _gBufferDiffuse = _rhiRenderer.GetViewportRenderTarget();

        _mainPass = new RenderPassInfo
        {
            RenderTarget = GBufferDiffuse,
            Name = "MainPass"
        };

        CurrentRenderContext.BeginRenderPass(_mainPass);

        OnFrameBegin?.Invoke(CurrentRenderContext);
    }

    public void FrameEnd()
    {
        OnFrameEnd?.Invoke(CurrentRenderContext);

        CurrentRenderContext.EndRenderPass();
        //CurrentRenderContext.EndRecording();

        _rhiRenderer.FrameEnd();

        _frameNumber++;
    }
}
