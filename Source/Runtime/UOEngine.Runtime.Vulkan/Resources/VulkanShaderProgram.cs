// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Text;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal class VulkanShaderProgram
{
    public readonly ShaderProgramType Type;

    public readonly ShaderParameter[] InputBindings = [];
    public readonly ShaderStreamBinding[] StreamBindings = [];

    public readonly VkShaderModule Handle;

    internal unsafe VulkanShaderProgram(VulkanDevice device, ShaderProgramType type, in ShaderProgramCompileResult compileResult)
    {
        Type = type;

        StreamBindings = compileResult.StreamBindings;
        InputBindings = compileResult.ShaderBindings;

        Span<byte> entryPointNameAsBytes = Encoding.ASCII.GetBytes(compileResult.EntryPointName);

        device.Api.vkCreateShaderModule(device.Handle, compileResult.ByteCode.AsSpan(), null, out Handle);

        UOEDebug.Assert(Handle != VkShaderModule.Null);

    }
}
