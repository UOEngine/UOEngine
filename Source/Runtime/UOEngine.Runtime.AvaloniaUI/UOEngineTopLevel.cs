// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls.Embedding;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineTopLevel: EmbeddableControlRoot
{
    public UOEngineTopLevel(UOEngineTopLevelImpl topLevelImpl): base(topLevelImpl)
    {

    }

    public void OnDraw(Rect rect) => PlatformImpl!.Paint?.Invoke(rect);
}
