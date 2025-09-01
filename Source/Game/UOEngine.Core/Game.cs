using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UOEngine.Core;
using UOEngine.Interop;

namespace UOEngine;

public class Engine(IUOEngineApp app) : IDisposable
{
    readonly IUOEngineApp _app = app;

    public CameraEntity? ActiveCamera = null;

    [DllImport("kernel32.dll")]
    static extern IntPtr SetUnhandledExceptionFilter(UnhandledExceptionFilterDelegate lpTopLevelExceptionFilter);

    delegate int UnhandledExceptionFilterDelegate(IntPtr exceptionPointers);

    public int Run(string[] args)
    {
        int code = -1;

        SetUnhandledExceptionFilter(CrashHandler);

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("AppDomain.CurrentDomain.UnhandledException", e.ExceptionObject as Exception);
        };

        bool waitForDebugger = false;

        if (waitForDebugger && (Debugger.IsAttached == false))
        {
            Console.WriteLine("Waiting for debugger to attach ...");

            while (Debugger.IsAttached == false)
            {
                Thread.Sleep(0);
            }
        }

        try
        {
            code = RunInternal(args);
        }
        catch(SEHException)
        {
            IntPtr exceptionPointers = Marshal.GetExceptionPointers();

            Debug.Assert(false);
        }
        catch (Exception) when (Debugger.IsAttached == false)
        {
            Console.WriteLine("Uh oh");

            Debug.Assert(false);
        }
        finally
        {
            Dispose();
        }

        Console.WriteLine("Exiting...");

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

        Stopwatch frameTime = new Stopwatch();
        
        while (EngineInterop.EnginePreUpdate() == 1)
        {
            float deltaTimeSeconds = (float)frameTime.Elapsed.TotalSeconds;

            frameTime.Restart();

            _app.Update(deltaTimeSeconds);

            EngineInterop.EnginePostUpdate();
        }


        return 0;
    }

    static private int CrashHandler(IntPtr exceptionPointers)
    {
        Console.WriteLine("Native crash happened.");

        return 0;
    }
}
