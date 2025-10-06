namespace UOEngine.Runtime;

public class ApplicationLoop
{
    public event Action<TimeSpan>? OnUpdate;

    internal void Update(TimeSpan time)
    {
        OnUpdate?.Invoke(time);
    }
}
