#include "NativeInterop/RenderContextNative.h"

#include "Renderer/Renderer.h"
#include "Renderer/RenderContext.h"

void SetShaderBindingData(RenderTexture* inTexture)
{
	//GRenderer.GetContext()->SetShaderBindingData(inTexture);
}

void Draw()
{
	GRenderer.GetContext()->Draw();
}

ShaderInstance* GetShaderInstance()
{
	return GRenderer.GetContext()->GetShaderInstance();
}
