namespace UOEngine.Runtime.EntityComponentSystem
{
    public readonly record struct ComponentType(int id)
    {
        public readonly int Id = id; // Like a type id.
        public Type Type => ComponentRegistry.Types[Id];
    }

    public static class Component<T>
    {
        public static readonly ComponentType ComponentType;

        public static readonly Signature Signature;
        static Component()
        {
            ComponentType = ComponentRegistry.Add<T>();
            Signature = new Signature([ComponentType]);
        }

        public static int[] ToLookupArray(ComponentType[] types)
        {
            var max = 0;

            foreach (var type in types)
            {
                var componentId = type.Id;

                if (componentId >= max)
                {
                    max = componentId;
                }
            }

            var array = new int[max + 1];

            Array.Fill(array, -1);

            for (var index = 0; index < types.Length; index++)
            {
                ref var type = ref types[index];
                var componentId = type.Id;
                array[componentId] = index;
            }

            return array;
        }
    }
}
