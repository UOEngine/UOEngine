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

	void			Load(const String& FilePath);

	void			Compile(const String& ShaderCode);


private:

	EShaderType		Type;

};