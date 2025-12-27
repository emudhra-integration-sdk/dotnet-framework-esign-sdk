using System;
using System.Linq;
using System.Text.RegularExpressions;
using eSignASPLibrary.Util;

namespace eSignASPLibrary
{
    /// <summary>
    /// Provides a fluent API for building eSignInput objects using the builder pattern.
    /// </summary>
    /// <example>
    /// <code>
    /// var input = new eSignInputBuilder()
    ///     .SetDocBase64(pdfBase64)
    ///     .SetDocInfo("Invoice_2024")
    ///     .SetSignedBy("John Doe")
    ///     .SetLocation("New York")
    ///     .SignAllPages()
    ///     .SetCoordinates(eSign.Coordinates.Bottom_Right)
    ///     .Build();
    /// </code>
    /// </example>
    public class eSignInputBuilder
    {
        #region Private Fields

        private string _docBase64;
        private string _signedBy;
        private string _location;
        private string _reason;
        private bool _coSign = true;
        private bool _requiredGreenTick = true;
        private bool _requiredValidMessage = true;
        private eSign.PageToBeSigned _pageToBeSigned = eSign.PageToBeSigned.ALL;
        private eSign.Coordinates _coordinates = eSign.Coordinates.Bottom_Right;
        private eSign.DocType _docType = eSign.DocType.Pdf;
        private string _pageNumbers;
        private int _fontSize = 8;
        private string _pageLevelCoordinates;
        private string _customCoordinates;
        private string _pdfUrl;
        private string _docInfo;
        private string _appearanceText;

        #endregion

        #region Core Methods - Document Content

        /// <summary>
        /// Sets the Base64-encoded document content (PDF, Hash, or XML).
        /// </summary>
        /// <param name="docBase64">Base64-encoded document content.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when docBase64 is null or empty.</exception>
        public eSignInputBuilder SetDocBase64(string docBase64)
        {
            if (string.IsNullOrWhiteSpace(docBase64))
            {
                throw new ArgumentNullException(nameof(docBase64), "Document content cannot be null or empty.");
            }
            _docBase64 = docBase64;
            return this;
        }

        /// <summary>
        /// Sets the document information/identifier.
        /// </summary>
        /// <param name="docInfo">Document identifier or metadata.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetDocInfo(string docInfo)
        {
            _docInfo = docInfo;
            return this;
        }

        /// <summary>
        /// Sets the document URL (optional).
        /// </summary>
        /// <param name="pdfUrl">URL where the document is located.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetPdfUrl(string pdfUrl)
        {
            _pdfUrl = pdfUrl;
            return this;
        }

        /// <summary>
        /// Sets the document type (Pdf, Hash, or eMandate).
        /// </summary>
        /// <param name="docType">Type of document to sign.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetDocType(eSign.DocType docType)
        {
            _docType = docType;
            return this;
        }

        #endregion

        #region Signature Appearance Methods

        /// <summary>
        /// Sets the name of the person signing the document.
        /// </summary>
        /// <param name="signedBy">Signer's name.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetSignedBy(string signedBy)
        {
            _signedBy = signedBy;
            return this;
        }

        /// <summary>
        /// Sets the location where the document is being signed.
        /// </summary>
        /// <param name="location">Signing location.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetLocation(string location)
        {
            _location = location;
            return this;
        }

        /// <summary>
        /// Sets the reason for signing the document.
        /// </summary>
        /// <param name="reason">Signing reason.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetReason(string reason)
        {
            _reason = reason;
            return this;
        }

        /// <summary>
        /// Sets custom appearance text for the signature.
        /// </summary>
        /// <param name="appearanceText">Custom text to display in signature appearance.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetAppearanceText(string appearanceText)
        {
            _appearanceText = appearanceText;
            return this;
        }

        /// <summary>
        /// Sets the font size for the signature appearance.
        /// </summary>
        /// <param name="fontSize">Font size (default: 8).</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when fontSize is less than or equal to 0.</exception>
        public eSignInputBuilder SetFontSize(int fontSize)
        {
            if (fontSize <= 0)
            {
                throw new ArgumentException("Font size must be greater than 0.", nameof(fontSize));
            }
            _fontSize = fontSize;
            return this;
        }

