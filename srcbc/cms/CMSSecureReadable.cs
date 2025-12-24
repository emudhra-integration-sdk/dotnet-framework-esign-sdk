using System;

using emCastle.Asn1.X509;
using emCastle.Crypto.Parameters;

namespace emCastle.Cms
{
	internal interface CmsSecureReadable
	{
		AlgorithmIdentifier Algorithm { get; }
		object CryptoObject { get; }
		CmsReadable GetReadable(KeyParameter key);
	}
}
