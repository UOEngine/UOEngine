#pragma once

#include "Core/Types.h"
#include "Renderer/RenderTexture.h"

class RenderCommandQueue;
class RenderDevice;
struct IDXGISwapChain1;
struct D3D12_CPU_DESCRIPTOR_HANDLE;

class RenderSwapChain
{
public:

									RenderSwapChain();

	struct InitParameters
	{
		int32				Width = 0;
		int32				Height = 0;
		void*				WindowHandle = nullptr;
		RenderDevice*		Device = nullptr;
		uint32				BackBufferCount = 0;
	};

	bool							Init(const InitParameters& Parameters);

	void							Resize(int32 Width, int32 Height);

	uint32							GetNextPresentIndex() const					{return CurrentBackBufferIndex;}
	RenderTexture*					GetBackBufferTexture(uint32 Index) const	{return &BackBufferTextures[Index]; }

	void							Present();

private:

	void							SetBackBufferIndex(uint32 Index)			{ CurrentBackBufferIndex = Index % BackBufferCount; }

	IDXGISwapChain1*				SwapChain1;

	uint32							BackBufferCount;

	RenderTexture*					BackBufferTextures;

	uint32							CurrentBackBufferIndex;

};
