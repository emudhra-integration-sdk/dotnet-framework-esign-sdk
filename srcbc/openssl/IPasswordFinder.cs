using System;

namespace emCastle.OpenSsl
{
	public interface IPasswordFinder
	{
		char[] GetPassword();
	}
}
