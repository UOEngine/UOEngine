using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.AccessControl;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public class World
    {
        private Entity[]                                            _entities = new Entity[_maxEntities];

        private const int                                           _maxEntities = 1024;

        private int                                                 _numEntities = 0;

        private Dictionary<ulong, Archetype>                        _archetypes = [];
        private int                                                 _numArchetypesCreated = 0;

        private readonly Archetype                                  _defaultArchetype;

        private Dictionary<Entity, EntityRecord>                    _entityRecords = [];
        private Dictionary<ulong, Dictionary<ulong, ArchetypeRecord>>   _componentIndices = []; // component_index

        public World() 
        {
            _defaultArchetype = new Archetype([], EntityPool.Get());

            CreateEntity(_defaultArchetype.Id, "DefaultArchetype");
        }

        public ref Entity CreateEntity()
        {
            var id = EntityPool.Get();

            return ref CreateEntity(id);
        }

        public ref Entity CreateEntity(string name)
        {
            var id = EntityPool.Get();

            return ref CreateEntity(id, name);
        }

        public ref Entity CreateEntity(ulong id, string name = "")
        {
            Debug.Assert(_numEntities <= _maxEntities);

            ref var entity = ref _entities[id];

            entity = new Entity(id, this, name);

            int row = _defaultArchetype.AddEntity(entity);

            _entityRecords.Add(entity, new EntityRecord(_defaultArchetype, row));

            _numEntities++;

            return ref entity;
        }

        //public bool HasComponent<T>(Entity entity) where T : struct
        //{
        //    ComponentType componentType = Component<T>.ComponentType;

        //    Archetype archetype = _entityRecords[entity].Archetype;

        //    return _componentIndices[componentType].ContainsKey(archetype.Id);
        //}

        //public T? GetComponent<T>(Entity entity) where T: struct
        //{
        //    if(HasComponent<T>(entity) == false)
        //    {
        //        return null;
        //    }

        //    ComponentType componentType = Component<T>.ComponentType;

        //    EntityRecord entityRecord = _entityRecords[entity];

        //    Archetype archetype = entityRecord.Archetype;

        //    var archetypes = _componentIndices[componentType];

        //    ArchetypeRecord archetypeRecord = archetypes[archetype.Id];

        //    return archetype.Get<T>(archetypeRecord.Column, entityRecord.Row);
        //}

        public void Add<T>(Entity entity, in T component = default) where T: struct
        {
            ComponentType componentType = Component<T>.ComponentType;

            EntityRecord record = _entityRecords[entity];
            Archetype oldArchetype = record.Archetype;

            Archetype newArchetype = GetOrCreateArchetype(componentType, oldArchetype);

            if(newArchetype != oldArchetype)
            {
                MoveEntity(entity, oldArchetype, newArchetype);

                record = _entityRecords[entity];
            }

            newArchetype.Set(record.Row, component);
        }

        public void Set<T>(Entity entity, in T component) where T: struct
        {
            EntityRecord record = _entityRecords[entity];

            record.Archetype.Set<T>(record.Row, component);
        }

        public T Get<T>(Entity entity) where T : struct
        {
            EntityRecord record = _entityRecords[entity];

            return record.Archetype.Get<T>(record.Row);
        }

        private Archetype GetOrCreateArchetype(in ComponentType componentType, Archetype oldArchetype)
        {
            if(_componentIndices.TryGetValue(componentType.Id, out var archetypeMap))
            {
                if(archetypeMap.TryGetValue(oldArchetype.Id, out ArchetypeRecord archetypeRecord))
                {
                    return oldArchetype;
                }
            }
            else
            {
                _componentIndices.Add(componentType.Id, []);
            }

            Archetype? archetype = oldArchetype.GetAddEdge(componentType);

            if (archetype == null)
            {
                SortedSet<ComponentType> componentTypes = new(oldArchetype.ComponentTypes)
                {
                    componentType,
                };

                Entity archetypeEntity = CreateEntity($"Archetype_{_numArchetypesCreated++}");

                var componentTypesImmutable = componentTypes.ToImmutableSortedSet();

                archetype = new Archetype(componentTypesImmutable, archetypeEntity.Id);

                ArchetypeRecord archetypeRecord = new(componentTypesImmutable.IndexOf(componentType));

                _componentIndices[componentType.Id].Add(archetype.Id, archetypeRecord);

                _archetypes.Add(archetype.Id, archetype);

                oldArchetype.AddAddEdge(componentType, archetype);
            }

            return archetype;
        }

        private void MoveEntity(Entity entity, Archetype oldArchetype, Archetype newArchetype)
        {
            int             destinationRow = newArchetype.AddEntity(entity);

            EntityRecord    record = _entityRecords[entity];
            int             sourceRow = record.Row;
            Array[]         components = oldArchetype.ComponentData;
            Array[]         destinationComponents = newArchetype.ComponentData;

            ImmutableSortedSet<ComponentType> componentTypes = oldArchetype.ComponentTypes;

            for (int column = 0; column < components.Length; column++)
            {
                Array           sourceArray = components[column];
                ComponentType   componentType = componentTypes.ElementAt(column);
                Array           destinationArray = newArchetype.GetArray(componentType);

                Debug.Assert(newArchetype.HasComponent(componentType));
                Array.Copy(sourceArray, sourceRow, destinationArray, destinationRow, 1);
            }

            EntityRecord newRecord = new(newArchetype, destinationRow);

            Entity movedEntity = oldArchetype.RemoveEntity(record.Row);
            EntityRecord movedEntityRecord = new EntityRecord(oldArchetype, record.Row);

            _entityRecords[movedEntity] = movedEntityRecord;
            _entityRecords[entity] = newRecord;
        }
    }
}
