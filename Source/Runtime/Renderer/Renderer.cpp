#include "Renderer/Renderer.h"

#include <d3d12.h>
#include <dxgi1_6.h>

#include "Core/Assert.h"
#include "Core/Containers/Array.h"
#include "Renderer/D3D12PipelineStream.h"
#include "Renderer/D3D12RenderTargetView.h"
#include "Renderer/RenderContext.h"
#include "Renderer/RenderBuffer.h"
#include "Renderer/RenderDevice.h"
#include "Renderer/RenderSwapChain.h"
#include "Renderer/Shader.h"
#include "Renderer/ShaderInstance.h"

Renderer GRenderer;

TComPtr<ID3D12PipelineState> PipelineState;
TComPtr<ID3D12RootSignature> RootSig;

ShaderInstance DevShaderInstance;

ShaderBindingHandle sShaderTextureHandle;
ShaderBindingHandle sPerInstanceBufferHandle;
Shader sVertexShader(EShaderType::Vertex);
Shader sPixelShader(EShaderType::Pixel);

Renderer::Renderer()
{
	NumFramesRendered = 0;

	bInitialised = false;

	mDevice = nullptr;

	CommandContext = nullptr;
	Viewport = nullptr;

	mDefaultRedCheckerboardTexture = nullptr;
	mDefaultBlackCheckerboardTexture = nullptr;
}

bool Renderer::Initialise(const InitParameters& Parameters)
{
	UINT				DxgiFactoryFlags = 0;
	TComPtr<ID3D12Debug> DebugController;

	bool UseValidation = true;

	if (UseValidation && SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&DebugController))))
	{
		DebugController->EnableDebugLayer();

		// Enable additional debug layers.
		DxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;

		TComPtr<ID3D12Debug1> DebugInterface1;

		if (SUCCEEDED((DebugController->QueryInterface(IID_PPV_ARGS(&DebugInterface1)))))
		{
			DebugInterface1->SetEnableGPUBasedValidation(true);
		}
	}

	mDevice = new RenderDevice();

	if (mDevice->Initialise(DxgiFactoryFlags) == false)
	{
		GAssert(false);
	}

	const uint32 BackBufferCount = 3;

	Viewport = new RenderSwapChain();

	RenderSwapChain::InitParameters SwapChainParameters = {};

	SwapChainParameters.WindowHandle = Parameters.WindowHandle;
	SwapChainParameters.Extents = Parameters.ViewportExtents;
	SwapChainParameters.Device = mDevice;
	SwapChainParameters.BackBufferCount = BackBufferCount;

	Viewport->Init(SwapChainParameters);

	CommandContext = new RenderCommandContext(mDevice);

	CreateDefaultPipelineState();

	mDefaultBlackCheckerboardTexture = CreateTestTexture(0xFF000000);
	mDefaultBlackCheckerboardTexture->SetName("TestTexture");

	mDefaultRedCheckerboardTexture = CreateTestTexture(0xFF0000FF);

	mTestModelBuffer = new RenderBuffer();

	const uint32 size_per_element = sizeof(float) * 16;

	const uint32 num_max_instances = 32;

	mTestModelBuffer->Init(mDevice, num_max_instances, size_per_element, DXGI_FORMAT_UNKNOWN);

	TSpan<uint8> data = mTestModelBuffer->Lock();

	Matrix4x4* model_matrix = data.As<Matrix4x4>();

	model_matrix[0].SetToIdentity();

	model_matrix[0].SetTranslation({100.0f, 0.0f, 0.0f});

	mTestModelBuffer->Unlock();
	
	sPerInstanceBufferHandle = DevShaderInstance.GetParameter(EShaderType::Vertex, "cbPerFrameData");
	sShaderTextureHandle = DevShaderInstance.GetParameter(EShaderType::Pixel, "texture");

	bInitialised = true;

	return true;
}

void Renderer::Shutdown()
{
	mDevice->WaitForGpuIdle();

	Viewport->Shutdown();
}

void Renderer::BeginFrame(const IntVector2D& ViewportExtents)
{
	mDevice->BeginFrame();

	Viewport->Resize(ViewportExtents);

	CommandContext->Begin();

	RenderTexture* BackBufferTexture = Viewport->GetBackBuffer();

	CommandContext->SetRenderTarget(BackBufferTexture);
	CommandContext->SetPipelineState(PipelineState, RootSig);
	CommandContext->SetViewport(Rect(0, 0, Viewport->GetExtents().X, Viewport->GetExtents().Y));

	Rect viewport_rect = CommandContext->GetViewport();

	Matrix4x4 projection_matrix = Matrix4x4::sCreateOrthographic(viewport_rect.Left(), viewport_rect.Right(), viewport_rect.Bottom(), viewport_rect.Top(), -1.0f, 1.0f);

	CommandContext->SetProjectionMatrix(projection_matrix);
	CommandContext->SetShaderInstance(&DevShaderInstance);

	// Ready to do C# rendering now.
}

