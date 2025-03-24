#include "Renderer/RenderSwapChain.h" 

#include <d3d12.h>
#include <dxgi1_6.h>
#include <wrl/client.h>

#include "Core/Assert.h"

#include "Renderer/RendererDevice.h"

using Microsoft::WRL::ComPtr;

RenderSwapChain::RenderSwapChain()
{
	BackBufferRTVs = nullptr;
}

bool RenderSwapChain::Init(const InitParameters& Parameters)
{
	ComPtr<IDXGIFactory4> DxgiFactory;

	if (SUCCEEDED(CreateDXGIFactory2(0, IID_PPV_ARGS(&DxgiFactory))) == false)
	{
		GAssert(false);
	}

	BackBufferRTVs = new D3D12_CPU_DESCRIPTOR_HANDLE[BufferCount];

	DXGI_SWAP_CHAIN_DESC1	SwapChainDesc = {};

	SwapChainDesc.Width = Parameters.Width;
	SwapChainDesc.Height = Parameters.Height;
	SwapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	SwapChainDesc.Stereo = FALSE;
	SwapChainDesc.SampleDesc = { 1, 0 };
	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	SwapChainDesc.BufferCount = BufferCount;
	SwapChainDesc.Scaling = DXGI_SCALING_NONE;
	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
	SwapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

	// It is recommended to always allow tearing if tearing support is available.
	SwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	//SwapChainDesc.Flags |= CheckTearingSupport() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

	HWND WindowHandle = (HWND)Parameters.WindowHandle;

	if (SUCCEEDED(DxgiFactory->CreateSwapChainForHwnd(Parameters.Device->GetGraphicsQueue(), WindowHandle, &SwapChainDesc, nullptr, nullptr, &SwapChain1)) == false)
	{
		GAssert(false);
	}

	for (int32 i = 0; i < BufferCount; i++)
	{
		ComPtr<ID3D12Resource> BackBuffer;

		SwapChain1->GetBuffer(i, IID_PPV_ARGS(&BackBuffer));

		BackBufferRTVs[i] = Parameters.Device->CreateRenderTargetView(BackBuffer.Get());
	}

	//if (SUCCEEDED(DxgiFactory->MakeWindowAssociation(WindowHandle, )) == false)
	//{
	//	GAssert(false);
	//}

	return true;
}

void RenderSwapChain::Resize(int32 Width, int32 Height)
{

}
