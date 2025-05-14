#include "Renderer/RenderSwapChain.h" 

#include <d3d12.h>
#include <dxgi1_6.h>
#include <wrl/client.h>

#include "Core/Assert.h"

#include "Renderer/RenderContext.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"

using Microsoft::WRL::ComPtr;

RenderSwapChain::RenderSwapChain()
{
	BackBufferCount = 0;
	BackBufferTextures = nullptr;
	CurrentBackBufferIndex = 0;
}

bool RenderSwapChain::Init(const InitParameters& Parameters)
{
	ComPtr<IDXGIFactory4> DxgiFactory;

	if (SUCCEEDED(CreateDXGIFactory2(0, IID_PPV_ARGS(&DxgiFactory))) == false)
	{
		GAssert(false);
	}

	BackBufferCount = Parameters.BackBufferCount;

	SetBackBufferIndex(0);

	BackBufferTextures = new RenderTexture[BackBufferCount];

	Device = Parameters.Device;

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	SwapChainDesc.Width = Parameters.Extents.X;
	SwapChainDesc.Height = Parameters.Extents.Y;
	SwapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	SwapChainDesc.Stereo = FALSE;
	SwapChainDesc.SampleDesc = { 1, 0 };
	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	SwapChainDesc.BufferCount = BackBufferCount;
	SwapChainDesc.Scaling = DXGI_SCALING_NONE;
	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
	SwapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

	// It is recommended to always allow tearing if tearing support is available.
	SwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	//SwapChainDesc.Flags |= CheckTearingSupport() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

	HWND WindowHandle = (HWND)Parameters.WindowHandle;

	ID3D12CommandQueue* CommandQueue = Device->GetQueue(ERenderQueueType::Direct)->GetQueue();

	GAssert(CommandQueue != nullptr);

	if (SUCCEEDED(DxgiFactory->CreateSwapChainForHwnd(CommandQueue, WindowHandle, &SwapChainDesc, nullptr, nullptr, &SwapChain1)) == false)
	{
		GAssert(false);
	}

	CreateBackBufferTextures();

	return true;
}

void RenderSwapChain::Resize(const Vector2D& NewExtents)
{
	if (NewExtents == Extents)
	{
		return;
	}

	//RenderFence Fence;

	//Fence.Signal();
	//Fence.Wait();

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	if (FAILED(SwapChain1->GetDesc1(&SwapChainDesc)))
	{
		GAssert(false);
	}

	if (FAILED(SwapChain1->ResizeBuffers(BackBufferCount, NewExtents.X, NewExtents.Y, SwapChainDesc.Format, SwapChainDesc.Flags)))
	{
		GAssert(false);
	}

	Extents = NewExtents;

	CreateBackBufferTextures();
}

void RenderSwapChain::Present(RenderCommandContext* CommandContext)
{
	CommandContext->TransitionResource(GetBackBufferTexture(CurrentBackBufferIndex)->GetResource(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT);

	CommandContext->FlushCommands();

	SwapChain1->Present(0, 0);

	//RenderFence Fence;

	//Fence.Signal();
	//Fence.Wait();
	
	SetBackBufferIndex(CurrentBackBufferIndex + 1);
}

void RenderSwapChain::CreateBackBufferTextures()
{
	RenderTexture::TextureDescription TextureInitParameters;

	TextureInitParameters.Device = Device;
	TextureInitParameters.Type = RenderTextureType::RenderTarget;

	for (int32 i = 0; i < BackBufferCount; i++)
	{
		ComPtr<ID3D12Resource> BackBuffer;

		SwapChain1->GetBuffer(i, IID_PPV_ARGS(&BackBuffer));

		TextureInitParameters.Resource = BackBuffer.Get();

		BackBufferTextures[i].Init(TextureInitParameters);
	}
}

