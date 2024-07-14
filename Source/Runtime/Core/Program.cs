using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

        protected virtual void OnServicesCreated(IServiceProvider serviceProvider)
        {

        }

        protected void Run(string[] args)
        {
            Running = true;

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            serviceCollection = builder.Services;

            Initialise();

            builder.Services.AddSingleton<EventLoop>();

            using IHost host = builder.Build();

            OnServicesCreated(host.Services);

            // ----------------

            var eventLoop = host.Services.GetRequiredService<EventLoop>();
            var tickableServices = new List<ITickable>();

            foreach (var tickableServiceType in tickableServiceTypes)
            {
                var service = host.Services.GetRequiredService(tickableServiceType) as ITickable;

                Debug.Assert(service != null);

                tickableServices.Add(service);
            }

            while (Running)
            {
                float deltaSeconds = 0.0f;

                eventLoop.OnFrameStarted(deltaSeconds);

                foreach(var tickable in tickableServices)
                {
                    tickable.Tick(deltaSeconds);
                }
            }

            Console.WriteLine("Quitting!");
        }

        protected void RegisterService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T: class
        {
            serviceCollection?.AddSingleton<T>();

            if(typeof(ITickable).IsAssignableFrom(typeof(T)))
            {
                tickableServiceTypes.Add(typeof(T));
            }
        }

        public bool                 Running { get; set; }

        private IServiceCollection? serviceCollection;
        private List<Type>          tickableServiceTypes = [];
    }
}
