#pragma once

#include "Core/Types.h"

class RenderCommandAllocator;
enum class ERenderQueueType : uint8;

class RenderCommandList
{
public:

								RenderCommandList(RenderCommandAllocator* CommandAllocator);

	void						Close();

private:

	ERenderQueueType			QueueType;

	RenderCommandAllocator*		CommandAllocator;
};