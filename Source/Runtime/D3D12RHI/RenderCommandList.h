#pragma once

#include "Core/Types.h"

class RenderCommandAllocator;
class RenderDevice;
enum D3D12_RESOURCE_STATES;
enum class ERenderQueueType : uint8;
struct ID3D12GraphicsCommandList;
struct ID3D12Resource;

class RenderCommandList
{
public:

								RenderCommandList(ERenderQueueType inQueueType, RenderDevice* inDevice);

	void						Close();

	void						AddTransitionBarrier(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After);

	void						ClearRenderTarget(ID3D12Resource* Resource);

	bool						IsOpen() const											{return bClosed == false;}
	bool						IsClosed() const										{return bClosed;}

	void						Reset(RenderCommandAllocator* inCommandAllocator);

	ID3D12GraphicsCommandList*	GetGraphicsCommandList() 								{return mCommandList;}

	RenderCommandAllocator*		GetAndClearCommandAllocator();

	void						CopyTextureRegion();

private:

	ERenderQueueType			mQueueType;

	RenderCommandAllocator*		mCommandAllocator;

	ID3D12GraphicsCommandList*	mCommandList;

	bool						bClosed;
};