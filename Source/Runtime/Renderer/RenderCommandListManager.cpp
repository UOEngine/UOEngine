#include "Renderer/RenderCommandListManager.h"

#include <d3d12.h>

void RenderCommandListManager::Init(RenderDevice* InDevice)
{
	Device = InDevice;

	GraphicsQueue.Create(Device, D3D12_COMMAND_LIST_TYPE_DIRECT);
	CopyQueue.Create(Device, D3D12_COMMAND_LIST_TYPE_COPY);
	ComputeQueue.Create(Device, D3D12_COMMAND_LIST_TYPE_COMPUTE);
}

void RenderCommandListManager::WaitUntilIdle()
{
	GraphicsQueue.WaitUntilIdle();
	CopyQueue.WaitUntilIdle();
	ComputeQueue.WaitUntilIdle();
}
