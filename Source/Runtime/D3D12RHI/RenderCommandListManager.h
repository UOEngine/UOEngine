#pragma once

#include "RenderCommandQueue.h"

class RenderDevice;

class RenderCommandListManager
{
public:

	void						Init(RenderDevice* InDevice);

	void						WaitUntilIdle();

	const RenderCommandQueue&	GetGraphicsQueue() const {return GraphicsQueue;}

private:

	RenderDevice*				Device;

	RenderCommandQueue			GraphicsQueue;
	RenderCommandQueue			CopyQueue;
	RenderCommandQueue			ComputeQueue;
};