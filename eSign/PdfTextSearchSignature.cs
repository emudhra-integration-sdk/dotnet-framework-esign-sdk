using System;

namespace eSignASPLibrary
{
    /// <summary>
    /// Configuration for signature placement based on PDF text search.
    /// Allows dynamic signature positioning by searching for specific text in the PDF.
    /// </summary>
    /// <example>
    /// <code>
    /// var searchConfig = new PdfTextSearchSignature
    /// {
    ///     SearchText = "Sign here:",
    ///     SignatureWidth = 150,
    ///     SignatureHeight = 50,
    ///     XOffset = 10,
    ///     YOffset = -5
    /// };
    /// </code>
    /// </example>
    public class PdfTextSearchSignature
    {
        /// <summary>
        /// Gets or sets the text to search for in the PDF document.
        /// </summary>
        /// <remarks>
        /// The search is case-sensitive by default. Use IgnoreCase property to modify this behavior.
        /// </remarks>
        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the width of the signature box in points.
        /// </summary>
        /// <remarks>
        /// Default value is 150 points (~2 inches at 72 DPI).
        /// </remarks>
        public int SignatureWidth { get; set; } = 150;

        /// <summary>
        /// Gets or sets the height of the signature box in points.
        /// </summary>
        /// <remarks>
        /// Default value is 60 points (~0.83 inches at 72 DPI).
        /// </remarks>
        public int SignatureHeight { get; set; } = 60;

        /// <summary>
        /// Gets or sets the horizontal offset from the found text position in points.
        /// </summary>
        /// <remarks>
        /// Positive values move the signature to the right, negative values move it to the left.
        /// Default is 0 (signature starts at the end of the found text).
        /// </remarks>
        public int XOffset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the vertical offset from the found text position in points.
        /// </summary>
        /// <remarks>
        /// Positive values move the signature up, negative values move it down.
        /// Default is 0 (signature aligns with the baseline of the found text).
        /// </remarks>
        public int YOffset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the specific page number to search (1-based index).
        /// </summary>
        /// <remarks>
        /// If null or 0, searches all pages. If specified, only searches that page.
        /// </remarks>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Gets or sets which occurrence to use if the text appears multiple times (0-based index).
        /// </summary>
        /// <remarks>
        /// 0 = first occurrence, 1 = second occurrence, etc.
        /// If the specified index doesn't exist, uses the first occurrence.
        /// Default is 0 (first occurrence).
        /// </remarks>
        public int MatchIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether the text search should ignore case.
        /// </summary>
        /// <remarks>
        /// Default is false (case-sensitive search).
        /// </remarks>
        public bool IgnoreCase { get; set; } = false;

        /// <summary>
        /// Gets or sets the placement strategy relative to the found text.
        /// </summary>
        /// <remarks>
        /// Determines where the signature should be positioned relative to the found text.
        /// </remarks>
        public SignaturePlacement Placement { get; set; } = SignaturePlacement.RightOf;

        /// <summary>
        /// Validates the search configuration.
        /// </summary>
        /// <param name="errorMessage">Contains error message if validation fails.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                errorMessage = "SearchText cannot be null or empty.";
                return false;
            }

            if (SignatureWidth <= 0)
            {
                errorMessage = "SignatureWidth must be greater than 0.";
                return false;
            }

            if (SignatureHeight <= 0)
            {
                errorMessage = "SignatureHeight must be greater than 0.";
                return false;
            }

            if (MatchIndex < 0)
            {
                errorMessage = "MatchIndex must be 0 or greater.";
                return false;
            }

            if (PageNumber.HasValue && PageNumber.Value <= 0)
            {
                errorMessage = "PageNumber must be greater than 0.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Defines signature placement strategies relative to the found text.
    /// </summary>
    public enum SignaturePlacement
    {
        /// <summary>
        /// Place signature to the right of the found text.
        /// </summary>
        RightOf,

        /// <summary>
        /// Place signature to the left of the found text.
        /// </summary>
        LeftOf,

        /// <summary>
        /// Place signature above the found text.
        /// </summary>
        Above,

        /// <summary>
        /// Place signature below the found text.
        /// </summary>
        Below,

        /// <summary>
        /// Place signature at the exact position of the found text (overlapping).
        /// </summary>
        AtPosition
    }
}
