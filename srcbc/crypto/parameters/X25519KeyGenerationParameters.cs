using System;

using emCastle.Security;

namespace emCastle.Crypto.Parameters
{
    public class X25519KeyGenerationParameters
        : KeyGenerationParameters
    {
        public X25519KeyGenerationParameters(SecureRandom random)
            : base(random, 255)
        {
        }
    }
}
