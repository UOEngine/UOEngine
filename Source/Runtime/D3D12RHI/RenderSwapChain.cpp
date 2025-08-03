#include "RenderSwapChain.h" 

#include <d3d12.h>
#include <dxgi1_6.h>

#include "Core/Assert.h"
#include "D3D12Resource.h"
#include "RenderContext.h"
#include "RenderCommandQueue.h"
#include "RenderDevice.h"
#include "RenderFence.h"

RenderSwapChain::RenderSwapChain()
{
	mBackBufferCount = 0;
	mCurrentBackBufferIndex = 0;
	mDevice = nullptr;
}

bool RenderSwapChain::Init(const InitParameters& Parameters)
{
	TComPtr<IDXGIFactory4> DxgiFactory;

	if (SUCCEEDED(CreateDXGIFactory2(0, IID_PPV_ARGS(&DxgiFactory))) == false)
	{
		GAssert(false);
	}

	mBackBufferCount = Parameters.BackBufferCount;

	mBackBufferTextures.SetNum(mBackBufferCount);

	for (int32 i = 0; i < mBackBufferCount; i++)
	{
		mBackBufferTextures[i] = new RenderTexture();
	}

	mDevice = Parameters.Device;

	Format = DXGI_FORMAT_R8G8B8A8_UNORM;

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	SwapChainDesc.Width = Parameters.Extents.X;
	SwapChainDesc.Height = Parameters.Extents.Y;
	SwapChainDesc.Format = Format;
	SwapChainDesc.Stereo = FALSE;
	SwapChainDesc.SampleDesc = { 1, 0 };
	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	SwapChainDesc.BufferCount = mBackBufferCount;
	SwapChainDesc.Scaling = DXGI_SCALING_NONE;
	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
	SwapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

	// It is recommended to always allow tearing if tearing support is available.
	//SwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	//SwapChainDesc.Flags |= CheckTearingSupport() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

	HWND WindowHandle = (HWND)Parameters.WindowHandle;

	ID3D12CommandQueue* CommandQueue = mDevice->GetQueue(ERenderQueueType::Direct)->GetQueue();

	GAssert(CommandQueue != nullptr);

	TComPtr<IDXGISwapChain1> SwapChain1;

	if (SUCCEEDED(DxgiFactory->CreateSwapChainForHwnd(CommandQueue, WindowHandle, &SwapChainDesc, nullptr, nullptr, &SwapChain1)) == false)
	{
		GAssert(false);
	}

	if (FAILED(SwapChain1->QueryInterface(__uuidof(IDXGISwapChain4), (void**)&SwapChain)))
	{
		GAssert(false);
	}

	mExtents = Parameters.Extents;

	CreateBackBufferTextures();

	return true;
}

void RenderSwapChain::Shutdown()
{
	for (int32 i = 0; i < mBackBufferCount; i++)
	{
		mBackBufferTextures[i]->Release();
	}

	SwapChain->Release();
	SwapChain = nullptr;
}

void RenderSwapChain::Resize(const IntVector2D& NewExtents)
{
	if (NewExtents == mExtents)
	{
		return;
	}

	if (NewExtents.X == 0 || NewExtents.Y == 0)
	{
		return;
	}

	mExtents = NewExtents;

	mDevice->GetQueue(ERenderQueueType::Direct)->WaitUntilIdle();

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	if (FAILED(SwapChain->GetDesc1(&SwapChainDesc)))
	{
		GAssert(false);
	}

	for (int32 i = 0; i < mBackBufferCount; i++)
	{
		mBackBufferTextures[i]->Release();
	}

	if (FAILED(SwapChain->ResizeBuffers(mBackBufferCount, NewExtents.X, NewExtents.Y, SwapChainDesc.Format, SwapChainDesc.Flags)))
	{
		GAssert(false);
	}

	mCurrentBackBufferIndex = SwapChain->GetCurrentBackBufferIndex();

	CreateBackBufferTextures();
}

void RenderSwapChain::Present(RenderCommandContext* CommandContext)
{
	CommandContext->TransitionResource(GetBackBufferTexture(mCurrentBackBufferIndex)->GetResource()->Get(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT);

	CommandContext->FlushCommands();

	SwapChain->Present(1, 0);

	mCurrentBackBufferIndex = SwapChain->GetCurrentBackBufferIndex();
}

void RenderSwapChain::CreateBackBufferTextures()
{
	for (int32 i = 0; i < mBackBufferCount; i++)
	{
		TComPtr<ID3D12Resource> backbuffer_resource;

		SwapChain->GetBuffer(i, IID_PPV_ARGS(&backbuffer_resource));

		backbuffer_resource->SetName(TEXT("BackBuffer"));

		mBackBufferTextures[i]->InitFromExternalResource(mDevice, backbuffer_resource, true);

	}
}

