using System.Runtime.CompilerServices;

using static SDL3.SDL;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.SDL3GPU;

internal class Sdl3GpuBuffer: Sdl3GpuResource
{
    public readonly RhiBufferType Type;
    public readonly byte[] Data;

    private readonly SDL_GPUBufferCreateInfo _description;
    private SDL_GPUBufferBinding _bufferBinding = new();

    private readonly Sdl3GpuDevice _device;

    public Sdl3GpuBuffer(Sdl3GpuDevice device, RhiBufferType type, uint sizeInBytes, string name = "")
        : base(device, SDL_SetGPUBufferName, name)
    {
        // SDL_SetGPUBufferName seems to not be set in the .c.
        Type = type;
        _device = device;

        switch (type)
        {
            case RhiBufferType.Index:
                {
                    _description.usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_INDEX;
                    break;
                }

            case RhiBufferType.Vertex:
                {
                    _description.usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX;

                    break;
                }

            default:
                {
                    throw new SwitchExpressionException("Unhandled GPU buffer type.");
                }
        }

        _description.size = sizeInBytes;

        Handle = SDL_CreateGPUBuffer(device.Handle, _description);

        _bufferBinding.buffer = Handle;

        Data = new byte[sizeInBytes];
    }

    public void SetData(int offsetInBytes, nint data, int dataLength)
    {
        var destination = Data.AsSpan(offsetInBytes, dataLength);

        unsafe
        {
            fixed (byte* destinationPtr = destination)
            {
                Buffer.MemoryCopy((void*)data, destinationPtr, dataLength, dataLength);
            }
        }
    }

    public void Upload()
    {
        // Eventually we will want a ring buffer to submit to in batches, but quick and easy for now...
        var createInfo = new SDL_GPUTransferBufferCreateInfo
        {
            size = _description.size,
            usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD
        };

        IntPtr transferBuffer = SDL_CreateGPUTransferBuffer(_device.Handle, createInfo);

        IntPtr mappedMemory = SDL_MapGPUTransferBuffer(Device.Handle, transferBuffer, false);

        unsafe
        {
            fixed (void* src = Data)
            {
                Buffer.MemoryCopy(src, (void*)mappedMemory, _description.size, _description.size);
            }
        }

        SDL_UnmapGPUTransferBuffer(Device.Handle, mappedMemory);

        IntPtr commandBuffer = SDL_AcquireGPUCommandBuffer(Device.Handle);
        IntPtr copyPass = SDL_BeginGPUCopyPass(commandBuffer);

        SDL_GPUTransferBufferLocation location = new SDL_GPUTransferBufferLocation
        {
            transfer_buffer = transferBuffer,
            
            offset = 0
        };

        var region = new SDL_GPUBufferRegion
        {
            buffer = Handle,
            offset = 0,
            size = _description.size
        };

        SDL_UploadToGPUBuffer(copyPass, location, region, false);

        SDL_EndGPUCopyPass(copyPass);
        SDL_SubmitGPUCommandBuffer(commandBuffer);

        SDL_ReleaseGPUTransferBuffer(Device.Handle, transferBuffer);
    }

    public void Bind(IntPtr renderPassHandle)
    {
        switch (Type)
        {
            case RhiBufferType.Index:
                {
                    SDL_BindGPUIndexBuffer(renderPassHandle, _bufferBinding, SDL_GPUIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT);
                    break;
                }
            default:
                {
                    SDL_BindGPUVertexBuffers(renderPassHandle, 0, [_bufferBinding], 1);
                    break;
                }
        }
    }

    protected override void FreeResource()
    {
        SDL_ReleaseGPUBuffer(Device.Handle, _bufferBinding.buffer);

        _bufferBinding.buffer = IntPtr.Zero;
    }
}
