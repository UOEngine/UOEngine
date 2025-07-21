#pragma once

#include "Core/Math/Matrix4x4.h"
#include "Core/Math/Rect.h"
#include "Core/Types.h"
#include "Renderer/RenderCommandList.h"

class D3D12RenderTargetView;
class RenderCommandAllocator;
class RenderCommandList;
class RenderDevice;
class RenderTexture;
enum class ERenderQueueType: uint8;
enum D3D12_RESOURCE_STATES;
struct ID3D12GraphicsCommandList;
struct ID3D12PipelineState;
struct ID3D12RootSignature;

class RenderCommandContext
{
public:

									RenderCommandContext(RenderDevice* InDevice);

	static RenderCommandContext&	Get();

	void							Begin();
	void							End();

	void							BeginRenderPass();
	void							EndRenderPass();

	void							SetRenderTarget(RenderTexture* View);

	void							SetPipelineState(ID3D12PipelineState* PipelineState, ID3D12RootSignature* RootSignature);

	void							TransitionResource(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After);

	void							SetViewport(const Rect& inViewportRect);
	Rect							GetViewport() const 							{return mViewportRect;}

	void							SetShaderBindingData(RenderTexture* inTexture);

	void							SetProjectionMatrix(const Matrix4x4& inMatrix);

	void							CopyTexture();

	void							FlushCommands();

	void							Draw();

	// Get the raw ID3D12GraphicsCommandList
	ID3D12GraphicsCommandList*		GetGraphicsCommandList()						{ return GetCommandList()->GetGraphicsCommandList(); }

private:
	RenderCommandList*				GetCommandList();

	void							OpenCommandList();
	void							CloseCommandList();

	void							AddTransitionBarrier(D3D12RenderTargetView* Texture, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After);

	// The active command list.
	RenderCommandList*				mCommandList;

	RenderCommandAllocator*			CommandAllocator;

	RenderDevice*					mDevice;

	ERenderQueueType				QueueType;

	// State for drawing.

	RenderTexture*					RenderTarget;
	Rect							mViewportRect;
	Matrix4x4						mProjectionMatrix;

};

