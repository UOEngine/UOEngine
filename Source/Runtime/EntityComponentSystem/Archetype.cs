using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public class Archetype
    {
        public readonly ImmutableSortedSet<ComponentType>   ComponentTypes;
        public readonly ulong                               Id;

        public readonly Array[]                             ComponentData = [];

        private const int                                   _maxEntities = 1024;
        private Entity[]                                    _entities = new Entity[_maxEntities];
        private int                                         _numEntities = 0;

        private Dictionary<ComponentType, Archetype>        _addEdges = [];
        private Dictionary<ComponentType, Archetype>        _removeEdges = [];

        public Archetype(ImmutableSortedSet<ComponentType> componentTypes, ulong id) 
        {
            Id = id;
            ComponentTypes = componentTypes;

            ComponentData = new Array[componentTypes.Count];
            
            for(int i = 0; i < componentTypes.Count; i++)
            {
                ComponentData[i] = Array.CreateInstance(componentTypes.ElementAt(i).Type, _maxEntities);
            }
        }

        public int AddEntity(Entity entity)
        {
            Debug.Assert(_entities.Contains(entity) == false);

            var numEntities = _numEntities;

            _entities[numEntities] = entity;

            _numEntities++;

            return numEntities;
        }

        public Entity RemoveEntity(int row)
        {
            int     lastRow = _numEntities - 1;
            Entity  lastEntity = _entities[lastRow];

            Array[] components = ComponentData;

            for (int column = 0; column < ComponentData.Length; column++)
            {
                Array sourceArray = components[column];
                Type type = sourceArray.GetType();

                //newArchetype.Set(destinationRow, co)
            }

            _entities[row] = lastEntity;

            _numEntities--;

            return lastEntity;
        }

        public Archetype? GetAddEdge(ComponentType componentType)
        {
            if(_addEdges.TryGetValue(componentType, out var edge))
            {
                return edge;
            }

            return null;
        }

        public void AddAddEdge(ComponentType componentType, Archetype archetype)
        {
            _addEdges.Add(componentType, archetype);
        }

        public void Set<T>(int row, in T? component) where T: struct
        {
            ulong id = Component<T>.ComponentType.id;

            int index = ComponentTypes.IndexOf(Component<T>.ComponentType);

            //int dataIndex = _componentIdToArrayIndex[id];

            //T[] compData = Unsafe.As<T[]>(ComponentData[dataIndex]);

            //compData[row] = component;

        }

        public T Get<T>(int componentRow, int column)
        {
            T[] data = Unsafe.As<T[]>(ComponentData[componentRow]);

            return data[column];
        }

        public Array GetArray(ComponentType componentType)
        {
            Debug.Assert(false);

            return ComponentData[0];
        }
    }
}
