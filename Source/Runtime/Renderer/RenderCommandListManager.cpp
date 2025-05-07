#include "Renderer/RenderCommandListManager.h"

#include <d3d12.h>

void RenderCommandListManager::Init(RenderDevice* InDevice)
{
	Device = InDevice;

}

void RenderCommandListManager::WaitUntilIdle()
{
	GraphicsQueue.WaitUntilIdle();
	CopyQueue.WaitUntilIdle();
	ComputeQueue.WaitUntilIdle();
}
