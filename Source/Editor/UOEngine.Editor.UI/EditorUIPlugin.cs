// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls;
using UOEngine.Editor.Abstractions;
using UOEngine.Runtime.AvaloniaUI;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Editor.UI;

[PluginEntry]
[PluginDependency(typeof(AvaloniaUIPlugin))]
public class EditorUIPlugin: IPlugin
{
    private readonly IRootContentHost _root;
    private readonly IEnumerable<IEditorTool> _editorTools;

    public EditorUIPlugin(IRootContentHost root, IEnumerable<IEditorTool> editorTools)
    {
        _root = root;
        _editorTools = editorTools;
    }

    public void PostStartup() 
    {
        _root.SetMainContent(new EditorMainView
        {
            DataContext = new EditorMainViewModel(_editorTools)
        });
    }

}

