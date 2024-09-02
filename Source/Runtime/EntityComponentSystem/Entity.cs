using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public struct Entity
    {
        public ulong        Id;
        public World        World;
        internal Archetype  Archetype;
        public int          ArchetypeRow;

        public Entity(ulong id, World world, Archetype archetype)
        {
            Id = id;
            World = world;
            Archetype = archetype;
        }

        public Entity AddComponent<T>() where T: struct
        {
            return this;
        }

        public Entity Set<T>(T component) where T : struct 
        {
            World.Set(this, component);

            return this; 
        }
    }
}
