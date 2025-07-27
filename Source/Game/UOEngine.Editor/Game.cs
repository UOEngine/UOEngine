using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UOEngine.Interop;

namespace UOEngine;

public class Engine(IUOEngineApp app) : IDisposable
{
    readonly IUOEngineApp _app = app;

    public int Run(string[] args)
    {
        int code = -1;

        try
        {
            code = RunInternal(args);
        }
        catch(SEHException ex)
        {
            IntPtr exceptionPointers = Marshal.GetExceptionPointers();

            Debug.Assert(false);
        }
        catch (Exception ex)
        {
            Debug.Assert(false);
        }
        finally
        {
            Dispose();
        }

        return code;
    }

    public void Dispose()
    {
        EngineInterop.EngineShutdown();
    }

    protected virtual void Update(float deltaTime) { }

    protected virtual bool Initialise()
    {
        return true;
    }

    private int RunInternal(string[] args)
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


        return 0;
    }
}
