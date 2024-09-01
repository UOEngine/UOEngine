
namespace UOEngine.Runtime.Core
{
    public class GameLoop
    {
        public Action<float>? FrameStarted;

        public Action<float>? FrameEnded;

        public bool IsQuitting { get; set; } = false;

        internal void OnFrameStarted(float  deltaSeconds)
        {
            FrameStarted?.Invoke(deltaSeconds);
        }

        internal void OnFrameEnded(float deltaSeconds)
        {
            FrameEnded?.Invoke(deltaSeconds);
        }
    }
}
