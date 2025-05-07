#pragma once

#include "Core/Types.h"

class RenderDevice;
enum class ERenderQueueType: uint8;
struct ID3D12CommandAllocator;

class RenderCommandAllocator
{
public:

							RenderCommandAllocator(RenderDevice* Device, ERenderQueueType InQeueType);

	ERenderQueueType		GetQueueType() const {return QueueType;}

	RenderDevice*			GetDevice() const {return Device;}

	ID3D12CommandAllocator*	GetHandle() const {return CommandAllocator;}

private:

	ERenderQueueType		QueueType;

	ID3D12CommandAllocator* CommandAllocator;

	RenderDevice*			Device;

};