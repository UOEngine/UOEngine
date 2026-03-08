using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.Runtime)]
public class VulkanPlugin: IPlugin
{
    private readonly VulkanRenderer _renderer;

    public VulkanPlugin(IRenderer renderer)
    {
        _renderer = (VulkanRenderer)renderer;
    }

    public void Startup()
    {
        _renderer.Startup();
    }
}
