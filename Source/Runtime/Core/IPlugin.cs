using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace UOEngine.Runtime.Core
{
    public abstract class IPlugin
    {
        abstract public void Initialise(IServiceProvider services);
    }
}
