using emCastle.Cms;
using emCastle.Utilities.Encoders;
using emCastle.X509;
using System;
using System.Text;

namespace eSignASPLibrary
{
    internal static class ValidateLicense
    {
        internal static bool verifySignature(string unSignedData, string signedData)
        {
            try
            {
                bool result = false;
                string Certificate =
                "MIIB7zCCAZWgAwIBAgIEV+y33zAKBggqhkjOPQQDAjB3MQswCQYDVQQGEwJJTjES" +
                "MBAGA1UECAwJS2FybmF0YWthMRgwFgYDVQQKDA9lTXVkaHJhIExpbWl0ZWQxEzAR" +
                "BgNVBAsMClRlY2hub2xvZ3kxJTAjBgNVBAMMHGVNdWRocmEgTGljZW5zZSBQcm90" +
                "ZWN0aW9uIDEwHhcNMTYwOTI5MDY0NDAxWhcNMjYwOTI5MDY0NDAxWjB3MQswCQYD" +
                "VQQGEwJJTjESMBAGA1UECAwJS2FybmF0YWthMRgwFgYDVQQKDA9lTXVkaHJhIExp" +
                "bWl0ZWQxEzARBgNVBAsMClRlY2hub2xvZ3kxJTAjBgNVBAMMHGVNdWRocmEgTGlj" +
                "ZW5zZSBQcm90ZWN0aW9uIDEwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAASo2Hlz" +
                "HnlGNiEKG0RoNEMEOr7sPcoWOK5PqTaVqoIeLF36BjKhIXXSS1y+AaO5UutBitsv" +
                "sf4wKnWbXEPjDzuyow8wDTALBgNVHQ8EBAMCBPAwCgYIKoZIzj0EAwIDSAAwRQIg" +
                "OdwZuqCMuZSDgw3WfCxNDe6izAYqo2FSf7jJWM7nmggCIQCxArpFjiB2atyeyAfN" +
                "ZlAVdHB1AfZO1ZT/G+rLt+JX2Q==";

                byte[] signedByte = Base64.Decode(signedData);

                CmsSignedData s = new CmsSignedData(new CmsProcessableByteArray(Encoding.UTF8.GetBytes(unSignedData)), signedByte);
                SignerInformationStore signers = s.GetSignerInfos();
                SignerInformation signerInfo = null;
                foreach (object obj in signers.GetSigners())
                {
                    signerInfo = (SignerInformation)obj;
                    break;
                }
                byte[] CertData = Convert.FromBase64String(Certificate);
                System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(CertData);
                X509Certificate cert = emCastle.Security.DotNetUtilities.FromX509Certificate(certificate);
                result = signerInfo.Verify(cert);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
