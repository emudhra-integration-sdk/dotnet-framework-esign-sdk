namespace eSignASPLibrary
{
    public class eSignInput
    {
        public string DocBase64 { get; }
        public string SignedBy { get; }
        public string Location { get; }
        public string Reason { get; }
        public bool CoSign { get; }
        public bool RequiredGreenTick { get; }
        public bool RequiredValidMessage { get; }
        public eSign.PageToBeSigned PageTobeSigned { get; }
        public eSign.Coordinates Coordinates { get; }
        public eSign.DocType docType { get; }
        public string PageNumbers { get; }
        public int FontSize { get; }
        public string PageLevelCoordinates { get; }
        public string CustomCoordinates { get; }
        public string PdfUrl { get; }
        public string DocInfo { get; }
        public string appearanceText { get; set; }
        public eSignInput(string xmlToSign, string docInfo, string docURL)
            : this(xmlToSign, docInfo, docURL, "", "", "", true, eSign.PageToBeSigned.PAGE_LEVEL, eSign.Coordinates.Bottom_Left, "", "", "", "", eSign.DocType.eMandate, true, true, 8) { }
        public eSignInput(string DocHash, string docInfo, string docURL, eSign.DocType docType)
       : this(DocHash, docInfo, docURL, "", "", "", true, eSign.PageToBeSigned.PAGE_LEVEL, eSign.Coordinates.Bottom_Left, "", "", "", "", docType, true, true, 8) { }
        public eSignInput(string DocBase64, string docInfo, string docURL, string Location, string Reason, string SignedBy, bool CoSign, eSign.PageToBeSigned Page, eSign.Coordinates Coordinates, string appearanceText, bool requiredGreenTick = true, bool requiredValidMessage = true, int fontsize = 8)
        : this(DocBase64, docInfo, docURL, Location, Reason, SignedBy, CoSign, Page, Coordinates, "", "", "", appearanceText, eSign.DocType.Pdf, requiredGreenTick, requiredValidMessage, fontsize) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DocBase64"></param>
        /// <param name="docInfo"></param>
        /// <param name="docURL"></param>
        /// <param name="Location"></param>
        /// <param name="Reason"></param>
        /// <param name="SignedBy"></param>
        /// <param name="CoSign"></param>
        /// <param name="Page"></param>
        /// <param name="Coordinates">Coordinates should contain only 4 Values 
        /// Ex:- "50,47,180,87"</param>
        /// <param name="appearanceText"></param>
        public eSignInput(string DocBase64, string docInfo, string docURL, string Location, string Reason, string SignedBy, bool CoSign, eSign.PageToBeSigned Page, string Coordinates, string appearanceText, bool requiredGreenTick = true, bool requiredValidMessage = true, int fontsize = 8)
       : this(DocBase64, docInfo, docURL, Location, Reason, SignedBy, CoSign, Page, eSign.Coordinates.Top_Left, "", "", Coordinates, appearanceText, eSign.DocType.Pdf, requiredGreenTick, requiredValidMessage, fontsize) { }

        public eSignInput(string DocBase64, string docInfo, string docURL, string Location, string Reason, string SignedBy, bool CoSign, eSign.Coordinates Coordinates, string PageNumbers, string appearanceText, bool requiredGreenTick = true, bool requiredValidMessage = true, int fontsize = 8)
        : this(DocBase64, docInfo, docURL, Location, Reason, SignedBy, CoSign, eSign.PageToBeSigned.SPECIFY, Coordinates, PageNumbers, "", "", appearanceText, eSign.DocType.Pdf, requiredGreenTick, requiredValidMessage, fontsize) { }

        public eSignInput(string DocBase64, string docInfo, string docURL, string Location, string Reason, string SignedBy, bool CoSign, string PageLevelCoordinates, string appearanceText, bool requiredGreenTick = true, bool requiredValidMessage = true, int fontsize = 8)
        : this(DocBase64, docInfo, docURL, Location, Reason, SignedBy, CoSign, eSign.PageToBeSigned.PAGE_LEVEL, eSign.Coordinates.Top_Left, "", PageLevelCoordinates, "", appearanceText, eSign.DocType.Pdf, requiredGreenTick, requiredValidMessage, fontsize) { }

        private eSignInput(string DocBase64, string DocInfo, string PdfUrl, string Location, string Reason, string SignedBy, bool CoSign, eSign.PageToBeSigned PageTobeSigned, eSign.Coordinates Coordinates,
            string PageNumbers, string PageLevelCoordinates, string customCoordinates, string appearanceText, eSign.DocType docType, bool requiredGreenTick, bool requiredValidMessage, int fontsize)
        {
            this.CoSign = CoSign;
            this.Coordinates = Coordinates;
            this.DocBase64 = DocBase64;
            this.Location = Location;
            this.PageTobeSigned = PageTobeSigned;
            this.PageLevelCoordinates = PageLevelCoordinates;
            this.CustomCoordinates = customCoordinates;
            this.PageNumbers = PageNumbers;
            this.Reason = Reason;
            this.SignedBy = SignedBy;
            this.DocInfo = DocInfo;
            this.PdfUrl = PdfUrl;
            this.appearanceText = appearanceText;
            this.docType = docType;
            this.RequiredGreenTick = requiredGreenTick;
            this.RequiredValidMessage = requiredValidMessage;
            this.FontSize = fontsize;
        }
    }
}
