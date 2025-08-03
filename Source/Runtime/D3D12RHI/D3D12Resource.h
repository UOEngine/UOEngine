#pragma once

#include <d3d12.h>

#include "Core/TComPtr.h"
#include "Core/Containers/String.h"

class D3D12Resource
{
public:

	TComPtr<ID3D12Resource>		Get() {return mResource;}

	void						Release()
								{
									mResource->Release();
								}

	void						SetResource(TComPtr<ID3D12Resource> Resource)
								{
									mResource = Resource;
								}

	D3D12_RESOURCE_DESC			GetDesc() {return mResource->GetDesc();}

	void						SetName(const String& Name)
								{
									mResource->SetName(L"TBD");
								}

private:

	TComPtr<ID3D12Resource>		mResource;
};