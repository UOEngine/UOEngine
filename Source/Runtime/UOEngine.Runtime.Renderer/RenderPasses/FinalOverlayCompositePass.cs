// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderPass))]
public sealed class FinalOverlayCompositePass : IRenderPass
{
    public string Name => "FinalOverlayComposite";
    public RenderPassStage Stage => RenderPassStage.BackbufferComposite;
    public int Order => 1;

    public void Execute(IRenderContext context, RenderSystem renderSystem)
    {
        renderSystem.FullscreenPassUtils.BlitTexture(context, renderSystem.UIOverlay.Texture, null);
    }
}
