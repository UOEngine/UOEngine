// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Diagnostics;
using System.Text;

using static SDL3.SDL;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU.Resources;

internal class SDL3GPUShaderProgram: Sdl3GpuResource
{
    public readonly ShaderProgramType Type;

    public readonly ShaderParameter[] InputBindings = [];
    public readonly ShaderStreamBinding[] StreamBindings = [];

    public SDL3GPUShaderProgram(Sdl3GpuDevice device, ShaderProgramType type, in ShaderProgramCompileResult compileResult)
        : base(device)
    {
        Debug.Assert(device.Handle != IntPtr.Zero);

        Type = type;

        SDL_GPUShaderStage stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX;

        switch(type)
        {
            case ShaderProgramType.Vertex:  stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX; break;
            case ShaderProgramType.Pixel: stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT; break;
            default: Debug.Assert(false); break;    
        }

        StreamBindings = compileResult.StreamBindings;
        InputBindings = compileResult.ShaderBindings;

        Span<byte> entryPointNameAsBytes = Encoding.ASCII.GetBytes(compileResult.EntryPointName);

        unsafe
        {
            fixed(byte* code = &compileResult.ByteCode[0])
            fixed (byte* p = entryPointNameAsBytes)
            {
                var createInfo = new SDL_GPUShaderCreateInfo()
                {
                    code = code,
                    code_size = (UIntPtr)compileResult.ByteCode.Length,
                    entrypoint = p,
                    stage = stage,
                    format = device.ShaderFormat,
                    num_uniform_buffers = (uint)compileResult.ShaderBindings.Count(p => p.InputType == RhiShaderInputType.Constant),
                    num_samplers = (uint)compileResult.ShaderBindings.Count(p => p.InputType == RhiShaderInputType.Sampler),
                };

                Handle = SDL_CreateGPUShader(Device.Handle, createInfo);
            }
        }

        Debug.Assert(Handle !=  IntPtr.Zero);
    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUShader(Device.Handle, Handle);
    }
}
