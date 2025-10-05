using System.Threading.Tasks;

using UOEngine.Plugin;

namespace UOEngine.Server;

public class ServerPlugin : IPlugin
{
    public void Startup()
    {
    }

    public async Task StartServerAsync(string[] args)
    {
        //Task serverTask = Task.Run(() => Server.Core.Main(args));

        //await serverTask;
    }
}
