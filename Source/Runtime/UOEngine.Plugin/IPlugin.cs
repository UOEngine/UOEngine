using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Plugin;

public interface IPlugin
{
    static void ConfigureServices(IServiceCollection services){}

    void Startup(){}
}
