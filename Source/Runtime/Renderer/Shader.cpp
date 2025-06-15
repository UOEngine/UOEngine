#include "Renderer/Shader.h"

#include "Core/FileReader.h"
//#include <dxcapi.h>
//#include <d3d12shader.h>

Shader::Shader(EShaderType InType)
{
	Type = InType;
}

void Shader::Load(const String& FilePath)
{
	FileHandle* File = nullptr;

	if (FileDevice::Open(FilePath, File) == false)
	{
		GAssert(false);

		return;
	}
	
	TArray<uint8> Contents;

	Contents.SetNum(File->GetSize());

	FileDevice::Read(File, Contents.GetData());

}
