
namespace UOEngine.Runtime.Core
{
    public class GameLoop
    {
        public class FrameStartedArgs
        {
            public float DeltaSeconds;
        }

        internal void OnFrameStarted(float  deltaSeconds)
        {
            FrameStarted?.Invoke(this, new FrameStartedArgs { DeltaSeconds = deltaSeconds});
        }

        public event FrameStartedEventHandler? FrameStarted;

        public delegate void FrameStartedEventHandler(object sender, FrameStartedArgs e);
    }
}
