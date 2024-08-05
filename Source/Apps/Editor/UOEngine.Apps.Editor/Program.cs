using UOEngine.Runtime.Core;
using UOEngine.Apps.Client;
using Microsoft.Extensions.DependencyInjection;

namespace UOEngine.Apps.Editor
{
    internal class UOEngineEditor: Program
    {
        static void Main(string[] args)
        {
            var editor = new UOEngineEditor();

            editor.Run(args);
        }

        protected override bool Initialise()
        {
            RegisterService<ClientProgram>();
            RegisterService<UOEngineImGui>();

            return true;
        }

        protected override void OnServicesCreated(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<UOEngineImGui>().Initialise();
        }
}