void Renderer::EndFrame()
{
	//DevShaderInstance.SetTexture(sShaderTextureHandle, mDefaultBlackCheckerboardTexture);
	//DevShaderInstance.SetBuffer(sPerInstanceBufferHandle, mTestModelBuffer);

	CommandContext->Draw();

	Viewport->Present(CommandContext);

	mDevice->EndFrame();

	//mDevice->WaitForGpuIdle();

	NumFramesRendered++;
}

void Renderer::CreateDefaultPipelineState()
{
	sVertexShader.Load("D:/UODev/UOEngineGitHub/Shaders/TexturedQuadVS.hlsl");
	sPixelShader.Load("D:/UODev/UOEngineGitHub/Shaders/TexturedQuadPS.hlsl");

	const int32 num_root_parameters = sVertexShader.GetNumBoundResources() + sPixelShader.GetNumBoundResources();

	TArray<D3D12_ROOT_PARAMETER1> root_parameters;

	sVertexShader.BuildRootSignature(root_parameters);
	sPixelShader.BuildRootSignature(root_parameters);

	D3D12_STATIC_SAMPLER_DESC static_sampler = {};

	static_sampler.Filter = D3D12_FILTER_MIN_MAG_MIP_POINT;
	static_sampler.AddressU = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
	static_sampler.AddressV = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
	static_sampler.AddressW = D3D12_TEXTURE_ADDRESS_MODE_WRAP;
	static_sampler.ShaderRegister = 0;
	static_sampler.RegisterSpace = 0;
	static_sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;

	D3D12_VERSIONED_ROOT_SIGNATURE_DESC root_sig_desc = {};

	root_sig_desc.Version = D3D_ROOT_SIGNATURE_VERSION_1_1;
	root_sig_desc.Desc_1_1.Flags = (D3D12_ROOT_SIGNATURE_FLAGS)0;
	root_sig_desc.Desc_1_1.NumParameters = root_parameters.Num();
	root_sig_desc.Desc_1_1.NumStaticSamplers = 1;
	root_sig_desc.Desc_1_1.pStaticSamplers = &static_sampler;
	root_sig_desc.Desc_1_1.pParameters = root_parameters.GetData();

	TComPtr<ID3DBlob> serialized_root_sig;
	TComPtr<ID3DBlob> error;
	
	HRESULT r = D3D12SerializeVersionedRootSignature(&root_sig_desc, &serialized_root_sig, &error);
	
	GAssert(serialized_root_sig != nullptr);

	(mDevice->GetDevice()->CreateRootSignature(0, serialized_root_sig->GetBufferPointer(), serialized_root_sig->GetBufferSize(), IID_PPV_ARGS(&RootSig)));

	D3D12PipelineStateStream pipeline_stream = {};

	pipeline_stream.SetRootSignature(RootSig);
	pipeline_stream.SetVertexShader(sVertexShader.GetBytecode(), sVertexShader.GetBytecodeLength());
	pipeline_stream.SetPixelShader(sPixelShader.GetBytecode(), sPixelShader.GetBytecodeLength());
	pipeline_stream.SetRasterizer();
	pipeline_stream.SetInputLayout();
	pipeline_stream.SetPrimitiveTopology();
	pipeline_stream.SetRenderTargetFormat(Viewport->GetFormat());
	pipeline_stream.SetBlendState();

	TComPtr<ID3D12PipelineState> new_pipeline_state = mDevice->CreatePipelineState(&pipeline_stream);

	PipelineState = new_pipeline_state;

	DevShaderInstance.Init(&sVertexShader, &sPixelShader);

}

RenderTexture* Renderer::CreateTestTexture(uint32 inCheckerboardColour)
{
	RenderTexture* texture = new RenderTexture();

	RenderTexture::TextureDescription desc = {};

	desc.Type = RenderTextureType::ReadOnly;
	desc.Width =  128;
	desc.Height = 64;
	desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	desc.Device = mDevice;

	texture->Init(desc);

	TArray<uint32> pixels;

	const uint32 bytes_per_pixel = 4;
	pixels.SetNum(desc.Width * desc.Height);

	for(uint32 y = 0; y < desc.Height; y++)
	{
		for (int32 x = 0; x < desc.Width; x++)
		{
			bool is_white = ((x / 1) % 2) ^ ((y / 1) % 2); // 1-texel squares
			
			pixels[y * desc.Width + x] = is_white ? 0xFFFFFFF : inCheckerboardColour;
		}
	}

	texture->InitialiseTextureData(pixels.AsSpan<uint8>());

	return texture;
}
