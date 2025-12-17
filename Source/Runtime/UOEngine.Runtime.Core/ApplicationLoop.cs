namespace UOEngine.Runtime.Core;

public class ApplicationLoop
{
    public event Action? OnFrameBegin;

    public event Action<float>? OnUpdate;

    public bool ExitRequested { get; private set; }

    public void FrameBegin()
    {
        OnFrameBegin?.Invoke();
    }

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
