#pragma once

#include "Core/Types.h"

class RenderDevice;
struct ID3D12CommandQueue;
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
		ID3D12CommandQueue* GraphicsQueue = nullptr;
		RenderDevice*		Device = nullptr;
	};

	bool							Init(const InitParameters& Parameters);

	void							Resize(int32 Width, int32 Height);

private:

	IDXGISwapChain1*				SwapChain1;

	static const int32				BufferCount = 3;

	D3D12_CPU_DESCRIPTOR_HANDLE*	BackBufferRTVs;

};
