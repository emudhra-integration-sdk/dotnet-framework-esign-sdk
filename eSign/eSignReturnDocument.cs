using eSignLibrary.text.pdf;
using System;
using System.Text;

namespace eSignASPLibrary
{
    public class eSignReturnDocument
    {
        public string SignedDocument { get; internal set; }
        public string SignedXML { get; internal set; }
        public string DocumentHash { get; internal set; }
        internal PdfSignatureAppearance SignatureAppreance { get; set; }
        public int DocId { get; internal set; }
        public string TempFilePath { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string ErrorCode { get; internal set; }
        public eSign.status Status { get; internal set; }
        public string DocumentURL { get; internal set; }
        public string DocumentInfo { get; internal set; }
        public string PreSignedDocument { get; internal set; }
        public eSign.DocType DocType { get; internal set; }
        public string SignedHash { get; internal set; }

        public eSignReturnDocument() { }
        public eSignReturnDocument(string returnDocumentBase64)
        {
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(returnDocumentBase64);
                string returnDocument = Encoding.UTF8.GetString(decodedBytes);
                string[] returnDocumentValues = returnDocument.Split('|');
                if (returnDocumentValues.Length != 6)
                {
                    throw new ArgumentException("invalid return Document");
                }
                int documentId = 0;
                int.TryParse(returnDocumentValues[0], out documentId);
                DocId = documentId;
                DocumentInfo = returnDocumentValues[1];
                DocumentURL = returnDocumentValues[2];
                DocumentHash = returnDocumentValues[3];
                //DocType = returnDocumentValues[5] == "Pdf" ? eSign.DocType.Pdf : eSign.DocType.Hash;
                //eSign.DocType parsedDocType;
                //if(Enum.TryParse(returnDocumentValues[5], out parsedDocType))
                //{

                //}
                DocType = (eSign.DocType)Enum.Parse(typeof(eSign.DocType), returnDocumentValues[5], true);
                PreSignedDocument = returnDocumentValues[4];
                SignedDocument = "";
            }
            catch (Exception e)
            {
                Status = 0;
                DocId = 0;
                PreSignedDocument = returnDocumentBase64;
                ErrorMessage = "Unable to create Return Document - " + e.Message;
            }
        }
        public eSignReturnDocument(eSign.status status, string errorMessage, string errorCode, int docId)
        {
            Status = status;
            DocId = docId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
        public string GetReturnDocumentBase64()
        {
            string returnDocument = $"{DocId}|{DocumentInfo}|{DocumentURL}|{DocumentHash}|{PreSignedDocument}|{DocType}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(returnDocument));
        }
    }
}

