// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Runtime.InteropServices;

using Vortice.Vulkan;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;
using System.Diagnostics;

namespace UOEngine.Runtime.Vulkan;

[DebuggerDisplay("VulkanSemaphore {Name}")]
internal class VulkanSemaphore: RhiSemaphore
{
    internal VkSemaphore Handle { get; private set; }

    private readonly VulkanDevice _device;

    private readonly bool _exportable;

    internal unsafe VulkanSemaphore(in RhiSemaphoreDescription description, VulkanDevice device)
    {
        _device = device;

        VkSemaphoreCreateInfo createInfo = new();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            throw new PlatformNotSupportedException("Need to implement this for other platforms.");
        }

        var exportInfo = new VkExportSemaphoreCreateInfo
        {
            handleTypes = VkExternalSemaphoreHandleTypeFlags.OpaqueWin32
        };

        if(description.Exportable)
        {
            _exportable = true;
            createInfo.pNext = &exportInfo;
        }

        VkSemaphore semaphore;

        device.Api.vkCreateSemaphore(device.Handle, &createInfo, null, &semaphore).CheckResult();

        Handle = semaphore;

        if (description.Name != null)
        {
            Name = description.Name;
            VulkanDebug.SetDebugName(Handle, Name);
        }

        Handle = semaphore;

        if (_exportable)
        {
            ExportedHandle = Export();
        }
    }

    private unsafe nint Export()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            throw new PlatformNotSupportedException("Need to implement this for other platforms.");
        }

        var handleInfo = new VkSemaphoreGetWin32HandleInfoKHR
        {
            semaphore = Handle,
            handleType = VkExternalSemaphoreHandleTypeFlags.OpaqueWin32
        };

        _device.Api.vkGetSemaphoreWin32HandleKHR(_device.Handle, &handleInfo, out var handle);

        return handle;
    }
}
