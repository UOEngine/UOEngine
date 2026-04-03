// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.AvaloniaUI;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Editor.UI;

[PluginEntry]
[PluginDependency(typeof(AvaloniaUIPlugin))]
public class EditorUIPlugin: IPlugin
{
    private readonly IRootContentHost _root;

    public EditorUIPlugin(IRootContentHost root)
    {
        _root = root;
    }

    public void Startup() 
    {
        _root.SetMainContent(new EditorMainView());
    }

}
