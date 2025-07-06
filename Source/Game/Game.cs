using System.Diagnostics;
using System.Runtime.InteropServices;

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
            int code = NativeMethods.EngineInit();

            if (code != 0)
            {
                return code;
            }

            if (_app.Initialise() == false)
            {
                return 1;
            }

            while (NativeMethods.EnginePreUpdate() == 1)
            {
                float deltaTime = 0.0f;

                _app.Update(deltaTime);

                NativeMethods.EnginePostUpdate();
            }

            NativeMethods.EngineShutdown();

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
