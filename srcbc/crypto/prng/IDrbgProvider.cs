using System;

using emCastle.Crypto.Prng.Drbg;

namespace emCastle.Crypto.Prng
{
    internal interface IDrbgProvider
    {
        ISP80090Drbg Get(IEntropySource entropySource);
    }
}
