#pragma once

struct RenderFence
{
	RenderFence(ID3D12Fence* InFence)
	{
		Fence = InFence;

		Event = ::CreateEvent(NULL, FALSE, FALSE, NULL);
	}

	void			Init()
	{

	}

	void			Signal();

	void			Wait()
	{
		//if (Fence->GetCompletedValue() <= 0)
		//{
		//	Fence->SetEventOnCompletion()
		//}
	}

	ID3D12Fence*	Fence = nullptr;
	HANDLE			Event;
};