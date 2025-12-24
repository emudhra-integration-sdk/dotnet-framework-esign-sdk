using System;
using System.IO;

namespace emCastle.Cms
{
	public interface CmsReadable
	{
		Stream GetInputStream();
	}
}
