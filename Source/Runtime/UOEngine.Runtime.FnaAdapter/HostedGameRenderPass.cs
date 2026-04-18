// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

[Service(UOEServiceLifetime.Singleton, typeof(IRenderPass))]
internal class HostedGameRenderPass : IRenderPass
{
    public string Name => "FNA Hosted Game";

    public RenderPassStage Stage => RenderPassStage.Scene;

    public int Order => 0;

    RenderPassBuilder IRenderPass.Rpb => new();

    private readonly HostedGameManager _hostedGameManager;

    public HostedGameRenderPass(IHostedGameManager hostedGameManager)
    {
        _hostedGameManager = (HostedGameManager)hostedGameManager;
    }

    public void Execute(IRenderContext context, RenderSystem renderSystem)
    {
        foreach (var session in _hostedGameManager.HostedGameSessions)
        {
            if (session.IsSuspended)
            {
                continue;
            }

            var target = session.HostedGame.Surface.AcquireRenderTarget();

            context.BeginRenderPass(new RenderPassInfo
            {
                Name = "FNAPass",
                RenderTarget = target,
                LoadAction = RhiRenderTargetLoadAction.Clear
            });

            session.HostedGame.Game.GraphicsDevice.OnFrameBegin(context);
            session.HostedGame.Game.Tick2();

            context.EndRenderPass();

            session.HostedGame.Surface.Present(context);
        }
    }
}
