// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
namespace UOEngine.Runtime.Core;

public class ApplicationLoop
{
    public event Action? OnFrameBegin;

    public event Action<float>? OnUpdate;

    public event Action? OnStartup;

    public event Action? OnExit;

    public bool ExitRequested { get; private set; }

    public void FrameBegin()
    {
        OnFrameBegin?.Invoke();
    }

    public void Start() => OnStartup?.Invoke();

    public void RequestExit(string? reason = null)
    {
        string message = reason ?? "No reason";

        Console.WriteLine($"Exit requested, reason: {message}");

        ExitRequested = true;
    }

    internal void Update(float time)
    {
        OnUpdate?.Invoke(time);
    }
}
