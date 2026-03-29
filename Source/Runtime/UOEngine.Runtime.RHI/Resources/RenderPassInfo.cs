// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.RHI;

public enum RhiRenderTargetLoadAction
{
    NoAction,
    Load,
    Clear,
    DontCare
}

public struct RenderPassInfo
{
    public required RhiRenderTarget? RenderTarget;
    public string Name;
    public required RhiRenderTargetLoadAction LoadAction;
    public Colour ClearColour;
}
