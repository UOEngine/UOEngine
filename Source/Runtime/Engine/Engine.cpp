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
	//Thread::CreationParameters ThreadCreationParameters;

	//ThreadCreationParameters.Function = &Engine::MessageThread;

	//WindowMessageThread = Thread::Create(ThreadCreationParameters);

	IPlatformWindow::CreateParameters CreateParameters;

	GameWindow = IPlatformWindow::Create(CreateParameters);

	if (GRenderer.Initialise() == false)
	{
		GAssert(false);
	}

	return true;

}

void Engine::Run()
{
	while (bExitRequested == false)
	{
		GameWindow->PollEvents();
	}
}