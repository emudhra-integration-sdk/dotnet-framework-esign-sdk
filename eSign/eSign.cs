using emCastle.Utilities.Encoders;
using eSignASPLibrary;
using eSignLibrary.text;
using eSignLibrary.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace eSignASPLibrary
{
    public sealed class eSign
    {
        public enum status
        {
            Failure = 0,
            Success = 1,
            Pending = 2,
        }

        public enum DocType
        {
            Pdf = 0,
            Hash = 1,
            eMandate = 2
        }

        public enum AppreanceRunDirection
        {
            RUN_DIRECTION_LTR,
            RUN_DIRECTION_RTL
        }

        public enum Coordinates
        {
            Top_Left = 1,
            Top_Center = 2,
            Top_Right = 3,
            Middle_Left = 4,
            Middle_Right = 5,
            Middle_Center = 6,
            Bottom_Left = 7,
            Bottom_Center = 8,
            Bottom_Right = 9
        }

        public enum PageToBeSigned
        {
            FIRST = 1,
            LAST = 2,
            EVEN = 3,
            ODD = 4,
            ALL = 5,
            SPECIFY = 6,
            PAGE_LEVEL = 7
        }

        public enum eSignAPIVersion
        {
            V2,
            V3
        }

        public enum AuthMode
        {
            OTP = 1,
            Fingerprint = 2,
            IRIS = 3,
            FACE = 4
        }

        public enum AppearanceType
        {
            Default = 0,
            Aadhaar = 1
        }

        private List<eSignReturnDocument> returnDocuments;
        //public eSign(string LicenceFilePath, string PFXFilePath, string PFXPassword, string PFXAlias, int SignatureContents = 21000) : this(LicenceFilePath, PFXFilePath, PFXPassword, PFXAlias, false, string.Empty, 0, string.Empty, string.Empty, SignatureContents) { }
        public eSign(string PFXFilePath, string PFXPassword, string PFXAlias, bool IsProxyRequired, string ProxyIP, int ProxyPort, string ProxyUserName, string ProxyPassword, string ASPID, string eSignURL, string eSignURLV2, string eSignCheckStatusURL, int SignatureContents = 21000)
        {
            eSignSettings.isProxyRequired = IsProxyRequired;
            eSignSettings.proxyIP = ProxyIP;
            eSignSettings.proxyPort = ProxyPort;
            eSignSettings.userName = ProxyUserName;
            eSignSettings.password = ProxyPassword;
            eSignSettings.pfxPath = PFXFilePath;
            eSignSettings.pfxPassword = PFXPassword;
            eSignSettings.pfxAlias = PFXAlias;
            eSignSettings.SignatureContents = SignatureContents;
            eSignSettings.ASPID = ASPID;
            eSignSettings.eSignURL = eSignURL;
            eSignSettings.eSignURLV2 = eSignURLV2;
            eSignSettings.eSignCheckStatusURL = eSignCheckStatusURL;            
        }

        public eSignServiceReturn GetGateWayParam(List<eSignInput> eSignInputs, string signerID, string transactionID, string resposeURL, string redirectUrl, string TempFolderPath, eSign.eSignAPIVersion eSignAPIVersion = eSignAPIVersion.V3, eSign.AuthMode authMode = AuthMode.OTP, bool isLTVRequired = true)
        {
            eSignServiceReturn oreturn = new eSignServiceReturn();
            string ASPID = string.Empty;
            string Timestamp = string.Empty;
            string TransactionID = string.Empty;
            //int contentEstimated = 8192 * 2;
            int contentEstimated = eSignSettings.SignatureContents;
            returnDocuments = new List<eSignReturnDocument>() { };
            try
            {
                if (eSignAPIVersion == eSignAPIVersion.V3)
                    Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                else
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff");
                TransactionID = string.IsNullOrWhiteSpace(transactionID) ? DateTime.Now.ToString("yyyyMMddHHmmssfff") : transactionID;
                oreturn.TransactionID = TransactionID;
                string tempFilePath = TempFolderPath + Path.DirectorySeparatorChar + TransactionID + ".sig";               
                ASPID = eSignSettings.ASPID;
                oreturn.ASPID = ASPID;
                if (eSignInputs.Count > 5)
                {
                    oreturn = new eSignServiceReturn() { ASPID = ASPID, TransactionID = TransactionID, ErrorCode = "ESS-102", ErrorMessage = "Maximum 5 Documents can be signed in a single request." };
                    return oreturn;
                }
                if (string.IsNullOrWhiteSpace(signerID))
                {
                    signerID = "";
                }
                if (string.IsNullOrWhiteSpace(TempFolderPath))
                {
                    tempFilePath = string.Empty;
                    //oreturn = new eSignServiceReturn() { ASPID = ASPID, TransactionID = TransactionID, ErrorCode = "ESS-110", ErrorMessage = "Temp folder path cannot be empty." };
                    //return oreturn;
                }
                else
                {
                    if (!Directory.Exists(TempFolderPath))
                    {
                        Directory.CreateDirectory(TempFolderPath);
                    }
                }
                if (eSignInputs.Count == 0)
                {
                    oreturn = new eSignServiceReturn() { ASPID = ASPID, TransactionID = TransactionID, ErrorCode = "ESS-113", ErrorMessage = "Must contain more than one input" };
                    return oreturn;
                }
                int count = 1;
                foreach (var eSignInput in eSignInputs)
                {
                    try
                    {
                        if (eSignInput.docType.Equals(DocType.Pdf))
                        {
                            byte[] decodePDF = Base64.Decode(eSignInput.DocBase64);
                            PdfReader reader = new PdfReader(decodePDF);
                            if (reader.IsRebuilt())
                            {
                                reader.EnableRebuild();
                                MemoryStream os1 = new MemoryStream();
                                PdfStamper stamper1 = new PdfStamper(reader, os1);
                                stamper1.Close();
                                reader = new PdfReader(os1.ToArray());
                            }
                            MemoryStream os = new MemoryStream();
                            PdfStamper stamper = PdfStamper.CreateSignature(reader, os, '\0', null, eSignInput.CoSign);
                            #region Appearance creation
                            PdfSignatureAppearance signatureAppearance = stamper.SignatureAppearance;

                            StringBuilder layer2text = new StringBuilder();
                            layer2text.Append("Digitally Signed.\n");
                            if (!string.IsNullOrWhiteSpace(eSignInput.SignedBy))
                            {
                                layer2text.Append(string.Format("Name: {0} \n", eSignInput.SignedBy));
                                signatureAppearance.Contact = eSignInput.SignedBy;
                            }
                            layer2text.Append(string.Format("Date: {0} \n", DateTime.Now.AddMinutes(1).ToString("dd-MMM-yyyy HH:mm:ss")));
                            signatureAppearance.SignDate = DateTime.Now.AddMinutes(1);
                            if (!string.IsNullOrWhiteSpace(eSignInput.Reason))
                            {
                                layer2text.Append(string.Format("Reason: {0} \n", eSignInput.Reason));
                                signatureAppearance.Reason = eSignInput.Reason;
                            }
                            if (!string.IsNullOrWhiteSpace(eSignInput.Location))
                            {
                                layer2text.Append(string.Format("Location: {0} \n", eSignInput.Location));
                                signatureAppearance.Location = eSignInput.Location;
                            }
                            // For Aadhaar appearance type: leave blank; PatchSignatureAppearance fills it in Phase 2 from the cert.
                            // For Default: bake the SignedBy/date/reason/location text into the appearance at Phase 1.
                            signatureAppearance.Layer2Text = eSignInput.AppearanceType == AppearanceType.Aadhaar
                                ? string.Empty
                                : layer2text.ToString();

                            if (!eSignInput.RequiredValidMessage)
                            {
                                signatureAppearance.TOP_SECTION = 0f;
                            }


                            BaseFont bf = BaseFont.GetArabicFont();
                            Font font = new Font(bf, Font.NORMAL);
                            if (eSignInput.FontSize != 8)
                            {
                                font = new Font(bf, eSignInput.FontSize);
                            }
                            signatureAppearance.Layer2Font = font;
                            signatureAppearance.Acro6Layers = false;
                            signatureAppearance.DisableGreenTick = !eSignInput.RequiredGreenTick;
                            signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                            signatureAppearance.CertificationLevel = PdfSignatureAppearance.NOT_CERTIFIED;
                            int[] pages = null;
                            ArrayList ar = null;
                            switch (eSignInput.PageTobeSigned)
                            {
                                case PageToBeSigned.FIRST:
                                    {
                                        pages = new int[1];
                                        pages[0] = 1;
                                    }
                                    break;
                                case PageToBeSigned.LAST:
                                    {
                                        pages = new int[1];
                                        pages[0] = reader.NumberOfPages;
                                    }
                                    break;
                                case PageToBeSigned.EVEN:
                                    {
                                        ar = new ArrayList();
                                        for (int i = 2; i <= reader.NumberOfPages; i = i + 2)
                                        {
                                            ar.Add(i);
                                        }
                                        pages = new int[ar.Count];
                                        for (int j = 0; j < ar.Count; j++)
                                        {
                                            pages[j] = Convert.ToInt32(ar[j]);
                                        }
                                    }
                                    break;
                                case PageToBeSigned.ODD:
                                    {
                                        ar = new ArrayList();
                                        for (int i = 1; i <= reader.NumberOfPages; i = i + 2)
                                        {
                                            ar.Add(i);
                                        }
                                        pages = new int[ar.Count];
                                        for (int j = 0; j < ar.Count; j++)
                                        {
                                            pages[j] = Convert.ToInt32(ar[j]);
                                        }
                                    }
                                    break;
                                case PageToBeSigned.ALL:
                                    {
                                        ar = new ArrayList();
                                        pages = new int[reader.NumberOfPages];
                                        for (int i = 0; i < reader.NumberOfPages; i++)
                                        {
                                            ar.Add(i + 1);
                                        }
                                        for (int j = 0; j < pages.Length; j++)
                                        {
                                            pages[j] = Convert.ToInt32(ar[j]);
                                        }
                                    }
                                    break;
                                case PageToBeSigned.SPECIFY:
                                    string[] Pagelevel;
                                    Pagelevel = eSignInput.PageNumbers.Split(',');
                                    pages = new int[Pagelevel.Length];
                                    for (int j = 0; j < Pagelevel.Length; j++)
                                    {
                                        pages[j] = Convert.ToInt32(Pagelevel[j]);
                                    }
                                    break;
                            }
                            string cuCoordinates = "";
                            switch (eSignInput.Coordinates)
                            {
                                case Coordinates.Top_Left:
                                    cuCoordinates = "25,715,150,785";
                                    break;
                                case Coordinates.Top_Center:
                                    cuCoordinates = "275,715,400,785";
                                    break;
                                case Coordinates.Top_Right:
                                    cuCoordinates = "425,715,550,785";
                                    break;
                                case Coordinates.Middle_Left:
                                    cuCoordinates = "7,493,124,423";
                                    break;
                                case Coordinates.Middle_Right:
                                    cuCoordinates = "468,491,590,421";
                                    break;
                                case Coordinates.Middle_Center:
                                    cuCoordinates = "275,492,400,422";
                                    break;
                                case Coordinates.Bottom_Left:
                                    cuCoordinates = "7,65,125,2";
                                    break;
                                case Coordinates.Bottom_Center:
                                    cuCoordinates = "275,65,400,2";
                                    break;
                                case Coordinates.Bottom_Right:
                                    cuCoordinates = "468,66,590,3";
                                    break;
                            }
                           
                            Rectangle rect = null;
                            Rectangle[] rList = null;
                            if (eSignInput.PageTobeSigned == PageToBeSigned.PAGE_LEVEL)
                            {
                                string[] Cordinatespagelevel, Pagelevel;
                                //Cordinatespagelevel = reformatPagelevelCoordinates(eSignInput.pageLevelCoordinates, reader.NumberOfPages).Split(';');
                                Cordinatespagelevel = eSignInput.PageLevelCoordinates.Split(';');
                                pages = new int[Cordinatespagelevel.Length];
                                rList = new Rectangle[Cordinatespagelevel.Length];
                                for (int i = 0; i < Cordinatespagelevel.Length; i++)
                                {
                                    Pagelevel = Cordinatespagelevel[i].Split(',');
                                    if (Pagelevel.Length > 1)
                                    {
                                        pages[i] = Convert.ToInt32(Pagelevel[0]);
                                        rect = new Rectangle(Convert.ToInt32(Pagelevel[1]), Convert.ToInt32(Pagelevel[2]), Convert.ToInt32(Pagelevel[3]), Convert.ToInt32(Pagelevel[4]));
                                        rList[i] = rect;
                                    }
                                }
                                signatureAppearance.SetVisibleSignature(rList, pages, "SIG_" + TransactionID);
                            }
                            else
                            {
                                //rect, pages, transactionID
                                //new Rectangle[] { rect }, pages, "SIG_" + TransactionID,rect
                                string[] Cordinatesarr;
                                if (!string.IsNullOrEmpty(eSignInput.CustomCoordinates))
                                    Cordinatesarr = eSignInput.CustomCoordinates.Split(',');
                                else
                                    Cordinatesarr = cuCoordinates.Split(',');

                                //string[] Cordinatesarr = cuCoordinates.Split(',');
                                rect = new Rectangle(Convert.ToInt32(Cordinatesarr[0]), Convert.ToInt32(Cordinatesarr[1]), Convert.ToInt32(Cordinatesarr[2]), Convert.ToInt32(Cordinatesarr[3]));
                                signatureAppearance.SetVisibleSignature(new Rectangle[] { rect }, pages, TransactionID);
                            }

                            Dictionary<PdfName, int> exc = new Dictionary<PdfName, int>();
                            exc[PdfName.CONTENTS] = contentEstimated * 2 + 2;
                            PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED);
                            dic.Reason = signatureAppearance.Reason;
                            dic.Location = signatureAppearance.Location;
                            dic.Contact = signatureAppearance.Contact;
                            dic.SignatureCreator = "eMudhra";
                            dic.Date = new PdfDate(signatureAppearance.SignDate);
                            signatureAppearance.CryptoDictionary = dic;
                            signatureAppearance.PreClose(exc);

                            //generation of sig file
                            long position = signatureAppearance.exclusionLocations[PdfName.CONTENTS].Position;
                            int outBufferSIZE = signatureAppearance.Sigout.Size;
                            string preSignedBytes = Convert.ToBase64String(signatureAppearance.Sigout.ToByteArray());
                            string preSignedPdf = position + "|" + outBufferSIZE + "|" + preSignedBytes;
                            preSignedPdf = Convert.ToBase64String(Encoding.UTF8.GetBytes(preSignedPdf));

                            Stream ostr = signatureAppearance.GetRangeStream();
                            #endregion
                            HashAlgorithm sha = new SHA256CryptoServiceProvider();
                            int read = 0;
                            byte[] buff = new byte[contentEstimated];
                            while ((read = ostr.Read(buff, 0, contentEstimated)) > 0)
                            {
                                sha.TransformBlock(buff, 0, read, buff, 0);
                            }
                            sha.TransformFinalBlock(buff, 0, 0);
                            byte[] hashd = Hex.Encode(sha.Hash);
                            string hashdocument = Encoding.UTF8.GetString(hashd, 0, hashd.Length);
                            returnDocuments.Add(new eSignReturnDocument()
                            {
                                DocumentHash = hashdocument,
                                DocType = DocType.Pdf,
                                DocId = count,
                                TempFilePath = tempFilePath,
                                SignatureAppreance = signatureAppearance,
                                DocumentURL = eSignInput.PdfUrl,
                                DocumentInfo = eSignInput.DocInfo,
                                PreSignedDocument = preSignedPdf,
                                AppearanceType = eSignInput.AppearanceType,
                            });
                        }
                        else if (eSignInput.docType.Equals(DocType.Hash))
                        {
                            returnDocuments.Add(new eSignReturnDocument()
                            {
                                DocumentHash = eSignInput.DocBase64,
                                DocType = DocType.Hash,
                                DocId = count,
                                TempFilePath = tempFilePath,
                                DocumentURL = eSignInput.PdfUrl,
                                DocumentInfo = eSignInput.DocInfo
                            });
                        }
                        else if (eSignInput.docType.Equals(DocType.eMandate))
                        {
                            try
                            {
                                byte[] bytes;
                                string xmlToSign = eSignInput.DocBase64;
                                XmlDocument xmlDocument = new XmlDocument();
                                XmlDsigC14NTransform cancolized = new XmlDsigC14NTransform(false);
                                xmlDocument.LoadXml(xmlToSign);
                                cancolized.LoadInput(xmlDocument);
                                Stream stream = (Stream)cancolized.GetOutput(typeof(Stream));
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    bytes = memoryStream.ToArray();
                                }
                                string base64CanonicalXML = Convert.ToBase64String(bytes);
                                SHA256 sha256 = SHA256.Create();
                                byte[] hash = sha256.ComputeHash(bytes);
                                string hashdocument = BitConverter.ToString(hash).Replace("-", string.Empty);
                                sha256.Dispose();
                                returnDocuments.Add(new eSignReturnDocument()
                                {
                                    DocumentHash = hashdocument,
                                    DocType = DocType.eMandate,
                                    DocId = count,
                                    TempFilePath = tempFilePath,
                                    DocumentURL = eSignInput.PdfUrl,
                                    DocumentInfo = eSignInput.DocInfo,
                                    PreSignedDocument = base64CanonicalXML
                                });
                            }
                            catch (XmlException ex)
                            {
                                returnDocuments.Add(new eSignReturnDocument()
                                {
                                    DocId = 0,
                                    TempFilePath = tempFilePath,
                                    ErrorMessage = $"error while loading XML document - {ex.Message}",
                                    ErrorCode = "ESS-116",
                                    Status = eSign.status.Failure
                                });
                                oreturn = new eSignServiceReturn() { ASPID = ASPID, ReturnValues = returnDocuments, TransactionID = TransactionID, ErrorCode = "ESS-116", ErrorMessage = $"error while loading XML document - {ex.Message}" };
                                return oreturn;
                            }
                            catch (Exception ex)
                            {
                                returnDocuments.Add(new eSignReturnDocument()
                                {
                                    DocId = 0,
                                    TempFilePath = tempFilePath,
                                    ErrorMessage = $"error while Siging XML document - {ex.Message}",
                                    ErrorCode = "ESS-117",
                                    Status = eSign.status.Failure
                                });
                                oreturn = new eSignServiceReturn() { ASPID = ASPID, ReturnValues = returnDocuments, TransactionID = TransactionID, ErrorCode = "ESS-117", ErrorMessage = $"error while Siging XML document - {ex.Message}" };
                                return oreturn;
                            }
                        }
                        count++;
                    }

                    catch (Exception ex)
                    {
                        returnDocuments.Add(new eSignReturnDocument()
                        {
                            DocId = 0,
                            TempFilePath = tempFilePath,
                            ErrorMessage = $"error while creating document hash - {ex.Message}",
                            ErrorCode = "ESS-106",
                            Status = eSign.status.Failure
                        });
                        oreturn = new eSignServiceReturn() { ASPID = ASPID, ReturnValues = returnDocuments, TransactionID = TransactionID, ErrorCode = "ESS-106", ErrorMessage = $"error while creating document hash- {ex.Message}" };
                        return oreturn;
                    }
                }
                if (returnDocuments.Where(i => i.DocId != 0).Count() <= 0)
                {
                    oreturn = new eSignServiceReturn() { ASPID = ASPID, ReturnValues = returnDocuments, TransactionID = TransactionID, ErrorCode = "ESS-106", ErrorMessage = $"error while creating document hash" };
                    return oreturn;
                }
                string tempData = eSignUtility.GenerateTempTransactionData(returnDocuments);
                if (Directory.Exists(TempFolderPath))
                {
                    File.WriteAllText(tempFilePath, tempData);
                }
                oreturn.PreSignedTempFile = tempFilePath;
                string RequestXML;
                string reqAuthMode = authMode.GetHashCode().ToString();

                if (eSignAPIVersion == eSignAPIVersion.V3)
                    RequestXML = eSignUtility.GetRequestXML(returnDocuments, ASPID, Timestamp, TransactionID, signerID, resposeURL, redirectUrl, isLTVRequired);
                else
                    RequestXML = eSignUtility.GetRequestXMLV2(returnDocuments, ASPID, Timestamp, TransactionID, reqAuthMode, resposeURL, isLTVRequired);

                string SignedRequestXML = eSignUtility.SignXML(RequestXML);
                oreturn.RequestXML = SignedRequestXML;
                string URLEncodedSignedXML = HttpUtility.UrlEncode(SignedRequestXML);
                string ResponseXML = string.Empty;
                try
                {
                    ResponseXML = eSignUtility.HttpsWebClientSendRequest(eSignAPIVersion.V3 == eSignAPIVersion ? eSignSettings.eSignURL : eSignSettings.eSignURLV2, URLEncodedSignedXML);
                    oreturn.ResponseXML = ResponseXML;
                }
                catch (WebException ex)
                {
                    oreturn = new eSignServiceReturn()
                    {
                        ReturnStatus = eSign.status.Failure,
                        TransactionID = transactionID,
                        ErrorCode = "ESS-108",
                        RequestXML = SignedRequestXML,
                        ASPID = ASPID,
                        ErrorMessage = $"Web exception while calling eSign API - {ex.Message}"
                    };
                    return oreturn;
                }
                if (string.IsNullOrWhiteSpace(ResponseXML))
                {
                    oreturn = new eSignServiceReturn()
                    {
                        ReturnStatus = eSign.status.Failure,
                        TransactionID = TransactionID,
                        ErrorCode = "ESS-109",
                        RequestXML = SignedRequestXML,
                        ResponseXML = ResponseXML,
                        ASPID = ASPID,
                        ErrorMessage = "Empty response XML from eSign API."
                    };
                    return oreturn;
                }
                oreturn.ResponseXML = ResponseXML;
                XmlDocument ResponseXMLDoc = new XmlDocument();
                ResponseXMLDoc.LoadXml(ResponseXML);
                var SignRespElement = ResponseXMLDoc.SelectSingleNode("//EsignResp");
                if (SignRespElement == null)
                {
                    oreturn = new eSignServiceReturn()
                    {
                        TransactionID = TransactionID,
                        ErrorCode = "ESS-110",
                        RequestXML = SignedRequestXML,
                        ResponseXML = ResponseXML,
                        ASPID = ASPID,
                        ErrorMessage = "Invalid ResponseXML",
                    };
                    return oreturn;
                }
                string status = SignRespElement.Attributes["status"].Value;
                string responseCode = SignRespElement.Attributes["resCode"].Value;
                string errorMessage = eSign.eSignAPIVersion.V3 == eSignAPIVersion ? SignRespElement.Attributes["errorMessage"].Value : SignRespElement.Attributes["errMsg"].Value;
                string gatewayParam = TransactionID + "|" + responseCode;
                gatewayParam = Convert.ToBase64String(Encoding.UTF8.GetBytes(gatewayParam));
                string errorCode = SignRespElement.Attributes["errorCode"].Value;
                oreturn.ErrorCode = errorCode;
                oreturn.ErrorMessage = errorMessage;
                if (status == "2")
                {
                    oreturn = new eSignServiceReturn()
                    {
                        TransactionID = TransactionID,
                        ResponseCode = responseCode,
                        ErrorCode = errorCode,
                        RequestXML = SignedRequestXML,
                        ResponseXML = ResponseXML,
                        ASPID = ASPID,
                        ErrorMessage = errorMessage,
                        GatewayParameter = gatewayParam,
                        PreSignedTempFile = tempFilePath,
                        ReturnStatus = eSign.status.Success,
                        PreSignedDocBytes = Encoding.UTF8.GetBytes(tempData)
                    };
                    return oreturn;
                }
                oreturn.RequestXML = SignedRequestXML;
                oreturn.ResponseXML = ResponseXML;
                oreturn.TransactionID = transactionID;
                oreturn.ReturnValues = returnDocuments;
                return oreturn;
            }
            catch (Exception e)
            {
                oreturn.ReturnStatus = status.Failure;
                oreturn.ErrorMessage = $"Some thing went wrong: {e.Message}";
                oreturn.ErrorCode = "ESS-999";
                return oreturn;
            }
        }
        public List<int> GetPageNumber(string pageNumber, int totalPages)
        {
            List<int> pagesList = new List<int>();
            if (pageNumber.Trim().ToLower().Equals("all"))
            {
                for (int i = 1; i <= totalPages; i++)
                {
                    pagesList.Add(i);
                }
                return pagesList;
            }
            else if (pageNumber.Trim().ToLower().Equals("l"))
            {
                pagesList.Add(totalPages);
                return pagesList;
            }
            else if (pageNumber.Trim().ToLower().Equals("sl"))
            {
                pagesList.Add(totalPages - 1);
                return pagesList;
            }
            else if (pageNumber.Trim().ToLower().Equals("f"))
            {
                pagesList.Add(1);
                return pagesList;
            }
            else if (pageNumber.Trim().ToLower().Equals("s"))
            {
                pagesList.Add(2);
                return pagesList;
            }
            int pageN;
            int.TryParse(pageNumber, out pageN);
            pagesList.Add(pageN);
            if (pageN > 0)
            {
                if (pageN > totalPages)
                {
                    throw new ArgumentException($"Invalid argument {pageNumber} for page number because page does not exist.");
                }
                return pagesList;
            }
            else
            {
                throw new ArgumentException($"Invalid argument {pageNumber} for page number, it can contain f->first, s->second, l->last, sl->second last and valid integer below number of pages.");
            }
        }
        public eSignServiceReturn GetSigedDocument(string ResponseXML, string PreSignedTempFile)
        {
            eSignServiceReturn oreturn = new eSignServiceReturn();
            try
            {
                oreturn.PreSignedTempFile = PreSignedTempFile;
                oreturn.ResponseXML = ResponseXML;
                if (string.IsNullOrEmpty(ResponseXML))
                {
                    oreturn.ErrorCode = "ESS-103";
                    oreturn.ErrorMessage = "Empty response XML";
                    return oreturn;
                }
                XmlDocument ResponseXMLDoc = null;
                try
                {
                    ResponseXMLDoc = new XmlDocument();
                    ResponseXMLDoc.LoadXml(ResponseXML);
                }
                catch (Exception ex)
                {
                    oreturn.ErrorCode = "ESS-104";
                    oreturn.ErrorMessage = $"Unable to Parse responseXML {ex.Message}";
                    return oreturn;
                }

                if (ResponseXMLDoc == null)
                {
                    oreturn.ErrorCode = "ESS-104";
                    oreturn.ErrorMessage = "Unable to Parse responseXML";
                    return oreturn;
                }
                var SignRespElement = ResponseXMLDoc.SelectSingleNode("//EsignResp");
                if (SignRespElement == null)
                {
                    oreturn.ErrorCode = "ESS-110";
                    oreturn.ErrorMessage = "Invalid ResponseXML";
                    return oreturn;
                }
                string status = SignRespElement.Attributes["status"].Value;
                string responseCode = SignRespElement.Attributes["resCode"].Value;
                string errorMessage = string.Empty, errorCode = string.Empty;

                if (SignRespElement.Attributes["errorMessage"] != null)
                    errorMessage = SignRespElement.Attributes["errorMessage"].Value;
                else if (SignRespElement.Attributes["errMsg"] != null)
                    errorMessage = SignRespElement.Attributes["errMsg"].Value;

                if (SignRespElement.Attributes["errorCode"] != null)
                    errorCode = SignRespElement.Attributes["errorCode"].Value;
                else if (SignRespElement.Attributes["errCode"] != null)
                    errorCode = SignRespElement.Attributes["errCode"].Value;

                string txnid = SignRespElement.Attributes["txn"].Value;
                oreturn.TransactionID = txnid;
                List<eSignReturnDocument> docsToReturn = new List<eSignReturnDocument>() { };
                if (status == "0")
                {
                    oreturn.ErrorMessage = errorMessage;
                    oreturn.ErrorCode = errorCode;
                    return oreturn;

                }
                else if (status == "1")
                {
                    oreturn.ResponseCode = responseCode;
                    string PreSignedDocuments = string.Empty;
                    List<eSignReturnDocument> PreSignedReturnDocuments;
                    try
                    {
                        PreSignedDocuments = File.ReadAllText(PreSignedTempFile);
                        PreSignedReturnDocuments = eSignUtility.GetReturnDocumentsFromPreSignedPDFFile(PreSignedDocuments);
                    }
                    catch (Exception ex)
                    {
                        oreturn.ErrorCode = "ESS-107";
                        oreturn.ErrorMessage = $"Unable to get return Documents {ex.Message}";
                        return oreturn;
                    }
                    string UserCretificateNode = SignRespElement.SelectSingleNode("//UserX509Certificate").InnerText;
                    if (string.IsNullOrWhiteSpace(UserCretificateNode))
                    {
                        oreturn.ErrorCode = "ESS-118";
                        oreturn.ErrorMessage = "User Certificate not found";
                        return oreturn;
                    }
                    XmlNodeList DocumentSignatureNodes = SignRespElement.SelectNodes("//DocSignature");
                    if (DocumentSignatureNodes.Count <= 0)
                    {
                        oreturn.ErrorCode = "ESS-105";
                        oreturn.ErrorMessage = "Signature element not found";
                        return oreturn;
                    }
                    foreach (XmlNode DocumentSignature in DocumentSignatureNodes)
                    {
                        string docId = DocumentSignature.GetXMLAttributeValue("id");
                        string errorcode = DocumentSignature.GetXMLAttributeValue("errorCode");
                        string errormessage = DocumentSignature.GetXMLAttributeValue("errorMessage");
                        int DocumentID = 0;
                        if (!int.TryParse(docId, out DocumentID))
                        {
                            continue;
                        }
                        if ((!string.IsNullOrWhiteSpace(errorMessage)) || (!string.IsNullOrWhiteSpace(errorMessage)))
                        {
                            docsToReturn.Add(new eSignReturnDocument()
                            {
                                DocId = DocumentID,
                                ErrorCode = errorcode,
                                ErrorMessage = errorcode
                            });
                            continue;
                        }
                        eSignReturnDocument PreSignedDoc = PreSignedReturnDocuments.FirstOrDefault(i => i.DocId == DocumentID);
                        if (PreSignedDoc == null)
                        {
                            docsToReturn.Add(new eSignReturnDocument()
                            {
                                DocId = DocumentID,
                                ErrorCode = "ESS-111",
                                ErrorMessage = "Unable to find presigned document"
                            });
                            continue;
                        }
                        if (PreSignedDoc.DocId == 0)
                        {
                            docsToReturn.Add(PreSignedDoc);
                            continue;
                        }
                        try
                        {
                            string SignedHash = DocumentSignature.InnerText;
                            switch (PreSignedDoc.DocType)
                            {
                                case DocType.Pdf:
                                    byte[] pdf = signClose(SignedHash, PreSignedDoc.PreSignedDocument);
                                    if (PreSignedDoc.AppearanceType == AppearanceType.Aadhaar)
                                        pdf = PatchSignatureAppearance(pdf, UserCretificateNode);
                                    PreSignedDoc.SignedDocument = Convert.ToBase64String(pdf);
                                    break;
                                case DocType.Hash:
                                    PreSignedDoc.SignedHash = SignedHash.TrimStart();
                                    break;
                                case DocType.eMandate:
                                    string signedXml = GetSignedXML(SignedHash, UserCretificateNode, PreSignedDoc);
                                    PreSignedDoc.SignedXML = signedXml;
                                    break;
                            }
                            
                            PreSignedDoc.Status = eSign.status.Success;
                            docsToReturn.Add(PreSignedDoc);

                        }
                        catch (Exception)
                        {
                            PreSignedDoc.ErrorCode = "ESS-112";
                            PreSignedDoc.ErrorMessage = "Unable to get Append signature to document";
                            docsToReturn.Add(PreSignedDoc);
                        }

                    }
                    oreturn.ReturnStatus = eSign.status.Success;
                    oreturn.ReturnValues = docsToReturn;
                    return oreturn;
                }
                else
                {
                    oreturn.ErrorCode = "ESS-114";
                    oreturn.ErrorMessage = $"Invalid return status in xml {status}";
                    return oreturn;
                }
            }
            catch (Exception ex)
            {
                oreturn.ErrorCode = "ESS-999";
                oreturn.ErrorMessage = $"Something went wrong {ex.Message}";
                return oreturn;
            }
        }
        public eSignServiceReturn GetSigedDocument(string ResponseXML, byte[] PreSignedDocBytes)
        {
            eSignServiceReturn oreturn = new eSignServiceReturn();
            try
            {
                oreturn.ResponseXML = ResponseXML;
                if (string.IsNullOrEmpty(ResponseXML))
                {
                    oreturn.ErrorCode = "ESS-103";
                    oreturn.ErrorMessage = "Empty response XML";
                    return oreturn;
                }
                XmlDocument ResponseXMLDoc = null;
                try
                {
                    ResponseXMLDoc = new XmlDocument();
                    ResponseXMLDoc.LoadXml(ResponseXML);
                }
                catch (Exception ex)
                {
                    oreturn.ErrorCode = "ESS-104";
                    oreturn.ErrorMessage = $"Unable to Parse responseXML {ex.Message}";
                    return oreturn;
                }

                if (ResponseXMLDoc == null)
                {
                    oreturn.ErrorCode = "ESS-104";
                    oreturn.ErrorMessage = "Unable to Parse responseXML";
                    return oreturn;
                }
                var SignRespElement = ResponseXMLDoc.SelectSingleNode("//EsignResp");
                if (SignRespElement == null)
                {
                    oreturn.ErrorCode = "ESS-110";
                    oreturn.ErrorMessage = "Invalid ResponseXML";
                    return oreturn;
                }
                string status = SignRespElement.Attributes["status"].Value;
                string responseCode = SignRespElement.Attributes["resCode"].Value;
                string errorMessage = string.Empty, errorCode = string.Empty;

                if (SignRespElement.Attributes["errorMessage"] != null)
                    errorMessage = SignRespElement.Attributes["errorMessage"].Value;
                else if (SignRespElement.Attributes["errMsg"] != null)
                    errorMessage = SignRespElement.Attributes["errMsg"].Value;

                if (SignRespElement.Attributes["errorCode"] != null)
                    errorCode = SignRespElement.Attributes["errorCode"].Value;
                else if (SignRespElement.Attributes["errCode"] != null)
                    errorCode = SignRespElement.Attributes["errCode"].Value;

                string txnid = SignRespElement.Attributes["txn"].Value;
                oreturn.TransactionID = txnid;
                List<eSignReturnDocument> docsToReturn = new List<eSignReturnDocument>() { };
                if (status == "0")
                {
                    oreturn.ErrorMessage = errorMessage;
                    oreturn.ErrorCode = errorCode;
                    return oreturn;

                }
                else if (status == "1")
                {
                    oreturn.ResponseCode = responseCode;
                    string PreSignedDocuments = string.Empty;
                    List<eSignReturnDocument> PreSignedReturnDocuments;
                    try
                    {
                        PreSignedDocuments = Encoding.UTF8.GetString(PreSignedDocBytes);
                        PreSignedReturnDocuments = eSignUtility.GetReturnDocumentsFromPreSignedPDFFile(PreSignedDocuments);
                    }
                    catch (Exception ex)
                    {
                        oreturn.ErrorCode = "ESS-107";
                        oreturn.ErrorMessage = $"Unable to get return Documents {ex.Message}";
                        return oreturn;
                    }

                    string UserCretificateNode = string.Empty;
                    var userCertXmlNode = SignRespElement.SelectSingleNode("//UserX509Certificate");
                    if (userCertXmlNode != null)
                        UserCretificateNode = userCertXmlNode.InnerText;

                    XmlNodeList DocumentSignatureNodes = SignRespElement.SelectNodes("//DocSignature");
                    if (DocumentSignatureNodes.Count <= 0)
                    {
                        oreturn.ErrorCode = "ESS-105";
                        oreturn.ErrorMessage = "Signature element not found";
                        return oreturn;
                    }
                    foreach (XmlNode DocumentSignature in DocumentSignatureNodes)
                    {
                        string docId = DocumentSignature.GetXMLAttributeValue("id");
                        string errorcode = DocumentSignature.GetXMLAttributeValue("errorCode");
                        string errormessage = DocumentSignature.GetXMLAttributeValue("errorMessage");

                        int DocumentID = 0;
                        if (!int.TryParse(docId, out DocumentID))
                        {
                            continue;
                        }
                        if ((!string.IsNullOrWhiteSpace(errorMessage)) || (!string.IsNullOrWhiteSpace(errorMessage)))
                        {
                            docsToReturn.Add(new eSignReturnDocument()
                            {
                                DocId = DocumentID,
                                ErrorCode = errorcode,
                                ErrorMessage = errorcode
                            });
                            continue;
                        }
                        eSignReturnDocument PreSignedDoc = PreSignedReturnDocuments.FirstOrDefault(i => i.DocId == DocumentID);
                        if (PreSignedDoc == null)
                        {
                            docsToReturn.Add(new eSignReturnDocument()
                            {
                                DocId = DocumentID,
                                ErrorCode = "ESS-111",
                                ErrorMessage = "Unable to find PreSigned DocBytes"
                            });
                            continue;
                        }
                        if (PreSignedDoc.DocId == 0)
                        {
                            docsToReturn.Add(PreSignedDoc);
                            continue;
                        }
                        try
                        {
                            string SignedHash = DocumentSignature.InnerText;
                            if (PreSignedDoc.DocType == DocType.Hash)
                            {
                                PreSignedDoc.SignedHash = SignedHash.TrimStart();
                            }
                            else
                            {
                                byte[] pdf = signClose(SignedHash, PreSignedDoc.PreSignedDocument);
                                if (PreSignedDoc.AppearanceType == AppearanceType.Aadhaar)
                                    pdf = PatchSignatureAppearance(pdf, UserCretificateNode);
                                PreSignedDoc.SignedDocument = Convert.ToBase64String(pdf);
                            }
                            PreSignedDoc.Status = eSign.status.Success;
                            docsToReturn.Add(PreSignedDoc);
                        }
                        catch (Exception)
                        {
                            PreSignedDoc.ErrorCode = "ESS-112";
                            PreSignedDoc.ErrorMessage = "Unable to get Append signature to document";
                            docsToReturn.Add(PreSignedDoc);
                        }

                    }
                    oreturn.ReturnStatus = eSign.status.Success;
                    oreturn.ReturnValues = docsToReturn;
                    return oreturn;
                }
                else
                {
                    oreturn.ErrorCode = "ESS-114";
                    oreturn.ErrorMessage = $"Invalid return status in xml {status}";
                    return oreturn;
                }
            }
            catch (Exception ex)
            {
                oreturn.ErrorCode = "ESS-999";
                oreturn.ErrorMessage = $"Something went wrong {ex.Message}";
                return oreturn;
            }
        }
        public eSignServiceReturn GetStatus(string transactionID)
        {
            eSignServiceReturn oreturn = new eSignServiceReturn();
            oreturn.TransactionID = transactionID;
            try
            {
                if (string.IsNullOrWhiteSpace(transactionID))
                {
                    oreturn.ErrorCode = "ESS-115";
                    oreturn.ErrorMessage = $"Transaction must be provided.";
                    return oreturn;
                }
                string ASPID = eSignSettings.ASPID;
                oreturn.ASPID = ASPID;
                string Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                string RequestXML = eSignUtility.GetStatusXML(ASPID, Timestamp, transactionID);
                string SignedRequestXML = eSignUtility.SignXML(RequestXML);
                oreturn.RequestXML = SignedRequestXML;
                string URLEncodedSignedXML = HttpUtility.UrlEncode(SignedRequestXML);
                string ResponseXML = string.Empty;
                try
                {
                    ResponseXML = eSignUtility.HttpsWebClientSendRequest(eSignSettings.eSignCheckStatusURL, URLEncodedSignedXML);
                    oreturn.ResponseXML = ResponseXML;
                }
                catch (WebException ex)
                {
                    oreturn.ErrorCode = "ESS-108";
                    oreturn.ErrorMessage = $"Web exception while calling eSign API - {ex.Message}";
                    return oreturn;
                }
                if (string.IsNullOrWhiteSpace(ResponseXML))
                {
                    oreturn.ErrorCode = "ESS-109";
                    oreturn.ErrorMessage = "Empty response XML from eSign API.";
                    return oreturn;
                }
                oreturn.ResponseXML = ResponseXML;
                XmlDocument ResponseXMLDoc = new XmlDocument();
                ResponseXMLDoc.LoadXml(ResponseXML);
                var SignRespElement = ResponseXMLDoc.SelectSingleNode("//EsignResp");
                if (SignRespElement == null)
                {
                    oreturn.ErrorCode = "ESS-110";
                    oreturn.ErrorMessage = "Invalid ResponseXML";
                    return oreturn;
                }
                string status = SignRespElement.Attributes["status"].Value;
                string errorMessage = SignRespElement.Attributes["errorMessage"].Value;
                string errorCode = SignRespElement.Attributes["errorCode"].Value;
                if (status == "0")
                {
                    oreturn.ReturnStatus = eSign.status.Failure;
                    oreturn.ErrorMessage = errorMessage;
                    oreturn.ErrorCode = errorCode;
                    return oreturn;
                }
                else if (status == "1")
                {
                    oreturn.ReturnStatus = eSign.status.Success;
                    return oreturn;
                }
                else if (status == "2")
                {
                    oreturn.ReturnStatus = eSign.status.Pending;
                    return oreturn;
                }
                return oreturn;
            }
            catch (Exception ex)
            {
                oreturn.ErrorCode = "ESS-999";
                oreturn.ErrorMessage = $"Something went wrong {ex.Message}";
                return oreturn;
            }
        }

        private static void DrawPreSignBackground(
            PdfTemplate tpl, float w, float h, string signDate, string reason, string location)
        {
            BaseFont font     = BaseFont.CreateFont(BaseFont.HELVETICA,      BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            BaseFont boldFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            tpl.SetColorFill(new BaseColor(240, 245, 255));
            tpl.Rectangle(0, 0, w, h);
            tpl.Fill();

            tpl.SetColorStroke(new BaseColor(30, 80, 160));
            tpl.SetLineWidth(1f);
            tpl.Rectangle(1, 1, w - 2, h - 2);
            tpl.Stroke();

            float x = 6f;
            float y = h - 14f;

            tpl.SetColorFill(BaseColor.BLACK);
            tpl.BeginText();
            tpl.SetFontAndSize(boldFont, 8f);
            tpl.SetTextMatrix(x, y);
            tpl.ShowText("DIGITALLY SIGNED BY");
            tpl.EndText();

            // name row is reserved for the TextField overlay — skip 13pt (title) + 12pt (name)
            y -= 25f;

            tpl.BeginText();
            tpl.SetFontAndSize(font, 7f);
            tpl.SetColorFill(new BaseColor(64, 64, 64));
            tpl.SetTextMatrix(x, y);
            tpl.ShowText("Date: " + signDate);
            tpl.EndText();
            y -= 11f;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                tpl.BeginText();
                tpl.SetFontAndSize(font, 7f);
                tpl.SetTextMatrix(x, y);
                tpl.ShowText("Reason: " + reason);
                tpl.EndText();
                y -= 10f;
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                tpl.BeginText();
                tpl.SetFontAndSize(font, 7f);
                tpl.SetTextMatrix(x, y);
                tpl.ShowText("Location: " + location);
                tpl.EndText();
            }
        }

        private static byte[] FillSignerNameField(byte[] signedPdfBytes, string userX509CertBase64, int docId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userX509CertBase64))
                    return signedPdfBytes;

                var subjectDN = new emCastle.X509.X509CertificateParser()
                    .ReadCertificate(Convert.FromBase64String(userX509CertBase64)).SubjectDN;
                var cnList = subjectDN.GetValueList(new emCastle.Asn1.DerObjectIdentifier("2.5.4.3"));
                string certName = (cnList != null && cnList.Count > 0) ? cnList[0].ToString() : "Unknown";

                using (MemoryStream inputMs = new MemoryStream(signedPdfBytes))
                using (MemoryStream outputMs = new MemoryStream())
                {
                    PdfReader  reader  = new PdfReader(inputMs);
                    PdfStamper stamper = new PdfStamper(reader, outputMs, '\0', true);

                    AcroFields fields = stamper.AcroFields;
                    fields.GenerateAppearances = true;

                    string prefix   = "SN_" + docId + "_";
                    bool   anyFilled = false;
                    foreach (string fn in new List<string>(fields.Fields.Keys))
                    {
                        if (fn.StartsWith(prefix))
                        {
                            fields.SetField(fn, certName);
                            anyFilled = true;
                        }
                    }

                    stamper.Close();
                    reader.Close();
                    return anyFilled ? outputMs.ToArray() : signedPdfBytes;
                }
            }
            catch
            {
                return signedPdfBytes;
            }
        }

        private static byte[] PatchSignatureAppearance(byte[] signedPdfBytes, string userX509CertBase64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userX509CertBase64))
                    return signedPdfBytes;

                var subjectDN = new emCastle.X509.X509CertificateParser()
                    .ReadCertificate(Convert.FromBase64String(userX509CertBase64)).SubjectDN;

                var cnList = subjectDN.GetValueList(new emCastle.Asn1.DerObjectIdentifier("2.5.4.3"));
                string certName = (cnList != null && cnList.Count > 0)
                    ? cnList[0].ToString() : "Unknown";

                // OID 2.5.4.12 = Title field; eMudhra stores masked Aadhaar here
                string aadhaarLast4 = "XXXX";
                try
                {
                    var aaList = subjectDN.GetValueList(new emCastle.Asn1.DerObjectIdentifier("2.5.4.12"));
                    if (aaList != null && aaList.Count > 0)
                    {
                        string val = aaList[0].ToString();
                        if (val.Length >= 4)
                            aadhaarLast4 = val.Substring(val.Length - 4);
                    }
                }
                catch { }

                using (MemoryStream inputMs = new MemoryStream(signedPdfBytes))
                using (MemoryStream outputMs = new MemoryStream())
                {
                    PdfReader reader   = new PdfReader(inputMs);
                    PdfStamper stamper = new PdfStamper(reader, outputMs, '\0', true); // append mode

                    AcroFields readerFields = reader.AcroFields;
                    IList<string> sigNames  = readerFields.GetSignatureNames();
                    if (sigNames.Count == 0)
                    {
                        stamper.Close();
                        reader.Close();
                        return signedPdfBytes;
                    }

                    // --- Build font object once; shared across all signature fields ---
                    PdfDictionary fontObj = new PdfDictionary();
                    fontObj.Put(PdfName.TYPE, PdfName.FONT);
                    fontObj.Put(PdfName.SUBTYPE, new PdfName("Type1"));
                    fontObj.Put(PdfName.BASEFONT, new PdfName("Times-Italic"));
                    fontObj.Put(new PdfName("Encoding"), new PdfName("WinAnsiEncoding"));
                    PdfIndirectObject fontRef = stamper.Writer.AddToBody(fontObj);

                    PdfDictionary fontResources = new PdfDictionary();
                    fontResources.Put(new PdfName("F1"), fontRef.IndirectReference);
                    PdfDictionary resDict = new PdfDictionary();
                    resDict.Put(PdfName.FONT, fontResources);

                    // Patch appearance for every signature field in the document
                    foreach (string sigFieldName in sigNames)
                    {
                        AcroFields.Item item = readerFields.GetFieldItem(sigFieldName);
                        if (item == null) continue;
                        PdfDictionary widget = item.GetWidget(0);
                        if (widget == null) continue;
                        Rectangle rect = PdfReader.GetNormalizedRectangle(
                            widget.GetAsArray(PdfName.RECT));

                        // Read signing date, reason, location from this field's sig dict
                        string signDate = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                        string reason   = string.Empty, location = string.Empty;
                        try
                        {
                            PdfDictionary sigDict =
                                PdfReader.GetPdfObject(widget.Get(PdfName.V)) as PdfDictionary;
                            if (sigDict != null)
                            {
                                PdfString dateStr = sigDict.GetAsString(PdfName.M);
                                if (dateStr != null)
                                    try { signDate = PdfDate.Decode(dateStr.ToString())
                                            .ToString("dd-MMM-yyyy HH:mm:ss"); } catch { }
                                PdfString rs = sigDict.GetAsString(PdfName.REASON);
                                PdfString ls = sigDict.GetAsString(PdfName.LOCATION);
                                if (rs != null) reason   = rs.ToUnicodeString();
                                if (ls != null) location = ls.ToUnicodeString();
                            }
                        }
                        catch { }

                        // Build text content stream for this field
                        var lines = new System.Collections.Generic.List<string>();
                        lines.Add("Digitally Signed by");
                        lines.Add("Name : " + certName);
                        lines.Add("Aadhaar No : **** **** " + aadhaarLast4);
                        if (!string.IsNullOrWhiteSpace(reason))
                            lines.Add("Reason: " + reason);
                        lines.Add("Date : " + signDate);

                        float startY = rect.Height - 10f;
                        var cs = new StringBuilder();
                        cs.Append("BT\n");
                        cs.Append("/F1 8 Tf\n");
                        cs.Append("/DeviceRGB cs\n0 0 0 sc\n");
                        cs.Append("8 " + startY.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " Td\n");
                        cs.Append("8 TL\n");
                        for (int li = 0; li < lines.Count; li++)
                        {
                            string escaped = lines[li]
                                .Replace("\\", "\\\\")
                                .Replace("(", "\\(")
                                .Replace(")", "\\)");
                            cs.Append(li < lines.Count - 1
                                ? "(" + escaped + ") Tj T*\n"
                                : "(" + escaped + ") Tj\n");
                        }
                        cs.Append("ET");

                        byte[] streamBytes = System.Text.Encoding.GetEncoding(1252).GetBytes(cs.ToString());

                        // Form XObject with BBox, Resources, content stream
                        PdfStream apStream = new PdfStream(streamBytes);
                        apStream.Put(PdfName.TYPE, PdfName.XOBJECT);
                        apStream.Put(PdfName.SUBTYPE, PdfName.FORM);
                        apStream.Put(PdfName.BBOX,
                            new PdfArray(new[] { 0f, 0f, rect.Width, rect.Height }));
                        apStream.Put(PdfName.RESOURCES, resDict);
                        PdfIndirectObject apRef = stamper.Writer.AddToBody(apStream);

                        // Update widget's /AP /N → new Form XObject
                        PdfDictionary newAp = new PdfDictionary();
                        newAp.Put(PdfName.N, apRef.IndirectReference);
                        widget.Put(PdfName.AP, newAp);

                        // Mark widget modified → written into incremental revision
                        stamper.MarkUsed(widget);
                    }

                    stamper.Close();
                    reader.Close();

                    return outputMs.ToArray();
                }
            }
            catch
            {
                return signedPdfBytes;
            }
        }

        private static void DrawSignatureAppearance(
            PdfAppearance app, float w, float h,
            string certName, string aadhaarLast4,
            string signDate, string reason, string location)
        {
            BaseFont font     = BaseFont.CreateFont(BaseFont.HELVETICA,
                                    BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            BaseFont boldFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD,
                                    BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            // Background
            app.SetColorFill(new BaseColor(240, 245, 255));
            app.Rectangle(0, 0, w, h);
            app.Fill();

            // Border
            app.SetColorStroke(new BaseColor(30, 80, 160));
            app.SetLineWidth(1f);
            app.Rectangle(1, 1, w - 2, h - 2);
            app.Stroke();

            float x = 6f;
            float y = h - 14f;

            // Title
            app.SetColorFill(BaseColor.BLACK);
            app.BeginText();
            app.SetFontAndSize(boldFont, 8f);
            app.SetTextMatrix(x, y);
            app.ShowText("DIGITALLY SIGNED BY");
            app.EndText();
            y -= 13f;

            // Signer name
            app.BeginText();
            app.SetFontAndSize(boldFont, 10f);
            app.SetColorFill(new BaseColor(10, 60, 140));
            app.SetTextMatrix(x, y);
            app.ShowText(certName);
            app.EndText();
            y -= 12f;

            // Aadhaar last 4
            app.BeginText();
            app.SetFontAndSize(font, 7.5f);
            app.SetColorFill(new BaseColor(64, 64, 64));
            app.SetTextMatrix(x, y);
            app.ShowText("Aadhaar: XXXX-XXXX-" + aadhaarLast4);
            app.EndText();
            y -= 11f;

            // Date
            app.BeginText();
            app.SetFontAndSize(font, 7f);
            app.SetTextMatrix(x, y);
            app.ShowText("Date: " + signDate);
            app.EndText();
            y -= 10f;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                app.BeginText();
                app.SetFontAndSize(font, 7f);
                app.SetTextMatrix(x, y);
                app.ShowText("Reason: " + reason);
                app.EndText();
                y -= 10f;
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                app.BeginText();
                app.SetFontAndSize(font, 7f);
                app.SetTextMatrix(x, y);
                app.ShowText("Location: " + location);
                app.EndText();
            }
        }

        private static byte[] signClose(string pkcs7, string preSignedValue)
        {
            try
            {
                byte[] preSignedBytes = Base64.Decode(preSignedValue);
                string preSignedDoc = Encoding.UTF8.GetString(preSignedBytes, 0, preSignedBytes.Length);
                MemoryStream originalout = new MemoryStream();
                string[] Doc = preSignedDoc.Split('|');
                byte[] sigbytes = Base64.Decode(pkcs7);
                //byte[] paddedSig = new byte[16384];
                byte[] paddedSig = new byte[eSignSettings.SignatureContents];
                Array.Copy(sigbytes, 0, paddedSig, 0, sigbytes.Length);
                PdfDictionary dic2 = new PdfDictionary();
                dic2.Put(PdfName.CONTENTS, new PdfString(paddedSig).SetHexWriting(true));

                byte[] bout = Convert.FromBase64String(Doc[2]); //Base64.Decode(Doc[2]); // index out of bound exception
                int boutLen = Convert.ToInt32(Doc[1]);
                int position = Convert.ToInt32(Doc[0]);

                //Calculate exclusionLocations
                Dictionary<PdfName, int> exclusionSizes = new Dictionary<PdfName, int>();
                //exclusionSizes.Add(PdfName.CONTENTS, 16384 * 2 + 2);
                exclusionSizes.Add(PdfName.CONTENTS, eSignSettings.SignatureContents * 2 + 2);
                Dictionary<PdfName, PdfLiteral> exclusionLocations = new Dictionary<PdfName, PdfLiteral>();
                PdfLiteral litEL = new PdfLiteral(80);
                exclusionLocations.Add(PdfName.BYTERANGE, litEL);
                foreach (KeyValuePair<PdfName, int> entry in exclusionSizes)
                {
                    PdfName key = entry.Key;
                    int v = entry.Value;
                    litEL = new PdfLiteral(v);
                    exclusionLocations.Add(key, litEL);
                }
                PdfLiteral temp;
                exclusionLocations.TryGetValue(PdfName.CONTENT, out temp);
                litEL.Position = position;
                //litEL.Position =  exclusionLocations.Where(i => i.Key == PdfName.CONTENTS).To
                exclusionLocations.Remove(PdfName.BYTERANGE);
                ByteBuffer bf = new ByteBuffer();
                foreach (PdfName key in dic2.Keys)
                {
                    PdfObject obj = dic2.Get(key);
                    PdfLiteral lit = new PdfLiteral(1);
                    exclusionLocations.TryGetValue(key, out lit);
                    if (lit == null)
                    {
                        throw new Exception("the.key.1.is.too.big.is.2.reserved.3");
                    }
                    bf.Reset();
                    obj.ToPdf(null, bf);
                    if (bf.Size > lit.PosLength)
                    {
                        throw new Exception("the.key.1.is.too.big.is.2.reserved.3");
                    }
                    Array.Copy(bf.Buffer, 0, bout, lit.Position, bf.Size);
                }
                if (dic2.Size != exclusionLocations.Count)
                {
                    throw new Exception("the.update.dictionary.has.less.keys.than.required");
                }
                originalout.Write(bout, 0, boutLen);
                return originalout.ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static string GetSignedXML(string pkcs7, string X509, eSignReturnDocument PreSignedDoc)
        {
            string CanocnicalizedXML = Encoding.UTF8.GetString(Convert.FromBase64String(PreSignedDoc.PreSignedDocument));

            //Document load
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(CanocnicalizedXML);

            var cert = (new emCastle.X509.X509CertificateParser().ReadCertificate(Convert.FromBase64String(X509))).SubjectDN;
            string AadhaarNumber = "00000000" + cert.GetValueList(new emCastle.Asn1.DerObjectIdentifier("2.5.4.12"))[0];

            //namespace
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("ns", "urn:iso:std:iso:20022:tech:xsd:pain.009.001.04");

            //Adding Aadhaar Number
            var CtctDtlsNode = doc.SelectSingleNode("//ns:CtctDtls", nsManager);
            var aadharNode = doc.CreateElement("Nm", doc.DocumentElement.NamespaceURI);
            aadharNode.InnerText = AadhaarNumber;
            CtctDtlsNode.InsertBefore(aadharNode, CtctDtlsNode.FirstChild);



            // Create the new XML node to append
            XmlNode newNode = doc.CreateElement("SplmtryData", doc.DocumentElement.NamespaceURI);
            XmlNode envlpNode = doc.CreateElement("Envlp", doc.DocumentElement.NamespaceURI);
            newNode.AppendChild(envlpNode);
            XmlNode cntsNode = doc.CreateElement("Cnts", doc.DocumentElement.NamespaceURI);
            envlpNode.AppendChild(cntsNode);
            XmlNode esignNode = doc.CreateElement("esign", doc.DocumentElement.NamespaceURI);
            cntsNode.AppendChild(esignNode);
            XmlNode signedContentNode = doc.CreateElement("SignedContent", doc.DocumentElement.NamespaceURI);
            signedContentNode.InnerText = PreSignedDoc.PreSignedDocument;
            esignNode.AppendChild(signedContentNode);
            XmlNode x509CertificateNode = doc.CreateElement("X509Certificate", doc.DocumentElement.NamespaceURI);
            x509CertificateNode.InnerText = Regex.Replace(X509, @"\s+", ""); ;
            esignNode.AppendChild(x509CertificateNode);
            XmlNode signatureNode = doc.CreateElement("Signature", doc.DocumentElement.NamespaceURI);
            signatureNode.InnerText = Regex.Replace(pkcs7, @"\s+", "");
            esignNode.AppendChild(signatureNode);


            // Append the new XML node after the </Mndt> node
            XmlNode parentNode = doc.SelectSingleNode("//ns:Mndt", nsManager);
            parentNode.AppendChild(newNode);

            return doc.OuterXml;
        }
    }
}

