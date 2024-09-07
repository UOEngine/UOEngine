
namespace UOEngine.Runtime.EntityComponentSystem
{
    public struct Entity
    {
        public ulong        Id;
        public World        World;
        public string       Name;

        public Entity(ulong id, World world, string name)
        {
            Id = id;
            World = world;
            Name = name;
        }

        public Entity Add<T>(in T? component = default) where T: struct
        {
            World.Add(this, component);

            return this;
        }

        //public Entity Set<T>() where T: struct
        //{
        //    World.Set<T>(this);

        //    return this;
        //}

        public Entity Set<T>(T component) where T : struct 
        {
            World.Set(this, component);

            return this; 
        }
    }
}
