using System.Collections.Generic;
namespace eSignASPLibrary
{
    public class eSignServiceReturn
    {
        public string TransactionID { get; internal set; }
        public string PreSignedTempFile { get; internal set; }
        public string RequestXML { get; internal set; }
        public string ResponseXML { get; internal set; }
        public string ResponseCode { get; internal set; }
        public string GatewayParameter { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string ErrorCode { get; internal set; }
        public string ASPID { get; internal set; }
        public eSign.status ReturnStatus { get; internal set; }
        public List<eSignReturnDocument> ReturnValues { get; internal set; }
        public byte[] PreSignedDocBytes { get; internal set; }
    }
}
