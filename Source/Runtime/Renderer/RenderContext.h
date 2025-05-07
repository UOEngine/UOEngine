#pragma once

#include "Core/Types.h"

class ID3D12GraphicsCommandList;
class RenderCommandAllocator;
class RenderCommandList;
class RenderDevice;
class RenderTexture;
enum class ERenderQueueType: uint8;

class RenderCommandContext
{
public:

								RenderCommandContext(RenderDevice* InDevice);

	void						Begin();
	void						End();

	void						BeginRenderPass();
	void						EndRenderPass();

	void						SetRenderTarget(RenderTexture* Texture);

private:
	RenderCommandList*			GetCommandList();

	void						OpenCommandList();
	void						CloseCommandList();

	// The active command list.
	RenderCommandList*			CommandList;

	RenderCommandAllocator*		CommandAllocator;

	RenderDevice*				Device;

	ERenderQueueType			QueueType;

	RenderTexture*				RenderTarget;

};

