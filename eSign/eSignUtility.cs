using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace eSignASPLibrary
{
    public class eSignUtility
    {
        internal static string GetRequestXML(List<eSignReturnDocument> returnDocuments, string ASPID, string ts, string txn, string signerID, string responseURL, string redirectURL, bool isLTVRequired)
        {
            try
            {
                XmlDocument RequestXml = new XmlDocument();
                XmlElement SignReqTag = RequestXml.CreateElement("Esign");
                SignReqTag.SetAttribute("ver", "3.0");
                SignReqTag.SetAttribute("signerid", signerID);
                SignReqTag.SetAttribute("ts", ts);
                SignReqTag.SetAttribute("txn", txn);
                SignReqTag.SetAttribute("aspId", ASPID);
                SignReqTag.SetAttribute("responseUrl", responseURL);
                SignReqTag.SetAttribute("redirectUrl", redirectURL);
                SignReqTag.SetAttribute("signingAlgorithm", "ECDSA");
                SignReqTag.SetAttribute("maxWaitPeriod", "1440");
                XmlElement DocsTag = RequestXml.CreateElement("Docs");
                foreach (eSignReturnDocument returnDocument in returnDocuments)
                {
                    if (returnDocument.DocId == 0)
                    {
                        continue;
                    }
                    XmlElement InputHashTag = RequestXml.CreateElement("InputHash");
                    InputHashTag.SetAttribute("hashAlgorithm", "SHA256");
                    InputHashTag.SetAttribute("docInfo", "SHA256");
                    InputHashTag.SetAttribute("docUrl", returnDocument.DocumentURL);
                    InputHashTag.SetAttribute("docInfo", returnDocument.DocumentInfo);
                    InputHashTag.SetAttribute("responseSigType", isLTVRequired ? "PKCS7pdf" : "PKCS7");
                    InputHashTag.SetAttribute("id", returnDocument.DocId.ToString());
                    InputHashTag.InnerText = returnDocument.DocumentHash;
                    DocsTag.AppendChild(InputHashTag);
                }
                SignReqTag.AppendChild(DocsTag);
                RequestXml.AppendChild(SignReqTag);
                return RequestXml.OuterXml;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal static string GetRequestXMLV2(List<eSignReturnDocument> returnDocuments, string ASPID, string ts, string txn, string authMode, string responseURL, bool isLTVRequired)
        {
            try
            {
                XmlDocument RequestXml = new XmlDocument();
                XmlElement SignReqTag = RequestXml.CreateElement("Esign");
                SignReqTag.SetAttribute("ver", "2.1");
                SignReqTag.SetAttribute("ekycId", "");
                SignReqTag.SetAttribute("ts", ts);
                SignReqTag.SetAttribute("txn", txn);
                SignReqTag.SetAttribute("aspId", ASPID);
                SignReqTag.SetAttribute("responseUrl", responseURL);
                //SignReqTag.SetAttribute("redirectUrl", redirectURL);
                SignReqTag.SetAttribute("responseSigType", isLTVRequired ? "PKCS7pdf" : "PKCS7");
                //SignReqTag.SetAttribute("signingAlgorithm", "RSA");
                //SignReqTag.SetAttribute("maxWaitPeriod", "1440");
                SignReqTag.SetAttribute("sc", "Y");
                SignReqTag.SetAttribute("ekycIdType", "A");
                SignReqTag.SetAttribute("AuthMode", authMode.ToString());
                XmlElement DocsTag = RequestXml.CreateElement("Docs");
                foreach (eSignReturnDocument returnDocument in returnDocuments)
                {
                    if (returnDocument.DocId == 0)
                    {
                        continue;
                    }
                    XmlElement InputHashTag = RequestXml.CreateElement("InputHash");
                    InputHashTag.SetAttribute("hashAlgorithm", "SHA256");
                    //InputHashTag.SetAttribute("docInfo", "SHA256");
                    // InputHashTag.SetAttribute("docUrl", returnDocument.DocumentURL);
                    InputHashTag.SetAttribute("docInfo", returnDocument.DocumentInfo);
                    // InputHashTag.SetAttribute("responseSigType", "pkcs7");
                    InputHashTag.SetAttribute("id", returnDocument.DocId.ToString());
                    InputHashTag.InnerText = returnDocument.DocumentHash;
                    DocsTag.AppendChild(InputHashTag);
                }
                SignReqTag.AppendChild(DocsTag);
                RequestXml.AppendChild(SignReqTag);
                return RequestXml.OuterXml;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal static string GetStatusXML(string ASPID, string ts, string txn)
        {
            XmlDocument RequestXml = new XmlDocument();
            XmlElement EsignStatusTag = RequestXml.CreateElement("EsignStatus");
            EsignStatusTag.SetAttribute("ver", "3.0");
            EsignStatusTag.SetAttribute("ts", ts);
            EsignStatusTag.SetAttribute("txn", txn);
            EsignStatusTag.SetAttribute("aspId", ASPID);
            RequestXml.AppendChild(EsignStatusTag);
            return RequestXml.OuterXml;
        }
        internal static string SignXML(string XMLValue)
        {
            try
            {
                string SignedXML = string.Empty;
                string PFXFilePath = string.IsNullOrWhiteSpace(eSignSettings.pfxPath) ? System.Web.Configuration.WebConfigurationManager.AppSettings["DocumentSigner"].ToString() : eSignSettings.pfxPath;
                string PFXPassword = string.IsNullOrWhiteSpace(eSignSettings.pfxPassword) ? System.Web.Configuration.WebConfigurationManager.AppSettings["DocumentSignerPassword"].ToString() : eSignSettings.pfxPassword;
                CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
                X509Certificate2 Cert = new X509Certificate2(PFXFilePath, PFXPassword, X509KeyStorageFlags.Exportable);
                XmlDocument Document = new XmlDocument();
                Document.LoadXml(XMLValue);
                var exportedKeyMaterial = Cert.PrivateKey.ToXmlString(true);
                var key = new RSACryptoServiceProvider();
                key.PersistKeyInCsp = false;
                key.FromXmlString(exportedKeyMaterial);
                SignedXml signedXml = new SignedXml(Document);
                signedXml.SigningKey = key;
                Reference reference = new Reference();
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                reference.Uri = "";
                signedXml.AddReference(reference);
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new KeyInfoX509Data(Cert));
                signedXml.KeyInfo = keyInfo;
                signedXml.ComputeSignature();
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                Document.DocumentElement.AppendChild(Document.ImportNode(xmlDigitalSignature, true));
                SignedXML = Document.OuterXml;
                return SignedXML;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal static string HttpsWebClientSendRequest(string URI, string queryString)
        {
            string result = string.Empty;
            WebClient webclient = null;
            try
            {
                if (eSignSettings.isProxyRequired)
                {
                    IWebProxy userWebProxy = new WebProxy(eSignSettings.proxyIP, eSignSettings.proxyPort);
                    if (eSignSettings.isDefaultCredentials)
                    {
                        userWebProxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    else
                    {
                        userWebProxy.Credentials = new NetworkCredential(eSignSettings.userName, eSignSettings.password);
                    }
                    webclient = new WebClient { Proxy = userWebProxy };
                }
                else
                {
                    IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                    defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                    webclient = new WebClient { Proxy = defaultWebProxy };
                }
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };

                webclient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                result = webclient.UploadString(URI, queryString);
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                webclient = null;
            }
        }

        internal static string GenerateTempTransactionData(List<eSignReturnDocument> returnDocuments)
        {
            try
            {
                string tempData = string.Empty;
                foreach (eSignReturnDocument returnDocument in returnDocuments)
                {
                    if (returnDocument.DocId == 0)
                    {
                        continue;
                    }
                    tempData = tempData + returnDocument.GetReturnDocumentBase64() + "|";
                }
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(tempData));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<eSignReturnDocument> GetReturnDocumentsFromPreSignedPDFFile(string PreSignedDocuments)
        {
            try
            {
                List<eSignReturnDocument> returnDocuments = new List<eSignReturnDocument>() { };
                string tempFileStr = Encoding.UTF8.GetString(Convert.FromBase64String(PreSignedDocuments));
                var returnDocumentsStrs = tempFileStr.Split('|').ToList();
                foreach (var returnDocumentStr in returnDocumentsStrs)
                {
                    if (string.IsNullOrWhiteSpace(returnDocumentStr))
                    {
                        continue;
                    }
                    returnDocuments.Add(new eSignReturnDocument(returnDocumentStr));
                }
                return returnDocuments;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}


