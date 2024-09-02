using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public class Archetype
    {
        public ImmutableArray<ComponentType>    Components { get; private set; } = [];

        private const int                       _maxEntities = 1024;
        private ulong[]                         _entityIds = new ulong[_maxEntities];
        private int                             _numEntities = 0;

        private readonly int[]                  _componentIdToArrayIndex = [];
        private Array[]                         _data;

        public Archetype(ComponentType[] components) 
        {
            Array.Fill(_entityIds, ulong.MaxValue);

            Components = components.ToImmutableArray();
            _componentIdToArrayIndex = Component<int>.ToLookupArray(components);

            _data = new Array[components.Length];

            for (int i = 0; i < components.Length; i++)
            {
                _data[i] = Array.CreateInstance(components[i].Type, _maxEntities);
            }
        }

        public bool ContainsEntity(ulong entityId)
        {
            return _entityIds.Contains(entityId);
        }

        public int AddEntity(ulong entityId)
        {
            Debug.Assert(ContainsEntity(entityId) == false);

            _entityIds[_numEntities] = entityId;
            _numEntities++;

            return _numEntities - 1;
        }

        public void RemoveEntity(ulong row)
        {
            //Debug.Assert(ContainsEntity(entityId));

        }
        public void MoveEntityIntoHere(Archetype oldArchetype, ulong entityId)
        {
            Debug.Assert(ContainsEntity(entityId) == false);
            Debug.Assert(oldArchetype.ContainsEntity(entityId));
        }

        public void Set<T>(int row, in T component)
        {
            int id = Component<T>.ComponentType.id;

            int dataIndex = _componentIdToArrayIndex[id];

            T[] compData = Unsafe.As<T[]>(_data[dataIndex]);

            compData[row] = component;

        }
    }
}
