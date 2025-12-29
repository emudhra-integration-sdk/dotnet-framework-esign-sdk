using System;
using System.Collections.Generic;
using eSignLibrary.text.pdf;
using eSignLibrary.text.pdf.parser;
using emCastle.Utilities.Encoders;

namespace eSignASPLibrary.Util
{
    /// <summary>
    /// Utility class for searching text in PDF documents and calculating signature placement coordinates.
    /// </summary>
    public static class PdfContentSearchUtility
    {
        /// <summary>
        /// Searches for text in a PDF document and returns signature placement coordinates.
        /// </summary>
        /// <param name="pdfBase64">Base64-encoded PDF document.</param>
        /// <param name="searchConfig">Search configuration object.</param>
        /// <returns>Search result with signature coordinates.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when PDF cannot be read.</exception>
        public static PdfTextSearchResult SearchAndGetSignatureCoordinates(string pdfBase64, PdfTextSearchSignature searchConfig)
        {
            if (string.IsNullOrWhiteSpace(pdfBase64))
            {
                return new PdfTextSearchResult
                {
                    Found = false,
                    ErrorMessage = "PDF content cannot be null or empty."
                };
            }

            if (searchConfig == null)
            {
                return new PdfTextSearchResult
                {
                    Found = false,
                    ErrorMessage = "Search configuration cannot be null."
                };
            }

            // Validate configuration
            if (!searchConfig.Validate(out string validationError))
            {
                return new PdfTextSearchResult
                {
                    Found = false,
                    ErrorMessage = validationError
                };
            }

            PdfReader reader = null;
            try
            {
                // Decode PDF from Base64
                byte[] pdfBytes = Base64.Decode(pdfBase64);
                reader = new PdfReader(pdfBytes);

                int totalPages = reader.NumberOfPages;
                int startPage = searchConfig.PageNumber.HasValue ? searchConfig.PageNumber.Value : 1;
                int endPage = searchConfig.PageNumber.HasValue ? searchConfig.PageNumber.Value : totalPages;

                // Validate page range
                if (startPage < 1 || startPage > totalPages)
                {
                    return new PdfTextSearchResult
                    {
                        Found = false,
                        ErrorMessage = $"Page number {startPage} is out of range. PDF has {totalPages} pages."
                    };
                }

                // Search for text across specified pages
                List<TextLocationInfo> allMatches = new List<TextLocationInfo>();

                for (int pageNum = startPage; pageNum <= endPage; pageNum++)
                {
                    var matches = SearchTextInPage(reader, pageNum, searchConfig.SearchText, searchConfig.IgnoreCase);
                    allMatches.AddRange(matches);
                }

                if (allMatches.Count == 0)
                {
                    return new PdfTextSearchResult
                    {
                        Found = false,
                        ErrorMessage = $"Text '{searchConfig.SearchText}' not found in the document.",
                        TotalMatches = 0
                    };
                }

                // Get the specified match (default: first occurrence)
                int matchIndex = Math.Min(searchConfig.MatchIndex, allMatches.Count - 1);
                TextLocationInfo selectedMatch = allMatches[matchIndex];

                // Calculate signature coordinates based on placement strategy
                var signatureCoords = CalculateSignatureCoordinates(
                    selectedMatch,
                    searchConfig.SignatureWidth,
                    searchConfig.SignatureHeight,
                    searchConfig.XOffset,
                    searchConfig.YOffset,
                    searchConfig.Placement
                );

                return new PdfTextSearchResult
                {
                    Found = true,
                    PageNumber = selectedMatch.PageNumber,
                    TextX = selectedMatch.X,
                    TextY = selectedMatch.Y,
                    TextWidth = selectedMatch.Width,
                    TextHeight = selectedMatch.Height,
                    SignatureX1 = signatureCoords.X1,
                    SignatureY1 = signatureCoords.Y1,
                    SignatureX2 = signatureCoords.X2,
                    SignatureY2 = signatureCoords.Y2,
                    TotalMatches = allMatches.Count,
                    FoundText = selectedMatch.Text
                };
            }
            catch (Exception ex)
            {
                return new PdfTextSearchResult
                {
                    Found = false,
                    ErrorMessage = $"Error searching PDF: {ex.Message}"
                };
            }
            finally
            {
                reader?.Close();
            }
        }