        #endregion

        #region Positioning Methods

        /// <summary>
        /// Sets the signature position using predefined coordinates.
        /// </summary>
        /// <param name="coordinates">Predefined coordinate position.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetCoordinates(eSign.Coordinates coordinates)
        {
            _coordinates = coordinates;
            _customCoordinates = null; // Clear custom coordinates
            _pageLevelCoordinates = null; // Clear page-level coordinates
            return this;
        }

        /// <summary>
        /// Sets custom coordinates for signature placement.
        /// </summary>
        /// <param name="coordinates">Custom coordinates in format "x1,y1,x2,y2" (e.g., "50,47,180,87").</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when coordinate format is invalid.</exception>
        public eSignInputBuilder SetCustomCoordinates(string coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
            {
                throw new ArgumentNullException(nameof(coordinates), "Custom coordinates cannot be null or empty.");
            }

            // Validate format: should be "x1,y1,x2,y2"
            var parts = coordinates.Split(',');
            if (parts.Length != 4)
            {
                throw new ArgumentException("Custom coordinates must be in format 'x1,y1,x2,y2' (e.g., '50,47,180,87').", nameof(coordinates));
            }

            if (!parts.All(p => int.TryParse(p.Trim(), out _)))
            {
                throw new ArgumentException("All coordinate values must be valid integers.", nameof(coordinates));
            }

            _customCoordinates = coordinates;
            _pageLevelCoordinates = null; // Clear page-level coordinates
            return this;
        }

        /// <summary>
        /// Sets page-level coordinates for signing different pages at different positions.
        /// </summary>
        /// <param name="pageLevelCoordinates">Page-level coordinates in format "page,x1,y1,x2,y2;page,x1,y1,x2,y2".</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when format is invalid.</exception>
        public eSignInputBuilder SetPageLevelCoordinates(string pageLevelCoordinates)
        {
            if (string.IsNullOrWhiteSpace(pageLevelCoordinates))
            {
                throw new ArgumentNullException(nameof(pageLevelCoordinates), "Page-level coordinates cannot be null or empty.");
            }

            _pageLevelCoordinates = pageLevelCoordinates;
            _pageToBeSigned = eSign.PageToBeSigned.PAGE_LEVEL;
            _customCoordinates = null; // Clear custom coordinates
            return this;
        }

        #endregion

        #region Page Selection Methods

        /// <summary>
        /// Sets which pages to sign.
        /// </summary>
        /// <param name="pageToBeSigned">Page selection option.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetPageToBeSigned(eSign.PageToBeSigned pageToBeSigned)
        {
            _pageToBeSigned = pageToBeSigned;
            return this;
        }

