using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using emCastle.Asn1;
using emCastle.Asn1.CryptoPro;
using emCastle.Asn1.Pkcs;
using emCastle.Asn1.X509;
using emCastle.Asn1.X9;
using emCastle.Crypto;
using emCastle.Crypto.Generators;
using emCastle.Crypto.Parameters;
using emCastle.Math;
using emCastle.Pkcs;
using emCastle.Security;
using emCastle.Security.Certificates;
using emCastle.Utilities.Encoders;
using emCastle.Utilities.IO.Pem;
using emCastle.X509;

namespace emCastle.OpenSsl
{
	/// <remarks>General purpose writer for OpenSSL PEM objects.</remarks>
	public class PemWriter
		: emCastle.Utilities.IO.Pem.PemWriter
	{
		/// <param name="writer">The TextWriter object to write the output to.</param>
		public PemWriter(
			TextWriter writer)
			: base(writer)
		{
		}

		public void WriteObject(
			object obj) 
		{
			try
			{
				base.WriteObject(new MiscPemGenerator(obj));
			}
			catch (PemGenerationException e)
			{
				if (e.InnerException is IOException)
					throw (IOException)e.InnerException;

				throw e;
			}
		}

		public void WriteObject(
			object			obj,
			string			algorithm,
			char[]			password,
			SecureRandom	random)
		{
			base.WriteObject(new MiscPemGenerator(obj, algorithm, password, random));
		}
	}
}
