#pragma once

#include "Core/Math/Vector2D.h"
#include "Core/Types.h"
#include "Renderer/RenderTexture.h"

class RenderCommandContext;
class RenderCommandQueue;
class RenderDevice;
struct IDXGISwapChain1;
struct D3D12_CPU_DESCRIPTOR_HANDLE;

class RenderSwapChain
{
public:

									RenderSwapChain();

	struct InitParameters
	{
		Vector2D			Extents;
		void*				WindowHandle = nullptr;
		RenderDevice*		Device = nullptr;
		uint32				BackBufferCount = 0;
	};

	bool							Init(const InitParameters& Parameters);

	void							Resize(const Vector2D& NewExtents);

	uint32							GetNextPresentIndex() const					{return CurrentBackBufferIndex;}
	RenderTexture*					GetBackBufferTexture(uint32 Index) const	{return &BackBufferTextures[Index]; }

	void							Present(RenderCommandContext* CommandContext);

	void							FlushCommands();

private:

	void							SetBackBufferIndex(uint32 Index)			{ CurrentBackBufferIndex = Index % BackBufferCount; }

	void							CreateBackBufferTextures();

	IDXGISwapChain1*				SwapChain1;

	uint32							BackBufferCount;

	RenderTexture*					BackBufferTextures;

	uint32							CurrentBackBufferIndex;

	Vector2D						Extents;

	RenderDevice*					Device;
};
