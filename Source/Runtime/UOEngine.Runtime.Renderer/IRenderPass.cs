// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public enum RenderPassStage
{
    Scene = 0,
    OverlayBuild,
    BackbufferComposite,
    Present
}

public interface IRenderPass
{
    string Name { get; }
    RenderPassStage Stage { get; }
    int Order { get; }   // suborder within stage
    void Execute(IRenderContext context, RenderSystem renderSystem);
}
