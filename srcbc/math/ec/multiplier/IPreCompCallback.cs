using System;

namespace emCastle.Math.EC.Multiplier
{
    public interface IPreCompCallback
    {
        PreCompInfo Precompute(PreCompInfo existing);
    }
}
