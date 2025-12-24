using System;

using emCastle.Security;

namespace emCastle.Crypto.Parameters
{
    public class X448KeyGenerationParameters
        : KeyGenerationParameters
    {
        public X448KeyGenerationParameters(SecureRandom random)
            : base(random, 448)
        {
        }
    }
}
