#pragma once

#include <d3d12.h>

#include "Core/TComPtr.h"

struct D3D12Resource
{
	TComPtr<ID3D12Resource>	mResource;
	D3D12_RESOURCE_DESC		mDescription = {};
};