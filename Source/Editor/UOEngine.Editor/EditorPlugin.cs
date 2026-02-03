// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Extensions.DependencyInjection;

using UOEngine.Runtime.Application;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;
using UOEngine.Ultima.UOAssets;

namespace UOEngine.Editor;

internal class UOEngineEditor : UOEngineApplication
{
    public UOEngineEditor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public void OnFrameBegin(IRenderContext context)
    {

    }

}
