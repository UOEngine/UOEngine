#include "Engine.h"

#include <windows.h>

#include "Core/Assert.h"
#include "Core/Containers/String.h"
#include "Core/Timer.h"
#include "Engine/Window.h"
#include "LivePP/LivePP.h"
#include "Memory/MemoryAllocator.h"

#include "RHI/IRHI.h"
#include "D3D12RHI/Renderer.h"

Engine GEngine;

Engine::Engine()
{
	mGameWindow = nullptr;

	mTicksPassed = 0;
}

bool Engine::Init()
{
	PrintDebugString("Engine::Init startup");

	//GRenderer2 = &GRenderer;

	bool wait_for_debugger = false;

	if (wait_for_debugger && (IsDebuggerPresent() == false))
	{
		while (IsDebuggerPresent() == false)
		{
			_mm_pause();
		}
	}

	//LivePP LivePPInstance;

	IPlatformWindow::CreateParameters CreateParameters;

	mGameWindow = IPlatformWindow::Create(CreateParameters);

	InitParameters RendererParameters = {};

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

	const uint32 per_frame_measurement = 144;

	if ((GRenderer.GetNumFramesRendered() % per_frame_measurement) == 0)
	{
		uint64 ticksNow = Timer::GetTicks();

		uint64 delta_ticks = ticksNow - mTicksPassed;

		uint64 delta_ticks_per_frame = (float)delta_ticks / (float)per_frame_measurement;

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
