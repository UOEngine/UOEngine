using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.EntityComponentSystem
{
    internal static class ComponentRegistry
    {
        public static int                                           NumComponents { get; private set;  } = 0;

        public static ReadOnlySpan<Type>                            Types => _types;

        public static IReadOnlyDictionary<Type, ComponentType>      TypeToComponentType => _typeToComponentType;

        private static readonly Dictionary<Type, ComponentType>     _typeToComponentType = new(64);
        private static Type[]                                       _types = new Type[64];

        public static ComponentType Add<T>()
        {
            return Add(typeof(T));
        }

        private static ComponentType Add(Type type)
        {
            if (_typeToComponentType.TryGetValue(type, out ComponentType component))
            {
                return component;
            }

            int id = NumComponents + 1;

            component = new ComponentType(id);

            _types[id] = type;

            _typeToComponentType.Add(type, component);

            NumComponents++;

            return component;
        }
    }
}
