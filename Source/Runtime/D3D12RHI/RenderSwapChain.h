#pragma once

#include "Core/Containers/Array.h"
#include "Core/Math/Vector2D.h"
#include "Core/Types.h"
#include "RenderTexture.h"

class RenderCommandContext;
class RenderCommandQueue;
class RenderDevice;
enum DXGI_FORMAT;
struct IDXGISwapChain3;
struct D3D12_CPU_DESCRIPTOR_HANDLE;

class RenderSwapChain
{
public:

									RenderSwapChain();

	struct InitParameters
	{
		IntVector2D			Extents;
		void*				WindowHandle = nullptr;
		RenderDevice*		Device = nullptr;
		uint32				BackBufferCount = 0;
	};

	bool							Init(const InitParameters& Parameters);
	void							Shutdown();

	void							Resize(const IntVector2D& NewExtents);

	IntVector2D						GetExtents() const							{return mExtents;}

	DXGI_FORMAT						GetFormat() const							{return Format;}

	RenderTexture*					GetBackBuffer() const						{return mBackBufferTextures[mCurrentBackBufferIndex]; }

	uint32							GetNextPresentIndex() const					{return mCurrentBackBufferIndex;}
	RenderTexture*					GetBackBufferTexture(uint32 Index) const	{return mBackBufferTextures[Index]; }

	void							Present(RenderCommandContext* CommandContext);

	void							FlushCommands();

private:

	void							CreateBackBufferTextures();

	IDXGISwapChain3*				SwapChain;

	uint32							mBackBufferCount;

	TArray<RenderTexture*>			mBackBufferTextures;

	uint32							mCurrentBackBufferIndex;

	IntVector2D						mExtents;

	RenderDevice*					mDevice;

	DXGI_FORMAT						Format;

};
