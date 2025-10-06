using ImGuiNET;

using UOEngine.Plugin;
using UOEngine.Runtime.Renderer;

namespace UOEngine.UI;

public class UIPlugin : IPlugin
{
    private readonly Renderer _renderer;

    public UIPlugin(Renderer renderer)
    {
        _renderer = renderer;

        _renderer.OnFrameBegin += () =>
        {
            ImGui.NewFrame();
        };

        _renderer.OnFrameEnd += () =>
        {
            ImGui.EndFrame();
        };
    }

    public void Startup()
    {
        var context = ImGui.CreateContext();

        ImGui.SetCurrentContext(context);
    }

    public void Update(float deltaSeconds)
    {

    }
}
