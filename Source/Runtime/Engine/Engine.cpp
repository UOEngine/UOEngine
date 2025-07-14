#include "Engine.h"

#include <windows.h>

#include "Core/Assert.h"
#include "Engine/Window.h"
#include "LivePP/LivePP.h"
#include "Memory/MemoryAllocator.h"
#include "Renderer/Renderer.h"

Engine GEngine;

Engine::Engine()
{
	mGameWindow = nullptr;
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
