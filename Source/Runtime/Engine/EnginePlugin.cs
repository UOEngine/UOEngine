using Engine.Components;
using UOEngine.Engine.Components;
using UOEngine.Runtime.Core;

using UOEngine.Runtime.EntityComponentSystem;

namespace UOEngine.Runtime.Engine
{
    public class EnginePlugin : IPlugin
    {
        public override void Initialise(IServiceProvider services)
        {
            var world = new World();

            var entity = world.CreateEntity("TestEntity");

            entity.Add<TransformComponent>(new TransformComponent { Position = { X = 1.0f, Y = 0.1f } });

            entity.Add<SpriteComponent>();

            //entity.Set(new TransformComponent { Position = { X = 1.5f, Y = 0.5f } });
        }
    }
}
