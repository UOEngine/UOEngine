#include "Engine.h"

#include "Core/Assert.h"
#include "DotNet/DotNet.h"
#include "Engine/EngineGlobals.h"
#include "Engine/Window.h"
#include "LivePP/LivePP.h"
#include "Renderer/Renderer.h"

Engine GEngine;

Engine::Engine()
{
}

bool Engine::Init()
{
	LivePP LivePPInstance;

	IPlatformWindow::CreateParameters CreateParameters;

	GameWindow = IPlatformWindow::Create(CreateParameters);

	Renderer::InitParameters RendererParameters = {};

	RendererParameters.ViewportExtents = GameWindow->GetExtents();
	RendererParameters.WindowHandle = GameWindow->GetHandle();

	if (GRenderer.Initialise(RendererParameters) == false)
	{
		GAssert(false);
	}

	GameWindow->SetVisible(true);

	if (DotNet::sGet().Init() == false)
	{
		GAssert(false);
	}

	return true;

}

void Engine::Run()
{
	while (EngineGlobals::IsRequestingExit() == false)
	{
		GameWindow->PollEvents();

		DotNet::sGet().ManagedUpdate(0.0f);

		GRenderer.RenderFrame(GameWindow->GetExtents());
	}

	GRenderer.Shutdown();
}