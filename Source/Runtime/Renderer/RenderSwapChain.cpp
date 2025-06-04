#include "Renderer/RenderSwapChain.h" 

#include <d3d12.h>
#include <dxgi1_6.h>
#include <wrl/client.h>

#include "Core/Assert.h"

#include "Renderer/RenderContext.h"
#include "Renderer/RenderCommandQueue.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderFence.h"

using Microsoft::WRL::ComPtr;

RenderSwapChain::RenderSwapChain()
{
	BackBufferCount = 0;
	BackBufferTextures = nullptr;
	CurrentBackBufferIndex = 0;
	Device = nullptr;
}

bool RenderSwapChain::Init(const InitParameters& Parameters)
{
	ComPtr<IDXGIFactory4> DxgiFactory;

	if (SUCCEEDED(CreateDXGIFactory2(0, IID_PPV_ARGS(&DxgiFactory))) == false)
	{
		GAssert(false);
	}

	BackBufferCount = Parameters.BackBufferCount;

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
	//SwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	//SwapChainDesc.Flags |= CheckTearingSupport() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

	HWND WindowHandle = (HWND)Parameters.WindowHandle;

	ID3D12CommandQueue* CommandQueue = Device->GetQueue(ERenderQueueType::Direct)->GetQueue();

	GAssert(CommandQueue != nullptr);

	ComPtr<IDXGISwapChain1> SwapChain1;

	if (SUCCEEDED(DxgiFactory->CreateSwapChainForHwnd(CommandQueue, WindowHandle, &SwapChainDesc, nullptr, nullptr, &SwapChain1)) == false)
	{
		GAssert(false);
	}

	if (FAILED(SwapChain1->QueryInterface(__uuidof(IDXGISwapChain4), (void**)&SwapChain)))
	{
		GAssert(false);
	}

	Extents = Parameters.Extents;

	CreateBackBufferTextures();

	return true;
}

void RenderSwapChain::Shutdown()
{
	for (int32 i = 0; i < BackBufferCount; i++)
	{
		BackBufferTextures[i].Release();
	}

	SwapChain->Release();
	SwapChain = nullptr;
}

void RenderSwapChain::Resize(const IntVector2D& NewExtents)
{
	if (NewExtents == Extents)
	{
		return;
	}

	if (NewExtents.X == 0 || NewExtents.Y == 0)
	{
		return;
	}

	Extents = NewExtents;

	Device->GetQueue(ERenderQueueType::Direct)->WaitUntilIdle();

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	if (FAILED(SwapChain->GetDesc1(&SwapChainDesc)))
	{
		GAssert(false);
	}

	if (FAILED(SwapChain->ResizeBuffers(BackBufferCount, NewExtents.X, NewExtents.Y, SwapChainDesc.Format, SwapChainDesc.Flags)))
	{
		GAssert(false);
	}

	CurrentBackBufferIndex = SwapChain->GetCurrentBackBufferIndex();

	CreateBackBufferTextures();
}

void RenderSwapChain::Present(RenderCommandContext* CommandContext)
{
	CommandContext->TransitionResource(GetBackBufferTexture(CurrentBackBufferIndex)->GetResource(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT);

	CommandContext->FlushCommands();

	SwapChain->Present(1, 0);

	CurrentBackBufferIndex = SwapChain->GetCurrentBackBufferIndex();
}

void RenderSwapChain::CreateBackBufferTextures()
{
	RenderTexture::TextureDescription TextureInitParameters;

	TextureInitParameters.Device = Device;
	TextureInitParameters.Type = RenderTextureType::RenderTarget;
	TextureInitParameters.Width = Extents.X;
	TextureInitParameters.Height = Extents.Y;
	TextureInitParameters.Format = DXGI_FORMAT_R8G8B8A8_UNORM;

	for (int32 i = 0; i < BackBufferCount; i++)
	{
		ComPtr<ID3D12Resource> BackBuffer;

		SwapChain->GetBuffer(i, IID_PPV_ARGS(&BackBuffer));

		TextureInitParameters.Resource = BackBuffer.Get();

		TextureInitParameters.Resource->SetName(TEXT("BackBuffer"));

		BackBufferTextures[i].Release();

		BackBufferTextures[i].Init(TextureInitParameters);
	}
}

