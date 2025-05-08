#pragma once

#include "Renderer/DescriptorAllocator.h"

struct ID3D12Resource;

class D3D12RenderTargetView
{
public:

	//D3D12RenderTargetView()

	ID3D12Resource*	GetResource() const { return nullptr;}

	DescriptorHandleCPU GetDescriptorHandleCPU() const {return HandleCPU;}

private:

	DescriptorHandleCPU	HandleCPU;
	
};