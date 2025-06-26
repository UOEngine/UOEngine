#pragma once

#include "Core/Containers/String.h"
#include "Core/Types.h"

enum class EShaderType : uint8
{
	Vertex,
	Pixel,
	Compute,
	Invalid
};

class Shader
{
public:

					Shader(EShaderType InType);

	bool			Load(const String& FilePath);

	uint8*			GetBytecode()			const {return Dxil.GetData();}

	uint32			GetBytecodeLength()		const {return Dxil.Num();}

private:

	EShaderType		Type;

	TArray<uint8>	Dxil;

};