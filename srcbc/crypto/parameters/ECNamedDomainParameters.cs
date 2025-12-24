using System;

using emCastle.Asn1;
using emCastle.Math;
using emCastle.Math.EC;

namespace emCastle.Crypto.Parameters
{
    public class ECNamedDomainParameters
        : ECDomainParameters
    {
        private readonly DerObjectIdentifier name;

        public DerObjectIdentifier Name
        {
            get { return name; }
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECDomainParameters dp)
            : this(name, dp.curve, dp.g, dp.n, dp.h, dp.seed)
        {
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n)
            : base(curve, g, n)
        {
            this.name = name;
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n, BigInteger h)
            : base(curve, g, n, h)
        {
            this.name = name;
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n, BigInteger h, byte[] seed)
            : base(curve, g, n, h, seed)
        {
            this.name = name;
        }
    }
}
