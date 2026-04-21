// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

internal sealed class FnaGameContext
{
    public required IServiceProvider Services { get; init; }
    public required IHostedGameHost Host { get; init; }
    public required IRenderResourceFactory RenderResourceFactory { get; init; }
    //public required IHostedGameInputSource Input { get; init; }
}