        /// <summary>
        /// Searches for text in a specific page of the PDF.
        /// </summary>
        private static List<TextLocationInfo> SearchTextInPage(PdfReader reader, int pageNumber, string searchText, bool ignoreCase)
        {
            List<TextLocationInfo> matches = new List<TextLocationInfo>();

            try
            {
                // Use custom extraction strategy to get text with positions
                var strategy = new TextSearchStrategy(searchText, ignoreCase);
                PdfTextExtractor.GetTextFromPage(reader, pageNumber, strategy);

                // Perform final search across all accumulated chunks
                strategy.FinalizeSearch();

                // Get all matches from the strategy
                foreach (var match in strategy.Matches)
                {
                    match.PageNumber = pageNumber;
                    matches.Add(match);
                }
            }
            catch (Exception)
            {
                // If page extraction fails, continue to next page
            }

            return matches;
        }

        /// <summary>
        /// Calculates signature coordinates based on found text position and placement strategy.
        /// </summary>
        private static SignatureCoordinates CalculateSignatureCoordinates(
            TextLocationInfo textLocation,
            int signatureWidth,
            int signatureHeight,
            int xOffset,
            int yOffset,
            SignaturePlacement placement)
        {
            float baseX = textLocation.X;
            float baseY = textLocation.Y;
            float textWidth = textLocation.Width;
            float textHeight = textLocation.Height;

            int x1, y1, x2, y2;

            switch (placement)
            {
                case SignaturePlacement.RightOf:
                    // Place signature to the right of the text
                    x1 = (int)(baseX + textWidth + xOffset);
                    y1 = (int)(baseY + yOffset);
                    x2 = x1 + signatureWidth;
                    y2 = y1 + signatureHeight;
                    break;

                case SignaturePlacement.LeftOf:
                    // Place signature to the left of the text
                    x1 = (int)(baseX - signatureWidth + xOffset);
                    y1 = (int)(baseY + yOffset);
                    x2 = x1 + signatureWidth;
                    y2 = y1 + signatureHeight;
                    break;

                case SignaturePlacement.Above:
                    // Place signature above the text
                    x1 = (int)(baseX + xOffset);
                    y1 = (int)(baseY + textHeight + yOffset);
                    x2 = x1 + signatureWidth;
                    y2 = y1 + signatureHeight;
                    break;

                case SignaturePlacement.Below:
                    // Place signature below the text
                    x1 = (int)(baseX + xOffset);
                    y1 = (int)(baseY - signatureHeight + yOffset);
                    x2 = x1 + signatureWidth;
                    y2 = y1 + signatureHeight;
                    break;

                case SignaturePlacement.AtPosition:
                default:
                    // Place signature at the exact position (overlapping)
                    x1 = (int)(baseX + xOffset);
                    y1 = (int)(baseY + yOffset);
                    x2 = x1 + signatureWidth;
                    y2 = y1 + signatureHeight;
                    break;
            }

            return new SignatureCoordinates
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };
        }

        /// <summary>
        /// Custom text extraction strategy that captures text with position information.
        /// Handles multi-chunk text searches by accumulating all text chunks.
        /// </summary>
        private class TextSearchStrategy : ITextExtractionStrategy
        {
            private readonly string _searchText;
            private readonly bool _ignoreCase;
            private readonly StringComparison _comparison;
            public List<TextLocationInfo> Matches { get; private set; }

            // Store all text chunks with their position info
            private List<TextChunkInfo> _textChunks = new List<TextChunkInfo>();

            public TextSearchStrategy(string searchText, bool ignoreCase)
            {
                _searchText = searchText;
                _ignoreCase = ignoreCase;
                _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                Matches = new List<TextLocationInfo>();
            }

            public void BeginTextBlock()
            {
                // Don't clear chunks - we want to accumulate across all text blocks on the page
            }

