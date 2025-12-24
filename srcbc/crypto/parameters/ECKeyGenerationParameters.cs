using System;

using emCastle.Asn1;
using emCastle.Asn1.CryptoPro;
using emCastle.Security;

namespace emCastle.Crypto.Parameters
{
    public class ECKeyGenerationParameters
		: KeyGenerationParameters
    {
        private readonly ECDomainParameters domainParams;
		private readonly DerObjectIdentifier publicKeyParamSet;

		public ECKeyGenerationParameters(
			ECDomainParameters	domainParameters,
			SecureRandom		random)
			: base(random, domainParameters.N.BitLength)
        {
            this.domainParams = domainParameters;
        }

		public ECKeyGenerationParameters(
			DerObjectIdentifier	publicKeyParamSet,
			SecureRandom		random)
			: this(ECKeyParameters.LookupParameters(publicKeyParamSet), random)
		{
			this.publicKeyParamSet = publicKeyParamSet;
		}

		public ECDomainParameters DomainParameters
        {
			get { return domainParams; }
        }

		public DerObjectIdentifier PublicKeyParamSet
		{
			get { return publicKeyParamSet; }
		}
    }
}
