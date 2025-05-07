#pragma once

class RenderCommandAllocator
{
public:

	RenderCommandAllocator(ERenderQueueType InQeueType);

	ERenderQueueType GetQueueType() const {return QueueType;}

private:

	ERenderQueueType QueueType;
};