// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.SDL3;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;
using UOEngine.Runtime.RHI.Resources;

namespace UOEngine.Runtime.UI;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Runtime)]
public class UIPlugin : IPlugin
{
    private readonly RenderSystem _rendererSystem;
    private readonly IRenderResourceFactory _renderFactory;

    private readonly List<IRenderTexture> _textures = [];

    public UIPlugin(RenderSystem renderer, IRenderResourceFactory renderFactory, ApplicationLoop applicationLoop)
    {
        _rendererSystem = renderer;
        _renderFactory = renderFactory;

        //_rendererSystem.OnFrameBegin += OnFrameBegin;

        //_rendererSystem.OnFrameEnd += OnFrameEnd;

        //applicationLoop.OnUpdate += Update;
    }

    public void Startup()
    {
        //var context = ImGui.CreateContext();

        //ImGui.SetCurrentContext(context);
        
        //ImGuiImplSDL3.SetCurrentContext(context);

        //RebuildFontAtlas();
    }

    private void Update(float time)
    {
        var io = ImGui.GetIO();

        uint width = _rendererSystem.UIOverlay.Width;
        uint height = _rendererSystem.UIOverlay.Height;

        io.DisplaySize = new System.Numerics.Vector2(width, height);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(width, height);
    }

    private void OnFrameBegin(IRenderContext renderContext)
    {
        //ImGui.NewFrame();
    }

    private void OnFrameEnd(IRenderContext renderContext)
    {
        ImGui.EndFrame();
        ImGui.Render();

        ImDrawDataPtr drawData = ImGui.GetDrawData();

        renderContext.BeginRenderPass(new RenderPassInfo
        {
            Name = "UI",
            RenderTarget = _rendererSystem.UIOverlay
        });

        for(int i = 0; i < drawData.CmdListsCount; i++)
        {
            // Build vertex buffers
        }

        renderContext.EndRenderPass();
    }

    public unsafe void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();

        var texData = io.Fonts.TexData;

        //var tex2d = _renderFactory.CreateTexture((uint)texData.Width, (uint)texData.Height);

        //tex2d.SetDataPointer((UIntPtr)pixelData, width * height * bytesPerPixel);

        //_textures.Add(tex2d);

        // Let ImGui know where to find the texture
        io.Fonts.ClearTexData(); // Clears CPU side texture data
    }
}
