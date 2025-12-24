using System;

namespace emCastle.Cms
{
	internal interface IDigestCalculator
	{
		byte[] GetDigest();
	}
}
