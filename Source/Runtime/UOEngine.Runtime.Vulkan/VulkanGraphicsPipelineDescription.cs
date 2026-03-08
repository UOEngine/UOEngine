// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Vortice.Vulkan;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Vulkan;

internal record struct VulkanGraphicsPipelineDescription
{
    internal RhiGraphicsPipelineDescription Description { get; set; }
    internal VkFormat AttachmentFormat { get; set; }
}
