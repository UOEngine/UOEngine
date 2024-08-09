using UOEngine.Runtime.Core;
using UOEngine.Runtime.Rendering;
using UOEngine.Apps.Client;
using Microsoft.Extensions.DependencyInjection;

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

            return true;
        }

        protected override void OnServicesCreated(IServiceProvider serviceProvider)
        {
            base.OnServicesCreated(serviceProvider);

            serviceProvider.GetRequiredService<UOEngineImGui>().Initialise();
        }
    }
}
