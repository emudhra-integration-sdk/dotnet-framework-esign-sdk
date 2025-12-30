using System;

namespace eSignASPLibrary
{
    /// <summary>
    /// Represents the result of a PDF text search operation.
    /// Contains the location where text was found and calculated signature coordinates.
    /// </summary>
    public class PdfTextSearchResult
    {
        /// <summary>
        /// Gets or sets whether the search was successful.
        /// </summary>
        public bool Found { get; set; }

        /// <summary>
        /// Gets or sets the page number where the text was found (1-based index).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate where the text was found.
        /// </summary>
        public float TextX { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate where the text was found.
        /// </summary>
        public float TextY { get; set; }

        /// <summary>
        /// Gets or sets the width of the found text.
        /// </summary>
        public float TextWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the found text.
        /// </summary>
        public float TextHeight { get; set; }

        /// <summary>
        /// Gets or sets the calculated X1 coordinate for signature placement.
        /// </summary>
        public int SignatureX1 { get; set; }

        /// <summary>
        /// Gets or sets the calculated Y1 coordinate for signature placement.
        /// </summary>
        public int SignatureY1 { get; set; }

        /// <summary>
        /// Gets or sets the calculated X2 coordinate for signature placement.
        /// </summary>
        public int SignatureX2 { get; set; }

        /// <summary>
        /// Gets or sets the calculated Y2 coordinate for signature placement.
        /// </summary>
        public int SignatureY2 { get; set; }

        /// <summary>
        /// Gets or sets the coordinate string in format "x1,y1,x2,y2".
        /// </summary>
        public string CoordinateString => $"{SignatureX1},{SignatureY1},{SignatureX2},{SignatureY2}";

        /// <summary>
        /// Gets or sets the error message if the search failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the total number of matches found.
        /// </summary>
        public int TotalMatches { get; set; }

        /// <summary>
        /// Gets or sets the actual text that was found.
        /// </summary>
        public string FoundText { get; set; }

        /// <summary>
        /// Gets or sets all match details for multi-occurrence scenarios.
        /// Each entry contains page number and signature coordinates.
        /// </summary>
        public System.Collections.Generic.List<MatchDetail> AllMatches { get; set; }
    }

    /// <summary>
    /// Represents details of a single text match with calculated signature coordinates.
    /// </summary>
    public class MatchDetail
    {
        /// <summary>
        /// Gets or sets the page number where this match was found (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate of the found text.
        /// </summary>
        public float TextX { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the found text.
        /// </summary>
        public float TextY { get; set; }

        /// <summary>
        /// Gets or sets the calculated signature X1 coordinate.
        /// </summary>
        public int SignatureX1 { get; set; }

        /// <summary>
        /// Gets or sets the calculated signature Y1 coordinate.
        /// </summary>
        public int SignatureY1 { get; set; }

        /// <summary>
        /// Gets or sets the calculated signature X2 coordinate.
        /// </summary>
        public int SignatureX2 { get; set; }

        /// <summary>
        /// Gets or sets the calculated signature Y2 coordinate.
        /// </summary>
        public int SignatureY2 { get; set; }

        /// <summary>
        /// Gets the coordinate string in format "x1,y1,x2,y2".
        /// </summary>
        public string CoordinateString => $"{SignatureX1},{SignatureY1},{SignatureX2},{SignatureY2}";

        /// <summary>
        /// Gets the page-level coordinate string in format "page,x1,y1,x2,y2".
        /// </summary>
        public string PageLevelCoordinateString => $"{PageNumber},{SignatureX1},{SignatureY1},{SignatureX2},{SignatureY2}";
    }
}
