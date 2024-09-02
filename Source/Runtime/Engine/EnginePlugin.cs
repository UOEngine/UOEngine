using Engine.Components;
using UOEngine.Runtime.Core;

using UOEngine.Runtime.EntityComponentSystem;

namespace UOEngine.Runtime.Engine
{
    public class EnginePlugin : IPlugin
    {
        public override void Initialise(IServiceProvider services)
        {
            var world = new World();

            world.CreateEntity().Set(new TransformComponent { Position = { X = 1.0f, Y = 0.1f } });
        }
    }
}
