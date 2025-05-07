#pragma once

#include "Core/Types.h"

class RenderCommandAllocator;
enum class ERenderQueueType : uint8;
struct ID3D12GraphicsCommandList;

class RenderCommandList
{
public:

								RenderCommandList(RenderCommandAllocator* CommandAllocator);

	void						Close();

private:

	ERenderQueueType			QueueType;

	RenderCommandAllocator*		CommandAllocator;

	ID3D12GraphicsCommandList*	CommandList;
};