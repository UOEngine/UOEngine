using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.EntityComponentSystem
{
    internal static class EntityPool
    {
        private static ulong        _id = 0;
        private static Queue<ulong> _freedIds = [];

        public static ulong Get()
        {
            if (_freedIds.Count > 0)
            {
                ulong id = _freedIds.Dequeue();

                return id;
            }

            ulong value = _id;

            _id++;

            return value;
        }

        public static void Free(ulong id)
        {
            _freedIds.Enqueue(id);
        }
    }
}
