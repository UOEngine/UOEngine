// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;
using Vortice.Vulkan;

namespace UOEngine.Runtime.Vulkan;

internal static class RhiTypesToVulkanTypes
{
    internal static VkPrimitiveTopology ToVkPrimitiveTopology(this RhiPrimitiveType primitiveType) => primitiveType switch
    {
        RhiPrimitiveType.TriangleList => VkPrimitiveTopology.TriangleList,
        _ => throw new NotImplementedException()
    };

    internal static VkFormat ToVkFormat(this RhiVertexAttributeFormat vertexFormat) => vertexFormat switch
    {
        RhiVertexAttributeFormat.Vector2 => VkFormat.R32G32Sfloat,
        RhiVertexAttributeFormat.Vector3 => VkFormat.R32G32B32Sfloat,
        RhiVertexAttributeFormat.Vector4 => VkFormat.R32G32B32A32Sfloat,
        RhiVertexAttributeFormat.UInt32 => VkFormat.R32Uint,
        RhiVertexAttributeFormat.R8G8B8A8_UNorm => VkFormat.R8G8B8A8Unorm,
        _ => throw new NotImplementedException()
    };

    internal static VkShaderStageFlags ToVkShaderStage(this ShaderProgramType shaderProgramType) => shaderProgramType switch
    {
        ShaderProgramType.Vertex => VkShaderStageFlags.Vertex,
        ShaderProgramType.Pixel => VkShaderStageFlags.Fragment,
        _ => throw new NotImplementedException()
    };

    internal static VkDescriptorType ToVkDescriptorType(this RhiShaderInputType shaderInputType) => shaderInputType switch
    {
        RhiShaderInputType.Constant => VkDescriptorType.UniformBuffer,
        RhiShaderInputType.Texture => VkDescriptorType.SampledImage,
        RhiShaderInputType.Sampler => VkDescriptorType.Sampler,
        _ => throw new NotImplementedException()
    };
}
