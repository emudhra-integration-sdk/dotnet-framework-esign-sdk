using System;
using System.IO;

using emCastle.Asn1;
using emCastle.Asn1.X509;
using emCastle.Crypto;
using emCastle.Crypto.Parameters;
using emCastle.Security;
using emCastle.Utilities.Date;
using emCastle.Utilities.IO;

namespace emCastle.Cms
{
	public class CmsAuthenticatedGenerator
		: CmsEnvelopedGenerator
	{
		/**
		* base constructor
		*/
		public CmsAuthenticatedGenerator()
		{
		}

		/**
		* constructor allowing specific source of randomness
		*
		* @param rand instance of SecureRandom to use
		*/
		public CmsAuthenticatedGenerator(
			SecureRandom rand)
			: base(rand)
		{
		}
	}
}
