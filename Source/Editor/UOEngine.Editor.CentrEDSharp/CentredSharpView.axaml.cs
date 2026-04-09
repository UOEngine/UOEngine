// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UOEngine.Editor.CentredSharp;

internal partial class CentredSharpView: UserControl
{
    internal CentredSharpView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
