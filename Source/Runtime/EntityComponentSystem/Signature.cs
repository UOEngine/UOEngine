using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.EntityComponentSystem
{
    public struct Signature
    {
        public ComponentType[] Components = [];

        public Signature(ComponentType[] components)
        {
            Components = components;
        }

        public uint ToMask()
        {
            uint mask = 0;

            //foreach (var component in Components)
            //{
            //    mask |= (uint)(1 << component.id);
            //}

            return mask;
        }
    };
}
