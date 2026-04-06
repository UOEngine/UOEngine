using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace UOEngine.Editor.UI;

internal partial class EditorLandingView: UserControl
{
    public EditorLandingView() => AvaloniaXamlLoader.Load(this);
}
