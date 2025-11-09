using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Plugin;

public interface IPlugin
{
    // Default empty implementation so no need to have to implement explicitly if not needed.
    static void ConfigureServices(IServiceCollection services){}

    public void Startup(){}
    public void PostStartup() { }
    public void Shutdown() {}
}
