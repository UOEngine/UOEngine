// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanShaderProgram: IDisposable
{
    public readonly ShaderProgramType Type;

    public readonly ShaderParameter[] InputBindings = [];
    public readonly ShaderStreamBinding[] StreamBindings = [];

    public VkShaderModule Handle => _handle;

    public readonly string EntryPoint;
    private bool _disposed;

    private readonly VulkanDevice _device;

    private VkShaderModule _handle;

    internal unsafe VulkanShaderProgram(VulkanDevice device, ShaderProgramType shaderProgramType, in ShaderProgramCompileResult compileResult)
    {
        _device = device;
        Type = shaderProgramType;

        StreamBindings = compileResult.StreamBindings;
        InputBindings = compileResult.ShaderBindings;

        EntryPoint = compileResult.EntryPointName;

        Span<byte> entryPointNameAsBytes = Encoding.ASCII.GetBytes(compileResult.EntryPointName);

        device.Api.vkCreateShaderModule(device.Handle, compileResult.ByteCode.AsSpan(), null, out _handle);

        UOEDebug.Assert(Handle != VkShaderModule.Null);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            _device.Api.vkDestroyShaderModule(_device.Handle, _handle);
            _handle = VkShaderModule.Null;

            _disposed = true;
        }
    }

    ~VulkanShaderProgram()
    {
        Dispose(disposing: false);
    }
}
