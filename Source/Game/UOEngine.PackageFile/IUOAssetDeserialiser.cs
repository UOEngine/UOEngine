using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.UOAssets
{
    public interface IUOAssetDeserialiser<T>
    {
        public T Deserialise(BinaryReader reader);
    }
}
