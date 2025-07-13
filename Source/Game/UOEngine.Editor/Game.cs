using System.Diagnostics;
using System.Runtime.InteropServices;
using UOEngine.Interop;

namespace UOEngine
{
    internal class Engine
    {
        public Engine(IUOEngineApp app) 
        {
            _app = app;
        }

        public int Run(string[] args)
        {
            int code = EngineInterop.EngineInit();

            if (code != 0)
            {
                return code;
            }

            if (_app.Initialise() == false)
            {
                return 1;
            }

            while (EngineInterop.EnginePreUpdate() == 1)
            {
                float deltaTime = 0.0f;

                _app.Update(deltaTime);

                EngineInterop.EnginePostUpdate();
            }

            EngineInterop.EngineShutdown();

            return 0;
        }

        protected virtual void Update(float deltaTime) { }

        protected virtual bool Initialise()
        {
            return true;
        }

        IUOEngineApp _app;
    }
}
