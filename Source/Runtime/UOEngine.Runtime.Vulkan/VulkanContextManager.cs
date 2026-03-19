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


    internal VulkanContextManager(VulkanDevice device, VulkanGlobalSamplers globalSamplers)
    {
        _device = device;
        _globalSamplers = globalSamplers;
    }

    internal VulkanGraphicsContext AllocateGraphicsContext(string name)
    {
        VulkanGraphicsContext? context = null;

        for (int i = 0; i < _freeGraphicsContexts.Count; i++)
        {
             _freeGraphicsContexts[i]?.SubmitFence?.Refresh();

            if (_freeGraphicsContexts[i]!.SubmitFence!.IsSignaled)
            {
                context = _freeGraphicsContexts[i];
                context.SubmitFence.Reset();

                _freeGraphicsContexts.RemoveAt(i);

                break;
            }
        }

        if (context == null)
        {
            context = new VulkanGraphicsContext(_device, _globalSamplers, _graphicsContexts.Count);
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
