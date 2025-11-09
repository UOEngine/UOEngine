using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Runtime.Plugin;

public interface IPlugin
{
    static void ConfigureServices(IServiceCollection services){}

    void Startup(){}
    void PostStartup() { }
    void Shutdown() {}
}
