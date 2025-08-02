#include "Engine.h"

#include <windows.h>

#include "Core/Assert.h"
#include "Core/Containers/String.h"
#include "Core/Timer.h"
#include "Engine/Window.h"
#include "LivePP/LivePP.h"
#include "Memory/MemoryAllocator.h"
#include "Renderer/Renderer.h"

Engine GEngine;

Engine::Engine()
{
	mGameWindow = nullptr;

	mTicksPassed = 0;
}

bool Engine::Init()
{
	//LivePP LivePPInstance;

	IPlatformWindow::CreateParameters CreateParameters;

	mGameWindow = IPlatformWindow::Create(CreateParameters);

	Renderer::InitParameters RendererParameters = {};

	RendererParameters.ViewportExtents = mGameWindow->GetExtents();
	RendererParameters.WindowHandle = mGameWindow->GetHandle();

	if (GRenderer.Initialise(RendererParameters) == false)
	{
		GAssert(false);
	}

	mGameWindow->SetVisible(true);

	mTicksPassed = Timer::GetTicks();

	return true;
}

void Engine::Shutdown()
{
	GRenderer.Shutdown();
}

void Engine::PreUpdate()
{
	mGameWindow->PollEvents();

	GRenderer.BeginFrame(mGameWindow->GetExtents());
}

void Engine::PostUpdate()
{
	GRenderer.EndFrame();

	if ((GRenderer.GetNumFramesRendered() % 500) == 0)
	{
		uint64 ticksNow = Timer::GetTicks();

		uint64 delta_ticks = ticksNow - mTicksPassed;

		uint64 delta_ticks_per_frame = (float)delta_ticks / 500.0f;

		double frame_time_ms = Timer::sGetTicksInMilliseconds(delta_ticks_per_frame);

		uint32 fps = 1000.0f / frame_time_ms;

		String frame_time_display = String::sFormat("UOEngine: %.0f ms ( %d fps)", frame_time_ms, fps);

		mGameWindow->SetTitle(frame_time_display.ToCString());

		mTicksPassed = ticksNow;
	}
}

void Engine::Run()
{
	while (EngineGlobals::IsRequestingExit() == false)
	{
		uint64 total_allocation = MemoryAllocator::Get().GetTotalAllocationSizeBytes();

		PreUpdate();
		PostUpdate();

	}
}
