#pragma once

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

class RenderCommandContext
{
public:

								RenderCommandContext(RenderDevice* InDevice);

	void						Begin();
	void						End();

	void						BeginRenderPass();
	void						EndRenderPass();

	void						SetRenderTarget(D3D12RenderTargetView* View);

private:
	RenderCommandList*			GetCommandList();

	void						OpenCommandList();
	void						CloseCommandList();

	void						AddTransitionBarrier(D3D12RenderTargetView* Texture, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After);

	ID3D12GraphicsCommandList*	GetGraphicsCommandList() {return GetCommandList()->GetGraphicsCommandList();}

	// The active command list.
	RenderCommandList*			CommandList;

	RenderCommandAllocator*		CommandAllocator;

	RenderDevice*				Device;

	ERenderQueueType			QueueType;

	// State for drawing.

	D3D12RenderTargetView*			RenderTarget;

};

