using System.Runtime.InteropServices;
using ImGuiNET;

using UOEngine.Plugin;
using UOEngine.Runtime;
using UOEngine.Runtime.Renderer;

namespace UOEngine.UI;

public class UIPlugin : IPlugin
{
    private readonly Renderer _renderer;
    private readonly RenderFactory _renderFactory;

    private readonly List<UOETexture> _textures = [];

    public UIPlugin(Renderer renderer, RenderFactory renderFactory, ApplicationLoop applicationLoop)
    {
        _renderer = renderer;
        _renderFactory = renderFactory;

        _renderer.OnFrameBegin += OnFrameBegin;

        _renderer.OnFrameEnd += OnFrameEnd;

        applicationLoop.OnUpdate += Update;
    }

    public void Startup()
    {
        var context = ImGui.CreateContext();

        ImGui.SetCurrentContext(context);

        RebuildFontAtlas();
    }

    private void Update(TimeSpan time)
    {
        var io = ImGui.GetIO();

        io.DisplaySize = new System.Numerics.Vector2(1, 1);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);
    }

    private void OnFrameBegin(RenderContext renderContext)
    {
        ImGui.NewFrame();
    }

    private void OnFrameEnd(RenderContext renderContext)
    {
        ImGui.EndFrame();
        ImGui.Render();
    }

    public unsafe void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixelData, out int width, out int height, out int bytesPerPixel);

        var tex2d = _renderFactory.CreateTexture(width, height);

        tex2d.SetDataPointer(pixelData, width * height * bytesPerPixel);

        _textures.Add(tex2d);

        // Let ImGui know where to find the texture
        io.Fonts.SetTexID(0);
        io.Fonts.ClearTexData(); // Clears CPU side texture data
    }
}
