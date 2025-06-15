#pragma once

#include "Core/Containers/String.h"

class FileHandle
{
public:
	FileHandle(void* InHandle, uint64 InSize, const String& InFilename)
	{
		Handle = InHandle;
		SizeInBytes = InSize;
		Filename = InFilename;
	}

	void* GetHandle() const {return Handle;}
	uint64 GetSize() const {return SizeInBytes;}

private:

	void*	Handle = nullptr;
	uint64	SizeInBytes = 0;
	String	Filename;

};

class FileDevice
{
public:

	static bool				Open(const String& FilePath, FileHandle*& OutFileHandle);

	static bool				Read(const FileHandle* Handle , uint8* OutBuffer);

};