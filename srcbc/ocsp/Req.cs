using System;
using System.Collections;
using System.IO;

using emCastle.Asn1;
using emCastle.Asn1.Ocsp;
using emCastle.Asn1.X509;
using emCastle.X509;

namespace emCastle.Ocsp
{
	public class Req
		: X509ExtensionBase
	{
		private Request req;

		public Req(
			Request req)
		{
			this.req = req;
		}

		public CertificateID GetCertID()
		{
			return new CertificateID(req.ReqCert);
		}

		public X509Extensions SingleRequestExtensions
		{
			get { return req.SingleRequestExtensions; }
		}

		protected override X509Extensions GetX509Extensions()
		{
			return SingleRequestExtensions;
		}
	}
}
