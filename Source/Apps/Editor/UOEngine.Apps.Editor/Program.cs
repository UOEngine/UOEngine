using Microsoft.Extensions.DependencyInjection;

using UOEngine.Apps.Client;
using UOEngine.Runtime.Engine;

namespace UOEngine.Apps.Editor
{
    internal class UOEngineEditor : ClientProgram
    {
        static void Main(string[] args)
        {
            var editor = new UOEngineEditor();

            editor.Run(args);
        }

        protected override bool Initialise()
        {
            base.Initialise();

            RegisterService<UOEngineImGui>();
            RegisterPlugin<EnginePlugin>();

            return true;
        }

        protected override void OnServicesCreated(IServiceProvider serviceProvider)
        {
            base.OnServicesCreated(serviceProvider);

            serviceProvider.GetRequiredService<UOEngineImGui>().Initialise();
        }
    }
}
