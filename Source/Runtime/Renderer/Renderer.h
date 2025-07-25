#pragma once

#include "Core/Math/Vector2D.h"
#include "Core/Types.h"

class RenderCommandListManager;
class RenderCommandContext;
class RenderDevice;
class RenderSwapChain;
class RenderTexture;

class Renderer
{
public:

								Renderer();

	struct InitParameters
	{
		void*		WindowHandle = nullptr;
		IntVector2D	ViewportExtents;
	};

	bool						Initialise(const InitParameters& Parameters);
	void						Shutdown();

	void						BeginFrame(const IntVector2D& ViewportExtents);
	void						EndFrame();

	RenderCommandContext*		GetContext() const	{return CommandContext;}

	RenderDevice*				GetDevice() const	{return mDevice;}

private:

	void						CreateDefaultPipelineState();
	RenderTexture*				CreateTestTexture(uint32 inCheckerboardColour);

	int32						NumFramesRendered;

	bool						bInitialised;

	RenderDevice*				mDevice;

	RenderCommandContext*		CommandContext;

	RenderSwapChain*			Viewport;

	RenderTexture*				mDefaultBlackCheckerboardTexture;
	RenderTexture*				mDefaultRedCheckerboardTexture;
};

extern Renderer GRenderer;