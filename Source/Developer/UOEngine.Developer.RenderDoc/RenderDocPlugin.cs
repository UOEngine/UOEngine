// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;

namespace UOEngine.Developer.RenderDoc;

[PluginEntry]
[PluginLoadingPhase(PluginLoadingPhase.DoNotLoadAutomatically)]
public class RenderDocPlugin: IPlugin
{
    private RenderDoc _renderDoc = new();

    public void Startup() 
    {
        if(CommandLine.HasOption("-renderdoc") == false)
        {
            return;
        }

        _renderDoc.TryLoad();
    }

}