            public void RenderText(TextRenderInfo renderInfo)
            {
                string text = renderInfo.GetText();

                if (string.IsNullOrEmpty(text))
                    return;

                // Store all text chunks with their position information
                LineSegment baseline = renderInfo.GetBaseline();
                Vector startPoint = baseline.GetStartPoint();
                Vector endPoint = baseline.GetEndPoint();

                float x = startPoint[0];
                float y = startPoint[1];
                float width = endPoint[0] - startPoint[0];
                float height = renderInfo.GetAscentLine().GetStartPoint()[1] -
                               renderInfo.GetDescentLine().GetStartPoint()[1];

                _textChunks.Add(new TextChunkInfo
                {
                    Text = text,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                });
            }

            public void EndTextBlock()
            {
                // Don't search here - wait until all text blocks are processed
            }

            /// <summary>
            /// Call this after all text extraction is complete to perform the search.
            /// </summary>
            public void FinalizeSearch()
            {
                SearchAcrossChunks();
            }

            private void SearchAcrossChunks()
            {
                if (_textChunks.Count == 0)
                    return;

                // Build the full text from all chunks
                string fullText = string.Join("", _textChunks.ConvertAll(c => c.Text));

                // Search for all occurrences
                int searchIndex = 0;
                while (searchIndex < fullText.Length)
                {
                    int foundIndex = fullText.IndexOf(_searchText, searchIndex, _comparison);
                    if (foundIndex < 0)
                        break;

                    // Find which chunk(s) contain this match
                    int currentPos = 0;
                    int matchStartChunk = -1;
                    int matchEndChunk = -1;

                    for (int i = 0; i < _textChunks.Count; i++)
                    {
                        int chunkLength = _textChunks[i].Text.Length;

                        if (matchStartChunk < 0 && foundIndex >= currentPos && foundIndex < currentPos + chunkLength)
                        {
                            matchStartChunk = i;
                        }

                        if (foundIndex + _searchText.Length > currentPos && foundIndex + _searchText.Length <= currentPos + chunkLength)
                        {
                            matchEndChunk = i;
                            break;
                        }

                        currentPos += chunkLength;
                    }

                    // If we didn't find end chunk, it might be at the very end
                    if (matchEndChunk < 0 && matchStartChunk >= 0)
                    {
                        matchEndChunk = _textChunks.Count - 1;
                    }

                    if (matchStartChunk >= 0 && matchEndChunk >= 0)
                    {
                        // Use the position of the first chunk where match starts
                        var startChunk = _textChunks[matchStartChunk];
                        var endChunk = _textChunks[matchEndChunk];

                        // Calculate the bounding box for the entire match
                        float matchX = startChunk.X;
                        float matchY = Math.Min(startChunk.Y, endChunk.Y);
                        float matchWidth = (endChunk.X + endChunk.Width) - startChunk.X;
                        float matchHeight = Math.Max(startChunk.Height, endChunk.Height);

                        Matches.Add(new TextLocationInfo
                        {
                            Text = fullText.Substring(foundIndex, _searchText.Length),
                            X = matchX,
                            Y = matchY,
                            Width = matchWidth,
                            Height = matchHeight
                        });
                    }

                    searchIndex = foundIndex + 1; // Continue searching for more occurrences
                }
            }

            public void RenderImage(ImageRenderInfo renderInfo)
            {
            }

            public string GetResultantText()
            {
                return string.Empty;
            }

            /// <summary>
            /// Stores a single text chunk with its position.
            /// </summary>
            private class TextChunkInfo
            {
                public string Text { get; set; }
                public float X { get; set; }
                public float Y { get; set; }
                public float Width { get; set; }
                public float Height { get; set; }
            }
        }

        /// <summary>
        /// Internal class to store text location information.
        /// </summary>
        private class TextLocationInfo
        {
            public int PageNumber { get; set; }
            public string Text { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
        }

        /// <summary>
        /// Internal class to store calculated signature coordinates.
        /// </summary>
        private class SignatureCoordinates
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }
        }
    }
}
