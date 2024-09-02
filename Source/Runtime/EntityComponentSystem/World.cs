using System.Diagnostics;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public class World
    {
        private Entity[]                         _entities = new Entity[_maxEntities];

        private const int                        _maxEntities = 1024;

        private int                              _numEntities = 0;

        private Dictionary<int, Archetype>       _archetypes = [];
        private int                              _numArchetypes = 0;
        private const int                        _maxArchetypes = 8;

        private Archetype                        _defaultArchetype = new([]);

        private Archetype?                       _componentArchetype;

        public World() 
        {
        }

        public void BuildComponentArchetype(object[] components)
        {

        }

        public ref Entity CreateEntity()
        {
            Debug.Assert(_numEntities <= _maxEntities);

            var newId = _numEntities;

            ref var entity = ref _entities[newId];

            entity = new Entity((ulong)newId, this, _defaultArchetype);

            entity.ArchetypeRow = _defaultArchetype.AddEntity(entity.Id);

            _numEntities++;

            return ref entity;
        }

        private Archetype GetOrCreateArchetype(in Signature signature)
        {
            int hash = (int)signature.ToMask();

            if(_archetypes.TryGetValue(hash, out var archetype))
            {
                return archetype;
            }

            archetype = new Archetype(signature.Components);

            _archetypes.Add(hash, archetype);

            return archetype;
        }

        public void Set<T>(Entity entity, T component) where T: struct
        {
            ComponentType componentType = Component<T>.ComponentType;
            Signature signature = Component<T>.Signature;

            // Find an existing archetype to see if it fits.

            Archetype archetype = GetOrCreateArchetype(signature);

            int row = -1;

            if(archetype.ContainsEntity(entity.Id))
            {
            }
            else
            {
                row = archetype.AddEntity(entity.Id);
            }

            archetype.Set(row, component);
        }
    }
}
