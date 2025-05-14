#include "Engine.h"

#include "Core/Assert.h"
#include "Engine/Window.h"
#include "Renderer/Renderer.h"

Engine GEngine;

Engine::Engine()
{
	bExitRequested = false;
}

bool Engine::Init()
{
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

	return true;

}

void Engine::Run()
{
	while (bExitRequested == false)
	{
		GameWindow->PollEvents();

		GRenderer.RenderFrame(GameWindow->GetExtents());
	}
}