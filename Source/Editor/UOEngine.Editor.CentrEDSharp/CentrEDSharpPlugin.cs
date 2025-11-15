using CentrED;
using CentrED.Client;
using CentrED.Server;
using CentrED.Utils;

using UOEngine.Runtime.Plugin;

namespace UOEngine.Editor.CentredSharp;

public class CentrEdSharpPlugin: IPlugin
{
    static public string WorkDir { get; } = AppContext.BaseDirectory;

    public static CentrEDGame CEDGame { get; private set; } = null!;
    public static CEDServer? CEDServer;
    public static readonly CentrEDClient CEDClient = new();
    public static readonly Metrics Metrics = new();

    public CentrEdSharpPlugin()
    {

    }

    public void Startup() 
    {
    }

    public void PostStartup() 
    {
        Config.Initialize();

        CEDGame = new CentrEDGame();

        CEDGame.DoInitialise();
    }

    public void Shutdown()
    {
        CEDGame.Dispose();
    }
}
