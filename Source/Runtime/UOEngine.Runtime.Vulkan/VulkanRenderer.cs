// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace UOEngine.Runtime.Vulkan;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderer))]
public class VulkanRenderer : IRenderer
{
    public IRenderSwapChain SwapChain => throw new NotImplementedException();

    private readonly IWindow _window;

    private VulkanDevice _device = null!;

    private readonly VulkanInstance _instance = new();

    private bool _enableDebug = false;

    public VulkanRenderer(IWindow window)
    {
        _window = window;

        if(CommandLine.HasOption("-debugdevice"))
        {
            _enableDebug = true;
        }
    }

    public void Startup()
    {
        VkResult result = vkInitialize();

        result.CheckResult();

        _instance.Create("UOEngineApp", _enableDebug);

        VkSurfaceKHR surface = _instance.CreateSurface(_window.Handle);

        _device = new VulkanDevice(_instance.GetSuitableDevice());

        _device.InitGpu(_instance.Api);

    }

    public void Shutdown()
    {
        _instance.Destroy();
    }

    public IRenderContext CreateRenderContext()
    {
        throw new NotImplementedException();
    }
}
