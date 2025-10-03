using ImGuiNET;

using UOEngine.Plugin;
using UOEngine.Renderer;

namespace UOEngine.UI;

public class UIPlugin : IPlugin
{
    //private readonly RendererPlugin _rendererPlugin;

    public UIPlugin()
    {
        //_rendererPlugin = rendererPlugin;

        //_rendererPlugin.OnFrameBegin += (sender, message) =>
        //{
        //    ImGui.NewFrame();
        //};

        //_rendererPlugin.OnFrameEnd += (sender, message) =>
        //{
        //    ImGui.EndFrame();
        //};
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
