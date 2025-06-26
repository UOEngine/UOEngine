#pragma once

#include "Core/Math/Vector2D.h"
#include "Core/Types.h"
#include "Renderer/RenderTexture.h"

class RenderCommandContext;
class RenderCommandQueue;
class RenderDevice;
enum DXGI_FORMAT;
struct IDXGISwapChain3;
struct D3D12_CPU_DESCRIPTOR_HANDLE;

class RenderSwapChain
{
public:

									RenderSwapChain();

	struct InitParameters
	{
		IntVector2D			Extents;
		void*				WindowHandle = nullptr;
		RenderDevice*		Device = nullptr;
		uint32				BackBufferCount = 0;
	};

	bool							Init(const InitParameters& Parameters);
	void							Shutdown();

	void							Resize(const IntVector2D& NewExtents);

	IntVector2D						GetExtents() const							{return Extents;}

	DXGI_FORMAT						GetFormat() const							{return Format;}

	RenderTexture*					GetBackBuffer() const						{return &BackBufferTextures[CurrentBackBufferIndex]; }

	uint32							GetNextPresentIndex() const					{return CurrentBackBufferIndex;}
	RenderTexture*					GetBackBufferTexture(uint32 Index) const	{return &BackBufferTextures[Index]; }

	void							Present(RenderCommandContext* CommandContext);

	void							FlushCommands();

private:

	void							CreateBackBufferTextures();

	IDXGISwapChain3*				SwapChain;

	uint32							BackBufferCount;

	RenderTexture*					BackBufferTextures;

	uint32							CurrentBackBufferIndex;

	IntVector2D						Extents;

	RenderDevice*					Device;

	DXGI_FORMAT						Format;

};
