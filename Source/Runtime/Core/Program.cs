using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Thread = System.Threading.Thread;

namespace UOEngine.Runtime.Core
{
    public class Program
    {
        protected virtual bool Loop() { return false; }

        public virtual bool RegisterSubsystems()
        {
            return true; 
        }

        protected virtual bool Initialise()
        {
            return true;
        }

        protected virtual void Shutdown(IServiceProvider serviceProvider)
        {

        }

        protected virtual void OnServicesCreated(IServiceProvider serviceProvider)
        {

        }

        protected void Run(string[] args)
        {
            Running = true;

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            serviceCollection = builder.Services;

            Initialise();

            builder.Services.AddSingleton<GameLoop>();
            builder.Services.AddSingleton<Input>();

            using IHost host = builder.Build();

            foreach (var pluginType in _pluginTypes)
            {
                var plugin = host.Services.GetService(pluginType) as IPlugin;

                plugin!.Initialise(host.Services);
            }

            OnServicesCreated(host.Services);

            // ----------------

            var eventLoop = host.Services.GetRequiredService<GameLoop>();
            var input = host.Services.GetRequiredService<Input>();

            while (eventLoop.IsQuitting == false)
            {
                float deltaSeconds = 0.01f;

                Thread.Sleep((int)(deltaSeconds * 1000.0f));

                input.Tick(deltaSeconds);

                eventLoop.OnFrameStarted(deltaSeconds);
                eventLoop.OnFrameEnded(deltaSeconds);
            }

            Shutdown(host.Services);

            Console.WriteLine("Quitting!");
        }

        protected void RegisterService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T: class
        {
            serviceCollection!.AddSingleton<T>();

            //if(typeof(ITickable).IsAssignableFrom(typeof(T)))
            //{
            //    tickableServiceTypes.Add(typeof(T));
            //}
        }

        protected void RegisterPlugin<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]  T>() where T: IPlugin, new()
        {
            _pluginTypes.Add(typeof(T));

            serviceCollection!.AddSingleton<T>();
        }

        public bool                 Running { get; set; }

        private IServiceCollection? serviceCollection;

        private List<Type>          _pluginTypes = [];
    }
}
