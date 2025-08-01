#pragma once

#include "Core/Math/Vector2D.h"
#include "Core/Types.h"

class RenderCommandListManager;
class RenderCommandContext;
class RenderBuffer;
class RenderDevice;
class RenderSwapChain;
class RenderTexture;

class IRenderingApiInterface
{
public:

	//virtual RenderDevice*	GetDevice() = 0;
};

class Renderer: private IRenderingApiInterface
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

	RenderBuffer*				mTestModelBuffer;
};

extern Renderer GRenderer;