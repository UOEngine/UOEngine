using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    internal static class Vulkan
    {
        internal static unsafe Result VkCreateFrameBuffer(Device device, in FramebufferCreateInfo createInfo, out Framebuffer outFramebuffer)
        {
            var framebuffers = stackalloc Framebuffer[1];

            Result result =  Vk.GetApi().CreateFramebuffer(device, in createInfo, null, framebuffers);

            outFramebuffer = framebuffers[0];

            return result;
        }

        internal static unsafe void VkDestroyImageView(Device device, ImageView imageView)
        {
            Vk.GetApi().DestroyImageView(device, imageView, null);
        }

        internal static unsafe void VkDestroyImage(Device device, Image image)
        {
            Vk.GetApi().DestroyImage(device, image, null);
        }

        internal static Result VkResetDescriptorPool(Device device, DescriptorPool descriptorPool)
        {
            Result result = _vk.ResetDescriptorPool(device, descriptorPool, 0);

            return result;
        }

        internal static void VkCmdBindDescriptorSets(CommandBuffer buffer, PipelineBindPoint pipelineBindPoint, PipelineLayout layout, ReadOnlySpan<DescriptorSet> descriptorSets)
        {
            _vk.CmdBindDescriptorSets(buffer, pipelineBindPoint, layout, 0, descriptorSets, null);
        }

        internal static unsafe void VkDestroyBuffer(Device device, Buffer buffer)
        {
            _vk.DestroyBuffer(device, buffer, null);
        }

        internal static unsafe void VkFreeMemory(Device device, DeviceMemory memory)
        {
            _vk.FreeMemory(device, memory, null);
        }

        internal static void VkBindVertexBuffer(CommandBuffer commandBuffer, Buffer buffer)
        {
            _vk.CmdBindVertexBuffers(commandBuffer, 0, [buffer], [0]);
        }

        internal static unsafe Result VkMapMemory(Device device, DeviceMemory memory, ulong offset, ulong size, void** data)
        {
            Result result = _vk.MapMemory(device, memory, offset, size, MemoryMapFlags.None, data);

            return result;
        }

        internal static void VkUnmapMemory(Device device, DeviceMemory memory)
        {
            _vk.UnmapMemory(device, memory);
        }

        static readonly Vk _vk = Vk.GetApi();
    }
}
