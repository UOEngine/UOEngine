#pragma once

#include "Core/Assert.h"
#include "Core/Types.h"

template<class T> 
class TComPtr
{
public:

	TComPtr() {mPtr = nullptr;}

	TComPtr(T* Ptr)
	{
		mPtr = Ptr;

		mPtr->AddRef();
	}

	TComPtr(const TComPtr<T>& Other)
	{
		mPtr = Other.mPtr;
		mPtr->AddRef();
	}

							~TComPtr()
							{
								Release();
							}

	void					Release()
							{
								if (mPtr != nullptr)
								{
									mPtr->Release();

									mPtr = nullptr;
								}
							}

							operator T*() const	{return mPtr; }
	T**						operator&()			{ GAssert(mPtr == nullptr); return &mPtr;}
	T*						operator->()		{ GAssert(mPtr != nullptr); return mPtr;}

	T*						operator=(T* ptr)
							{
								GAssert(false);
								if(ptr != mPtr)
								{
									mPtr = ptr;
									mPtr->AddRef();
								}

								return *this;
							}

	T*						operator=(const TComPtr<T>& Other)
							{
								if(Other != mPtr)
								{
									mPtr = Other.mPtr;
									mPtr->AddRef();
								}

								return *this;
							}

	using HRESULT = int32;
	template <class Q>
	HRESULT					QueryInterface(Q** outPtr) const 
							{	
								GAssert(outPtr != nullptr && mPtr != nullptr); 
								return mPtr->QueryInterface(__uuidof(Q), (void**)outPtr); 
							}

private:

	T*	mPtr;
};