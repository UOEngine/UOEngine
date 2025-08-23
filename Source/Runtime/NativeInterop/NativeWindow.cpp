#include "NativeInterop/NativeWindow.h"

#include "Core/Assert.h"
#include "Engine/Engine.h"
#include "Engine/Window.h"

IntVector2DNative GetExtents()
{
	GAssert(GEngine.GetWindow() != nullptr);

	IntVector2D extents = GEngine.GetWindow()->GetExtents();

	return {extents.X, extents.Y};
}
