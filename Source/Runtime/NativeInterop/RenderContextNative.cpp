#include "NativeInterop/RenderContextNative.h"

#include "Core/Assert.h"
#include "Renderer/Renderer.h"
#include "Renderer/RenderContext.h"
#include "Renderer/RenderTexture.h"

void SetShaderBindingData(RenderTexture* inTexture)
{
	//GRenderer.GetContext()->SetShaderBindingData(inTexture);
}

void Draw(uint32 inNumInstances)
{
	GRenderer.GetContext()->Draw(inNumInstances);
}

ShaderInstance* GetShaderInstance()
{
	return GRenderer.GetContext()->GetShaderInstance();
}

void SetBindlessTextures(RenderTexture** inTextures, uint32 inNum)
{
	TArray<RenderTexture*> textures;

	textures.SetNum(inNum);

	for (int32 i = 0; i < inNum; i++)
	{
		textures[i] = inTextures[i];

		GAssert(textures[i]->GetDescription().Width == 44);
		GAssert(textures[i]->GetDescription().Height == 44);

		GAssert(inTextures[i]->GetSrv().IsValid());

		//PrintDebugString("T_Test_%d", i);

		//PrintDebugString("Texture %s", textures[i]->GetName().ToCString());

	}

	GRenderer.GetContext()->SetBindlessTextures(textures);
}
