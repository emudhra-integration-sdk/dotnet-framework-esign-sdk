using System;

namespace emCastle.Math.EC
{
    public interface ECLookupTable
    {
        int Size { get; }
        ECPoint Lookup(int index);
    }
}
