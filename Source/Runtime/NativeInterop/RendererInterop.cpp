#include "NativeInterop/RendererInterop.h"

#include "Renderer/RenderContext.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderTexture.h"
#include "Renderer/Renderer.h"

RenderTexture* CreateTexture(uint32 Width, uint32 Height)
{
	RenderTexture* texture = new RenderTexture();

	RenderTexture::TextureDescription desc = {};

	desc.Type = RenderTextureType::ReadOnly;
	desc.Width = Width;
	desc.Height = Height;
	desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	desc.Device = GRenderer.GetDevice();

	texture->Init(desc);

	return texture;
}

void SetTextureData(RenderTexture* inTexture, uint8* inData, uint32 inSize)
{
	inTexture->InitialiseTextureData(TSpan<uint8>(inData, inSize));
}