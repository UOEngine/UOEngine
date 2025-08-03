#include "NativeInterop/RenderBufferNative.h"

#include "D3D12RHI/Renderer.h"
#include "D3D12RHI/RenderBuffer.h"

RenderBuffer* CreateRenderBuffer(uint32 inNumElements, uint32 inStride)
{
	RenderBuffer* buffer = new RenderBuffer();

	buffer->Init(GRenderer.GetDevice(), inNumElements, inStride, DXGI_FORMAT_UNKNOWN);

	return buffer;
}

void SetData(RenderBuffer* inBuffer, void* inData, uint32 inNumBytes)
{
	TSpan<uint8> data = inBuffer->Lock();

	GAssert(inNumBytes <= data.Num());

	Memory::MemCopy(data.GetData(), data.Num(), inData, inNumBytes);

	inBuffer->Unlock();
}
