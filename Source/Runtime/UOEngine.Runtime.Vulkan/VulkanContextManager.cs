// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Vulkan;

internal class VulkanContextManager
{
    private List<VulkanGraphicsContext> _graphicsContexts = [];
    private List<VulkanGraphicsContext> _freeGraphicsContexts = [];
    internal  List<VulkanGraphicsContext> _inUseGraphicsContexts = [];

    private readonly VulkanDevice _device;
    private VulkanTexture? _currentBackbuffer;

    private readonly VulkanGlobalSamplers _globalSamplers;

    private readonly VulkanRenderer _renderer;
    internal VulkanContextManager(VulkanRenderer renderer, VulkanDevice device, VulkanGlobalSamplers globalSamplers)
    {
        _device = device;
        _globalSamplers = globalSamplers;
        _renderer = renderer;
    }

    internal VulkanGraphicsContext AllocateGraphicsContext(string name)
    {
        VulkanGraphicsContext? context = null;

        if (_freeGraphicsContexts.Count > 0)
        {
            context = _freeGraphicsContexts.Last();
            _freeGraphicsContexts.RemoveAt(_freeGraphicsContexts.Count - 1);
        }

        if (context == null)
        {
            context = new VulkanGraphicsContext(new VulkanGraphicsContextInit
            {
                Device = _device,
                GlobalSamplers = _globalSamplers,
                Id = _graphicsContexts.Count,
                Renderer = _renderer
            });
            _graphicsContexts.Add(context);
        }

        context.Prepare(_currentBackbuffer!, name);
        context.BeginRecording();

        _inUseGraphicsContexts.Add(context);

        return context;
    }

    internal void AllocateComputeContext() => throw new NotImplementedException();

    internal void Release(VulkanGraphicsContext context)
    {
        _freeGraphicsContexts.Add(context);
    }

    internal void OnFrameBegin(VulkanTexture backbuffer)
    {
        _currentBackbuffer = backbuffer;
    }

}
