namespace UOEngine.Runtime.Core;

public class ApplicationLoop
{
    public event Action? OnFrameBegin;

    public event Action<float>? OnUpdate;

    public void FrameBegin()
    {
        OnFrameBegin?.Invoke();
    }

    internal void Update(float time)
    {
        OnUpdate?.Invoke(time);
    }
}
