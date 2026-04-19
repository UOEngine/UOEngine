// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using UOEngine.Runtime.Core;
using UOEngine.Runtime.Plugin;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

internal class HostedGameSession
{
    public bool IsSuspended { get; set; }
    public required IHostedGame HostedGame { get; init; }

}

[Service(UOEServiceLifetime.Singleton, typeof(IHostedGameManager))]
internal class HostedGameManager : IHostedGameManager
{
    private readonly List<HostedGameSession> _hostedGameSessions = [];

    internal IReadOnlyList<HostedGameSession> HostedGameSessions => _hostedGameSessions;

    public HostedGameManager(ApplicationLoop _applicationLoop)
    {
        _applicationLoop.OnUpdate += (float deltaTime) =>
        {
            foreach (var session in _hostedGameSessions)
            {
                if (session.IsSuspended)
                {
                    continue;
                }

                session.HostedGame.Game.Tick1();
            }
        };
    }

    public HostedGameHandle RegisterGame(IHostedGame hostedGame)
    {
        var hostedGameId = new HostedGameHandle
        {
            Id = _hostedGameSessions.Count
        };

        _hostedGameSessions.Add(new HostedGameSession
        {
            HostedGame = hostedGame
        });

        hostedGame.Game.DoInitialise();

        return hostedGameId;
    }

    public void SuspendGame(in HostedGameHandle hostedGameId)
    {
        _hostedGameSessions[hostedGameId.Id].IsSuspended = true;
    }

    public void ResumeGame(in HostedGameHandle hostedGameId)
    {
        _hostedGameSessions[hostedGameId.Id].IsSuspended = false;
    }
}
