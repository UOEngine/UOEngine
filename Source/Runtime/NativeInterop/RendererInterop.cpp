#include "NativeInterop/RendererInterop.h"

#include "D3D12RHI/RenderContext.h"
#include "D3D12RHI/RenderDevice.h"
#include "D3D12RHI/RenderTexture.h"
#include "D3D12RHI/Renderer.h"

RenderTexture* CreateTexture(uint32 Width, uint32 Height, const char* inName)
{
	RenderTexture* texture = new RenderTexture();

	RenderTexture::TextureDescription desc = {};

	desc.Type = RenderTextureType::ReadOnly;
	desc.Width = Width;
	desc.Height = Height;
	desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	desc.Device = GRenderer.GetDevice();
	desc.Name = inName;

	texture->Init(desc);

	return texture;
}

void SetTextureData(RenderTexture* inTexture, uint8* inData, uint32 inSize)
{
	inTexture->InitialiseTextureData(TSpan<uint8>(inData, inSize));
}