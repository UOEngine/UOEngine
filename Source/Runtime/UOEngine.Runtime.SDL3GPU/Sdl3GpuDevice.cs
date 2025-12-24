// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using static SDL3.SDL;

using UOEngine.Runtime.RHI;
using UOEngine.Runtime.Core;
using System.Runtime.InteropServices;

namespace UOEngine.Runtime.SDL3GPU;

internal enum Sdl3GpuBackend
{
    None = 0,
    D3D12,
    Vulkan,
    Metal,

    Count
}

internal class Sdl3GpuDevice//: IRenderDevice
{
    public Sdl3GpuBackend Backend { get; private set; }

    public bool DebugDevice { get; private set; }

    public SDL_GPUShaderFormat ShaderFormat { get; private set; }

    public IntPtr Handle
    {
        get
        {
            UOEDebug.Assert(_handle != IntPtr.Zero);

            return _handle;
        }
    }

    private Action? OnDeviceCreated;

    private IntPtr _handle;

    public void Setup()
    {
        string? driver = null;

        // Set default rendering backend.
        if(OperatingSystem.IsWindows())
        {
            ShaderFormat = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL;
            driver = "direct3d12";
            Backend = Sdl3GpuBackend.D3D12;
        }
        else if(OperatingSystem.IsLinux())
        {
            ShaderFormat = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;
            driver = "vulkan";
            Backend = Sdl3GpuBackend.Vulkan;
        }
        else
        {
            // No MacOS..yet.
            throw new NotSupportedException("Platform not currently supported.");
        }

        // Override rendering backend, otherwise SDL picks its own default
        // based on host platform.
        if (CommandLine.HasOption("-d3d12"))
        {
            ShaderFormat = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL;
            driver = "direct3d12";
            Backend = Sdl3GpuBackend.D3D12;
        }
        else if (CommandLine.HasOption("-vulkan"))
        {
            ShaderFormat = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;
            driver = "vulkan";
            Backend = Sdl3GpuBackend.Vulkan;
        }

        DebugDevice = false;

        if (CommandLine.HasOption("-debugdevice"))
        {
            DebugDevice = true;
        }

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            throw new Exception("SDL_Init failed: " + SDL_GetError());
        }

        _handle = SDL_CreateGPUDevice(ShaderFormat, DebugDevice, driver);

        if (_handle == IntPtr.Zero)
        {
            throw new Exception("Failed to initialise GPU device.");
        }

        OnDeviceCreated?.Invoke();
    }

    public void RegisterOnDeviceCreatedCallback(Action action)
    {
        if (_handle != IntPtr.Zero)
        {
            action();

            return;
        }

        OnDeviceCreated += action;
    }

    public void WaitForGpuIdle()
    {
        SDL_WaitForGPUIdle(Handle);
    }
}
