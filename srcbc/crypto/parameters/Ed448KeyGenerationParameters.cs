using System;

using emCastle.Security;

namespace emCastle.Crypto.Parameters
{
    public class Ed448KeyGenerationParameters
        : KeyGenerationParameters
    {
        public Ed448KeyGenerationParameters(SecureRandom random)
            : base(random, 448)
        {
        }
    }
}
