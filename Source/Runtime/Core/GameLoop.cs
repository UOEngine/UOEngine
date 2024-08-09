
namespace UOEngine.Runtime.Core
{
    public class GameLoop
    {
        internal void OnFrameStarted(float  deltaSeconds)
        {
            FrameStarted?.Invoke(deltaSeconds);
        }

        internal void OnFrameEnded(float deltaSeconds)
        {
            FrameEnded?.Invoke(deltaSeconds);
        }

        public event FrameStartedEventHandler?  FrameStarted;
        public delegate void                    FrameStartedEventHandler(float deltaSeconds);

        public event FrameStartedEventHandler?  FrameEnded;
        public delegate void                    FrameEndedEventHandler(float deltaSeconds);

    }
}
