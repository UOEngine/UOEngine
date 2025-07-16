using System.Diagnostics;
using System.Threading.Tasks;
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
            var startupMeasurement = Stopwatch.StartNew();

            var preEngineInitTask = Task.Run(() => _app.PreEngineInit());

            int code = EngineInterop.EngineInit();

            if (code != 0)
            {
                return code;
            }

            preEngineInitTask.Wait();

            if (_app.Initialise() == false)
            {
                return 1;
            }

            startupMeasurement.Stop();

            Console.WriteLine($"Startup time: {startupMeasurement.ElapsedMilliseconds} ms");

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