        /// <summary>
        /// Sets specific page numbers to sign.
        /// </summary>
        /// <param name="pageNumbers">Comma-separated page numbers (e.g., "1,3,5,7").</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when page numbers format is invalid.</exception>
        public eSignInputBuilder SetPageNumbers(string pageNumbers)
        {
            if (string.IsNullOrWhiteSpace(pageNumbers))
            {
                throw new ArgumentNullException(nameof(pageNumbers), "Page numbers cannot be null or empty.");
            }

            // Validate format: should be comma-separated integers
            var pages = pageNumbers.Split(',');
            if (!pages.All(p => int.TryParse(p.Trim(), out _)))
            {
                throw new ArgumentException("Page numbers must be comma-separated integers (e.g., '1,3,5').", nameof(pageNumbers));
            }

            _pageNumbers = pageNumbers;
            _pageToBeSigned = eSign.PageToBeSigned.SPECIFY;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to all pages.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignAllPages()
        {
            _pageToBeSigned = eSign.PageToBeSigned.ALL;
            _pageNumbers = null;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to the first page only.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignFirstPage()
        {
            _pageToBeSigned = eSign.PageToBeSigned.FIRST;
            _pageNumbers = null;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to the last page only.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignLastPage()
        {
            _pageToBeSigned = eSign.PageToBeSigned.LAST;
            _pageNumbers = null;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to even pages only.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignEvenPages()
        {
            _pageToBeSigned = eSign.PageToBeSigned.EVEN;
            _pageNumbers = null;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to odd pages only.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignOddPages()
        {
            _pageToBeSigned = eSign.PageToBeSigned.ODD;
            _pageNumbers = null;
            return this;
        }

        /// <summary>
        /// Configures signing to apply to specific pages.
        /// </summary>
        /// <param name="pageNumbers">Comma-separated page numbers (e.g., "1,3,5,7").</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SignSpecificPages(string pageNumbers)
        {
            return SetPageNumbers(pageNumbers);
        }

        #endregion

        #region Signature Options

        /// <summary>
        /// Sets whether to enable co-signing.
        /// </summary>
        /// <param name="coSign">True to enable co-signing, false otherwise.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetCoSign(bool coSign)
        {
            _coSign = coSign;
            return this;
        }

        /// <summary>
        /// Enables co-signing on the document.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder EnableCoSign()
        {
            _coSign = true;
            return this;
        }

        /// <summary>
        /// Disables co-signing on the document.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder DisableCoSign()
        {
            _coSign = false;
            return this;
        }

        /// <summary>
        /// Sets whether to display the green tick in signature.
        /// </summary>
        /// <param name="required">True to show green tick, false to hide.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetRequiredGreenTick(bool required)
        {
            _requiredGreenTick = required;
            return this;
        }

        /// <summary>
        /// Shows the green tick in signature appearance.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder ShowGreenTick()
        {
            _requiredGreenTick = true;
            return this;
        }

        /// <summary>
        /// Hides the green tick in signature appearance.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder HideGreenTick()
        {
            _requiredGreenTick = false;
            return this;
        }

        /// <summary>
        /// Sets whether to display the validation message in signature.
        /// </summary>
        /// <param name="required">True to show validation message, false to hide.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SetRequiredValidMessage(bool required)
        {
            _requiredValidMessage = required;
            return this;
        }

        /// <summary>
        /// Shows the validation message in signature appearance.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder ShowValidMessage()
        {
            _requiredValidMessage = true;
            return this;
        }

        /// <summary>
        /// Hides the validation message in signature appearance.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder HideValidMessage()
        {
            _requiredValidMessage = false;
            return this;
        }

        #endregion

        #region Preset Configuration Methods

        /// <summary>
        /// Configures the builder for PDF document signing.
        /// </summary>
        /// <param name="pdfBase64">Base64-encoded PDF content.</param>
        /// <param name="docInfo">Document identifier.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder ForPdfSigning(string pdfBase64, string docInfo)
        {
            SetDocBase64(pdfBase64);
            SetDocInfo(docInfo);
            SetDocType(eSign.DocType.Pdf);
            return this;
        }

        /// <summary>
        /// Configures the builder for hash signing.
        /// </summary>
        /// <param name="hashValue">Hash value to sign.</param>
        /// <param name="docInfo">Document identifier.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder ForHashSigning(string hashValue, string docInfo)
        {
            SetDocBase64(hashValue);
            SetDocInfo(docInfo);
            SetDocType(eSign.DocType.Hash);
            return this;
        }

        /// <summary>
        /// Configures the builder for eMandate XML signing.
        /// </summary>
        /// <param name="xmlContent">XML content to sign.</param>
        /// <param name="docInfo">Document identifier.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder ForeMandateSigning(string xmlContent, string docInfo)
        {
            SetDocBase64(xmlContent);
            SetDocInfo(docInfo);
            SetDocType(eSign.DocType.eMandate);
            return this;
        }

        #endregion

        #region Text Search Based Positioning

        /// <summary>
        /// Sets signature position based on searching for text in the PDF document.
        /// </summary>
        /// <param name="searchConfig">Configuration for text search and signature placement.</param>
        /// <param name="searchResult">Output parameter containing search results and any errors.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <remarks>
        /// This method requires that DocBase64 is already set. It will search for the specified text
        /// and automatically calculate the signature coordinates based on the search configuration.
        /// If search fails, the error information is returned in searchResult parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// var searchConfig = new PdfTextSearchSignature
        /// {
        ///     SearchText = "Sign here:",
        ///     SignatureWidth = 150,
        ///     SignatureHeight = 60,
        ///     XOffset = 10,
        ///     Placement = SignaturePlacement.RightOf
        /// };
        ///
        /// var input = new eSignInputBuilder()
        ///     .SetDocBase64(pdfBase64)
        ///     .SetSignaturePositionByTextSearch(searchConfig, out var result)
        ///     .SetSignedBy("John Doe")
        ///     .Build();
        ///
        /// if (!result.Found)
        /// {
        ///     // Handle error: result.ErrorMessage
        /// }
        /// </code>
        /// </example>
        public eSignInputBuilder SetSignaturePositionByTextSearch(PdfTextSearchSignature searchConfig, out PdfTextSearchResult searchResult)
        {
            // Initialize result
            searchResult = new PdfTextSearchResult { Found = false };

            if (string.IsNullOrWhiteSpace(_docBase64))
            {
                searchResult.ErrorMessage = "DocBase64 must be set before performing text search.";
                return this;
            }

            if (searchConfig == null)
            {
                searchResult.ErrorMessage = "Search configuration cannot be null.";
                return this;
            }

            // Perform the search
            searchResult = PdfContentSearchUtility.SearchAndGetSignatureCoordinates(_docBase64, searchConfig);

            if (!searchResult.Found)
            {
                // Search failed - error already set in searchResult.ErrorMessage
                // Return builder to allow fallback configuration
                return this;
            }

            // Search successful - set the coordinates
            _customCoordinates = searchResult.CoordinateString;
            _pageToBeSigned = eSign.PageToBeSigned.SPECIFY;
            _pageNumbers = searchResult.PageNumber.ToString();

            return this;
        }

        /// <summary>
        /// Convenience method to set signature position by searching for text with default settings.
        /// Places signature to the right of the found text with default dimensions.
        /// </summary>
        /// <param name="searchText">The text to search for in the PDF.</param>
        /// <param name="xOffset">Horizontal offset from the found text (default: 10).</param>
        /// <param name="yOffset">Vertical offset from the found text (default: 0).</param>
        /// <param name="searchResult">Output parameter containing search results and any errors.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// var input = new eSignInputBuilder()
        ///     .SetDocBase64(pdfBase64)
        ///     .SearchAndPlaceSignature("Sign here:", 15, 0, out var result)
        ///     .SetSignedBy("John Doe")
        ///     .Build();
        ///
        /// if (!result.Found)
        /// {
        ///     Console.WriteLine($"Search failed: {result.ErrorMessage}");
        /// }
        /// </code>
        /// </example>
        public eSignInputBuilder SearchAndPlaceSignature(string searchText, int xOffset, int yOffset, out PdfTextSearchResult searchResult)
        {
            var searchConfig = new PdfTextSearchSignature
            {
                SearchText = searchText,
                SignatureWidth = 150,
                SignatureHeight = 60,
                XOffset = xOffset,
                YOffset = yOffset,
                Placement = SignaturePlacement.RightOf
            };

            return SetSignaturePositionByTextSearch(searchConfig, out searchResult);
        }

        /// <summary>
        /// Convenience method to set signature position by searching for text with default settings.
        /// Places signature to the right of the found text with default dimensions.
        /// </summary>
        /// <param name="searchText">The text to search for in the PDF.</param>
        /// <param name="searchResult">Output parameter containing search results and any errors.</param>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder SearchAndPlaceSignature(string searchText, out PdfTextSearchResult searchResult)
        {
            return SearchAndPlaceSignature(searchText, 10, 0, out searchResult);
        }

        /// <summary>
        /// Advanced signature placement by searching for text with custom dimensions.
        /// </summary>
        /// <param name="searchText">The text to search for in the PDF.</param>
        /// <param name="width">Width of the signature box.</param>
        /// <param name="height">Height of the signature box.</param>
        /// <param name="placement">Placement strategy relative to found text.</param>
        /// <param name="xOffset">Horizontal offset (default: 0).</param>
        /// <param name="yOffset">Vertical offset (default: 0).</param>
        /// <param name="searchResult">Output parameter containing search results and any errors.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// var input = new eSignInputBuilder()
        ///     .SetDocBase64(pdfBase64)
        ///     .SearchAndPlaceSignature("Signature:", 200, 80, SignaturePlacement.Below, -10, 0, out var result)
        ///     .SetSignedBy("Jane Smith")
        ///     .Build();
        ///
        /// if (!result.Found)
        /// {
        ///     Console.WriteLine($"Search failed: {result.ErrorMessage}");
        /// }
        /// </code>
        /// </example>
        public eSignInputBuilder SearchAndPlaceSignature(
            string searchText,
            int width,
            int height,
            SignaturePlacement placement,
            int xOffset,
            int yOffset,
            out PdfTextSearchResult searchResult)
        {
            var searchConfig = new PdfTextSearchSignature
            {
                SearchText = searchText,
                SignatureWidth = width,
                SignatureHeight = height,
                XOffset = xOffset,
                YOffset = yOffset,
                Placement = placement
            };

            return SetSignaturePositionByTextSearch(searchConfig, out searchResult);
        }

        #endregion

        #region Validation & Build

        /// <summary>
        /// Validates the current builder state.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(_docBase64))
            {
                throw new InvalidOperationException("Document content (DocBase64) must be set before building.");
            }

            if (_docType == eSign.DocType.Pdf)
            {
                // For PDF signing, validate that we have appearance settings
                if (string.IsNullOrWhiteSpace(_signedBy) &&
                    string.IsNullOrWhiteSpace(_location) &&
                    string.IsNullOrWhiteSpace(_reason) &&
                    string.IsNullOrWhiteSpace(_appearanceText))
                {
                    // Warning: No appearance settings, but this is technically allowed
                }
            }

            if (_fontSize <= 0)
            {
                throw new InvalidOperationException("Font size must be greater than 0.");
            }
        }

        /// <summary>
        /// Builds and returns the eSignInput object.
        /// </summary>
        /// <returns>A new eSignInput instance with the configured settings.</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
        public eSignInput Build()
        {
            Validate();

            // Determine which constructor to use based on configuration
            if (_docType == eSign.DocType.Hash)
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl, _docType);
            }
            else if (_docType == eSign.DocType.eMandate)
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl);
            }
            else if (_pageToBeSigned == eSign.PageToBeSigned.PAGE_LEVEL && !string.IsNullOrWhiteSpace(_pageLevelCoordinates))
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl, _location, _reason, _signedBy,
                    _coSign, _pageLevelCoordinates, _appearanceText, _requiredGreenTick, _requiredValidMessage, _fontSize);
            }
            else if (_pageToBeSigned == eSign.PageToBeSigned.SPECIFY && !string.IsNullOrWhiteSpace(_pageNumbers))
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl, _location, _reason, _signedBy,
                    _coSign, _coordinates, _pageNumbers, _appearanceText, _requiredGreenTick, _requiredValidMessage, _fontSize);
            }
            else if (!string.IsNullOrWhiteSpace(_customCoordinates))
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl, _location, _reason, _signedBy,
                    _coSign, _pageToBeSigned, _customCoordinates, _appearanceText, _requiredGreenTick, _requiredValidMessage, _fontSize);
            }
            else
            {
                return new eSignInput(_docBase64, _docInfo, _pdfUrl, _location, _reason, _signedBy,
                    _coSign, _pageToBeSigned, _coordinates, _appearanceText, _requiredGreenTick, _requiredValidMessage, _fontSize);
            }
        }

        /// <summary>
        /// Resets the builder to its default state.
        /// </summary>
        /// <returns>This builder instance for method chaining.</returns>
        public eSignInputBuilder Reset()
        {
            _docBase64 = null;
            _signedBy = null;
            _location = null;
            _reason = null;
            _coSign = true;
            _requiredGreenTick = true;
            _requiredValidMessage = true;
            _pageToBeSigned = eSign.PageToBeSigned.ALL;
            _coordinates = eSign.Coordinates.Bottom_Right;
            _docType = eSign.DocType.Pdf;
            _pageNumbers = null;
            _fontSize = 8;
            _pageLevelCoordinates = null;
            _customCoordinates = null;
            _pdfUrl = null;
            _docInfo = null;
            _appearanceText = null;

            return this;
        }

        #endregion
    }
}
