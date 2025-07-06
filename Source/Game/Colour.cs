using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine
{
    public struct Colour
    {
        Colour(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        float R;
        float G;
        float B;
        float A;

        public static readonly Colour Red = new(1.0f, 0.0f, 0.0f, 1.0f);
    }
}
