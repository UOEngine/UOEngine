// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Xml.Linq;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public enum DefaultTextureType
{
    Red,
    Green, 
    Blue,
    RedCheckerboard,

    Count
}

[Service(UOEServiceLifetime.Singleton)]
public class RenderSystem
{
    public event Action<IRenderContext>? OnFrameBegin;
    public event Action<IRenderContext>? OnFrameEnd;

    public RhiRenderTarget GBufferDiffuse => _gBufferDiffuse ?? throw new InvalidOperationException("GBufferDiffuse not set");
    public RhiRenderTarget UIOverlay = new();

    public IRenderContext CurrentRenderContext => _context ?? throw new InvalidOperationException("Not initialized");

    public IRenderTexture GetDefaultTexture(DefaultTextureType type) => _defaultTextures[(int)type];

    private IRenderContext? _context;
    private readonly IRenderer _rhiRenderer;
    private readonly IRenderResourceFactory _resourceFactory;

    private RenderPassInfo _mainPass;

    private RhiRenderTarget? _gBufferDiffuse;

    private uint _frameNumber = 0;

    private IRenderTexture[] _defaultTextures = new IRenderTexture[(int)DefaultTextureType.Count];

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

        uint defaultTextureSize = 128;

        CreateDefaultTexture(DefaultTextureType.Red, defaultTextureSize, defaultTextureSize, Colour.Red, FillWithSolidColour, "DefaultRedTexture");
        CreateDefaultTexture(DefaultTextureType.Green, defaultTextureSize, defaultTextureSize, Colour.Green, FillWithSolidColour, "DefaultRedTexture");
        CreateDefaultTexture(DefaultTextureType.Blue, defaultTextureSize, defaultTextureSize, Colour.Blue, FillWithSolidColour, "DefaultRedTexture");
        CreateDefaultTexture(DefaultTextureType.RedCheckerboard, defaultTextureSize, defaultTextureSize, Colour.Red, FillWithCheckerboardEffect, "DefaultRedCheckerboardTexture");
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

    private void CreateDefaultTexture(DefaultTextureType type, uint width, uint height, in Colour colour, Action<uint, uint, Colour, Span<Colour>> pixelFillFunction, string? name = null)
    {
        var texture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = width,
            Height = height,
            Name = name,
            Usage = RhiRenderTextureUsage.Sampler
        });

        Span<Colour> texels = texture.GetTexelsAs<Colour>();

        pixelFillFunction(width, height, colour, texels);

        texture.Upload();

        _defaultTextures[(int)type] = texture;
    }

    private void FillWithSolidColour(uint width, uint height, Colour colour, Span<Colour> texels) => texels.Fill(colour);

    //private void CreateDefaultTexture(DefaultTextureType type, uint  width, uint height, in Colour colour, string? name = null)
    //{
    //    var texture = _resourceFactory.CreateTexture(new RhiTextureDescription
    //    {
    //        Width = width,
    //        Height = height,
    //        Name = name,
    //        Usage = RhiRenderTextureUsage.Sampler
    //    });

    //    Span<Colour> texels = texture.GetTexelsAs<Colour>();

    //    texels.Fill(colour);

    //    texture.Upload();

    //    _defaultTextures[(int)type] = texture;
    //}

    private void FillWithCheckerboardEffect(uint width, uint height, Colour colour, Span<Colour> texels)
    {
        int checkSize = 16;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                bool isWhite = ((x / checkSize) + (y / checkSize)) % 2 == 0;

                int i = y * (int)width + x;

                if (isWhite)
                {
                    texels[i] = Colour.White;
                }
                else
                {
                    texels[i] = colour;
                }
            }
        }
    }
}
