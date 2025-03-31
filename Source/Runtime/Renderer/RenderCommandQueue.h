#pragma once

class RenderDevice;
enum D3D12_COMMAND_LIST_TYPE;
struct ID3D12CommandList;
struct ID3D12CommandQueue;

class RenderCommandQueue
{
public:
								RenderCommandQueue();

	void						Create(RenderDevice* Device, D3D12_COMMAND_LIST_TYPE InType);

	void						WaitUntilIdle();

	void						ExecuteCommandList(ID3D12CommandList* CommandList);

	ID3D12CommandQueue*			GetQueue() const {return CommandQueue;}

private:

	D3D12_COMMAND_LIST_TYPE		Type;
	ID3D12CommandQueue*			CommandQueue;

};
