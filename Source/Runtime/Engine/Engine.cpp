#include "Engine.h"

#include "Core/Assert.h"
#include "Engine/Window.h"
#include "LivePP/LivePP.h"
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
}

void Engine::PostUpdate()
{
	GRenderer.RenderFrame(mGameWindow->GetExtents());
}
