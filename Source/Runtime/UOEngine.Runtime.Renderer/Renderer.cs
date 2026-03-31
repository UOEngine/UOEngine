// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

[Service(UOEServiceLifetime.Singleton)]
public class RenderSystem
{
    public event Action<IRenderContext>? OnFrameBegin;
    public event Action<IRenderContext>? OnFrameEnd;

    //public RhiRenderTarget GBufferDiffuse => _gBufferDiffuse ?? throw new InvalidOperationException("GBufferDiffuse not set");
    public RhiRenderTarget UIOverlay = new();

    public IRenderContext CurrentRenderContext => _context ?? throw new InvalidOperationException("Not initialized");

    public readonly GlobalRenderResources GlobalRenderResources;
    public readonly FullscreenPassUtils FullscreenPassUtils;

    private IRenderContext? _context;
    private readonly IRenderer _rhiRenderer;
    private readonly IRenderResourceFactory _resourceFactory;

    private RenderPassInfo _mainPass;

    //private RhiRenderTarget? _gBufferDiffuse;

    private uint _frameNumber = 0;

    public RenderSystem(IRenderer rhiRenderer, IRenderResourceFactory resourceFactory)
    {
        _rhiRenderer = rhiRenderer;
        _resourceFactory = resourceFactory;
        GlobalRenderResources = new GlobalRenderResources(resourceFactory);
        FullscreenPassUtils = new FullscreenPassUtils(GlobalRenderResources);
    }

    public void Startup()
    {
        GlobalRenderResources.Init();

        UIOverlay = new RhiRenderTarget();

        var uiTexture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = 1920,
            Height = 1080,
            Usage = RhiRenderTextureUsage.ColourTarget,
            Name = "UIOverlay",
        });

        UIOverlay.Setup(uiTexture);
    }

    public void FrameBegin()
    {
        _rhiRenderer.FrameBegin();

        var overlaySetupContext = _rhiRenderer.CreateRenderContext("OverlaySetupContext");

        overlaySetupContext.BeginRenderPass(new RenderPassInfo
        {
            Name = "Clear UI overlay",
            LoadAction = RhiRenderTargetLoadAction.Clear,
            RenderTarget = UIOverlay,
            ClearColour = new(0, 0, 0, 0)
        });

        overlaySetupContext.EndRenderPass();
        overlaySetupContext.Flush();

        _context = _rhiRenderer.CreateRenderContext("MainGraphicsContext");

        _context.BeginRenderPass(new RenderPassInfo
        {
            Name = "Clear backbuffer",
            LoadAction = RhiRenderTargetLoadAction.Clear,
            RenderTarget = null
        });

        _context.EndRenderPass();

        OnFrameBegin?.Invoke(_context);
    }

    public void FrameEnd()
    {
        OnFrameEnd?.Invoke(CurrentRenderContext);

        FullscreenPassUtils.BlitTexture(CurrentRenderContext, UIOverlay.Texture, null);

        _rhiRenderer.FrameEnd();

        _frameNumber++;
    }

    public void PrintStats()
    {
        UOEDebug.NotImplemented();
    }

}
