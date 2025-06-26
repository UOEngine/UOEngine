#include "Core/FileReader.h"

#include <windows.h>

bool FileDevice::Open(const String& FilePath, FileHandle*& OutFileHandle)
{
	HANDLE File = CreateFileA(FilePath.ToCString(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);

	if (File == INVALID_HANDLE_VALUE)
	{
		return false;
	}

	DWORD FileSize = GetFileSize(File, nullptr);

	OutFileHandle = new FileHandle(File, FileSize, FilePath);

	return true;
}

void FileDevice::Close(FileHandle* Handle)
{
	CloseHandle(Handle->GetHandle());
}

bool FileDevice::Read(const FileHandle* Handle, uint8* OutBuffer)
{
	DWORD NumberOfBytesRead = 0;

	if (ReadFile(Handle->GetHandle(), OutBuffer, Handle->GetSize(), nullptr, nullptr) == false)
	{
		return false;
	}

	return true;
}
