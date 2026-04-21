// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using UOEngine.Runtime.AvaloniaUI;
using UOEngine.Runtime.RHI;

namespace UOEngine.Editor.CentredSharp;

internal partial class CentredSharpView: UserControl
{
    public DrawingSurfaceControl Surface { get; }

    internal CentredSharpView(IRenderResourceFactory resourceFactory)
    {
        Surface = new DrawingSurfaceControl(resourceFactory);
        Content = Surface;
    }
}
