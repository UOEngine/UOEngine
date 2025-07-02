#pragma once

#include "Core/Types.h"

class RenderCommandAllocator;
enum D3D12_RESOURCE_STATES;
enum class ERenderQueueType : uint8;
struct ID3D12GraphicsCommandList;
struct ID3D12Resource;

class RenderCommandList
{
public:

								RenderCommandList(RenderCommandAllocator* CommandAllocator);

	void						Close();

	void						AddTransitionBarrier(ID3D12Resource* Resource, D3D12_RESOURCE_STATES Before, D3D12_RESOURCE_STATES After);

	void						ClearRenderTarget(ID3D12Resource* Resource);

	bool						IsOpen() const					{return bClosed == false;}
	bool						IsClosed() const				{return bClosed;}

	void						Reset();

	ID3D12GraphicsCommandList*	GetGraphicsCommandList() 	{return CommandList;}

	RenderCommandAllocator*		GetCommandAllocator() const {return CommandAllocator;}

	void						CopyTextureRegion();

private:

	ERenderQueueType			QueueType;

	RenderCommandAllocator*		CommandAllocator;

	ID3D12GraphicsCommandList*	CommandList;

	bool						bClosed;
};