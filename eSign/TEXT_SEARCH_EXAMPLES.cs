using eSignASPLibrary;
using eSignASPLibrary.Util;
using System;
using System.Collections.Generic;

namespace eSignASPLibrary.Examples
{
    /// <summary>
    /// Examples demonstrating PDF text search and signature placement.
    /// Error handling follows the traditional eSignServiceReturn pattern - no exceptions thrown.
    /// </summary>
    public class TextSearchExamples
    {
        /// <summary>
        /// Example 1: Simple text search with error handling
        /// Searches for text that may span multiple chunks (e.g., "Inspector's Signature")
        /// </summary>
        public static eSignServiceReturn Example1_SimpleTextSearch(string pdfBase64)
        {
            // Create input with text search
            // The search now works across text chunks, so "Inspector's Signature" will be found
            // even if the PDF stores it as separate chunks like "Inspector's " + "Signature"
            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Invoice_2024")
                .SearchAndPlaceSignature("Sign here:", out searchResult)
                .SetSignedBy("John Doe")
                .SetLocation("New York")
                .SetReason("Approval")
                .Build();

            // ✅ Check if text search was successful
            if (!searchResult.Found)
            {
                // Text not found - return error similar to eSignServiceReturn pattern
                Console.WriteLine($"Text search failed: {searchResult.ErrorMessage}");

                // Use fallback coordinates
                input = new eSignInputBuilder()
                    .SetDocBase64(pdfBase64)
                    .SetDocInfo("Invoice_2024")
                    .SetCoordinates(eSign.Coordinates.Bottom_Right)  // Fallback
                    .SignFirstPage()
                    .SetSignedBy("John Doe")
                    .SetLocation("New York")
                    .SetReason("Approval")
                    .Build();
            }
            else
            {
                Console.WriteLine($"✅ Text found on page {searchResult.PageNumber}");
                Console.WriteLine($"   Signature placed at: {searchResult.CoordinateString}");
            }

            // Continue with signing process
            return ProcessSigning(new List<eSignInput> { input });
        }

        /// <summary>
        /// Example 8: Search for "Inspector's Signature" in inspection reports
        /// </summary>
        public static eSignServiceReturn Example8_InspectorSignature(string pdfBase64)
        {
            // Search for "Inspector's Signature" text
            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Inspection_Report")
                .SearchAndPlaceSignature("Inspector's Signature", 150, 60, SignaturePlacement.Below, 0, -10, out searchResult)
                .SetSignedBy("Inspector Name")
                .SetLocation("Inspection Site")
                .SetReason("Inspection Completed")
                .Build();

            if (!searchResult.Found)
            {
                Console.WriteLine($"❌ Could not find 'Inspector's Signature': {searchResult.ErrorMessage}");
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "TXT-404",
                    ErrorMessage = searchResult.ErrorMessage
                };
            }

            Console.WriteLine($"✅ Found 'Inspector's Signature' on page {searchResult.PageNumber}");
            Console.WriteLine($"   Position: ({searchResult.TextX:F1}, {searchResult.TextY:F1})");
            Console.WriteLine($"   Signature will be placed below at: {searchResult.CoordinateString}");

            return ProcessSigning(new List<eSignInput> { input });
        }

        /// <summary>
        /// Example 2: Advanced text search with custom configuration
        /// </summary>
        public static eSignServiceReturn Example2_AdvancedSearch(string pdfBase64)
        {
            var searchConfig = new PdfTextSearchSignature
            {
                SearchText = "Authorized Signature:",
                SignatureWidth = 200,
                SignatureHeight = 80,
                Placement = SignaturePlacement.Below
            };

            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Contract_2024")
                .SetSignaturePositionByTextSearch(searchConfig, out searchResult)
                .SetSignedBy("Jane Smith")
                .SetLocation("Los Angeles")
                .Build();

            // ✅ Error handling - no exceptions thrown
            if (!searchResult.Found)
            {
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "TXT-001",
                    ErrorMessage = $"Text search failed: {searchResult.ErrorMessage}"
                };
            }

            return ProcessSigning(new List<eSignInput> { input });
        }

        /// <summary>
        /// Example 3: Multiple signatures with text search
        /// </summary>
        public static eSignServiceReturn Example3_MultipleSignatures(string pdfBase64)
        {
            var inputs = new List<eSignInput>();

            // First signature - search for "Manager:"
            PdfTextSearchResult result1;
            var input1 = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Doc1")
                .SearchAndPlaceSignature("Manager:", 10, 0, out result1)
                .SetSignedBy("Manager Name")
                .Build();

            if (result1.Found)
            {
                inputs.Add(input1);
                Console.WriteLine($"✅ Manager signature: page {result1.PageNumber}, position {result1.CoordinateString}");
            }
            else
            {
                Console.WriteLine($"⚠️ Manager signature failed: {result1.ErrorMessage}");
                // Use fallback
                inputs.Add(new eSignInputBuilder()
                    .SetDocBase64(pdfBase64)
                    .SetDocInfo("Doc1")
                    .SetCoordinates(eSign.Coordinates.Bottom_Right)
                    .SetSignedBy("Manager Name")
                    .Build());
            }

            // Second signature - search for "Director:"
            PdfTextSearchResult result2;
            var input2 = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Doc2")
                .SearchAndPlaceSignature("Director:", 10, 0, out result2)
                .SetSignedBy("Director Name")
                .Build();

            if (result2.Found)
            {
                inputs.Add(input2);
                Console.WriteLine($"✅ Director signature: page {result2.PageNumber}, position {result2.CoordinateString}");
            }
            else
            {
                Console.WriteLine($"⚠️ Director signature failed: {result2.ErrorMessage}");
            }

            return ProcessSigning(inputs);
        }

        /// <summary>
        /// Example 4: Handling second occurrence of text
        /// </summary>
        public static eSignServiceReturn Example4_SecondOccurrence(string pdfBase64)
        {
            // Find the SECOND occurrence of "Signature:"
            var searchConfig = new PdfTextSearchSignature
            {
                SearchText = "Signature:",
                MatchIndex = 1,  // 0 = first, 1 = second, etc.
                SignatureWidth = 150,
                SignatureHeight = 60,
                XOffset = 10,
                Placement = SignaturePlacement.RightOf
            };

            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Multi_Signer_Doc")
                .SetSignaturePositionByTextSearch(searchConfig, out searchResult)
                .SetSignedBy("Second Signer")
                .Build();

            if (!searchResult.Found)
            {
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "TXT-002",
                    ErrorMessage = $"Second occurrence not found: {searchResult.ErrorMessage}. Total matches: {searchResult.TotalMatches}"
                };
            }

            Console.WriteLine($"✅ Found occurrence #{searchConfig.MatchIndex + 1} out of {searchResult.TotalMatches} total matches");
            return ProcessSigning(new List<eSignInput> { input });
        }

        /// <summary>
        /// Example 5: Different placement strategies
        /// </summary>
        public static void Example5_PlacementStrategies(string pdfBase64)
        {
            // Place ABOVE text
            PdfTextSearchResult result1;
            var inputAbove = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SearchAndPlaceSignature("Name:", 150, 60, SignaturePlacement.Above, 0, 0, out result1)
                .SetSignedBy("Person 1")
                .Build();

            // Place BELOW text
            PdfTextSearchResult result2;
            var inputBelow = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SearchAndPlaceSignature("Title:", 150, 60, SignaturePlacement.Below, 0, 0, out result2)
                .SetSignedBy("Person 2")
                .Build();

            // Place to the LEFT
            PdfTextSearchResult result3;
            var inputLeft = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SearchAndPlaceSignature("Date:", 150, 60, SignaturePlacement.LeftOf, 0, 0, out result3)
                .SetSignedBy("Person 3")
                .Build();

            // Place to the RIGHT (default)
            PdfTextSearchResult result4;
            var inputRight = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SearchAndPlaceSignature("Sign:", 150, 60, SignaturePlacement.RightOf, 0, 0, out result4)
                .SetSignedBy("Person 4")
                .Build();

            // Check results
            Console.WriteLine($"Above placement: {(result1.Found ? "✅" : "❌")} {result1.ErrorMessage}");
            Console.WriteLine($"Below placement: {(result2.Found ? "✅" : "❌")} {result2.ErrorMessage}");
            Console.WriteLine($"Left placement: {(result3.Found ? "✅" : "❌")} {result3.ErrorMessage}");
            Console.WriteLine($"Right placement: {(result4.Found ? "✅" : "❌")} {result4.ErrorMessage}");
        }

        /// <summary>
        /// Example 6: Complete workflow with all error handling
        /// </summary>
        public static eSignServiceReturn Example6_CompleteWorkflow(
            string pdfBase64,
            string searchText,
            string signerName,
            string location)
        {
            // Step 1: Try text search
            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo($"Doc_{DateTime.Now:yyyyMMdd_HHmmss}")
                .SearchAndPlaceSignature(searchText, out searchResult)
                .SetSignedBy(signerName)
                .SetLocation(location)
                .SetReason("Document Approval")
                .ShowGreenTick()
                .EnableCoSign()
                .Build();

            // Step 2: Check search result
            if (!searchResult.Found)
            {
                Console.WriteLine($"⚠️ Text search failed: {searchResult.ErrorMessage}");
                Console.WriteLine("⚠️ Using fallback coordinates...");

                // Rebuild with fallback
                input = new eSignInputBuilder()
                    .SetDocBase64(pdfBase64)
                    .SetDocInfo($"Doc_{DateTime.Now:yyyyMMdd_HHmmss}")
                    .SetCoordinates(eSign.Coordinates.Bottom_Right)
                    .SignFirstPage()
                    .SetSignedBy(signerName)
                    .SetLocation(location)
                    .SetReason("Document Approval")
                    .ShowGreenTick()
                    .EnableCoSign()
                    .Build();
            }
            else
            {
                Console.WriteLine($"✅ Text search successful!");
                Console.WriteLine($"   Page: {searchResult.PageNumber}");
                Console.WriteLine($"   Text position: ({searchResult.TextX}, {searchResult.TextY})");
                Console.WriteLine($"   Signature position: {searchResult.CoordinateString}");
            }

            // Step 3: Process signing
            return ProcessSigning(new List<eSignInput> { input });
        }

        /// <summary>
        /// Helper method to simulate signing process
        /// </summary>
        private static eSignServiceReturn ProcessSigning(List<eSignInput> inputs)
        {
            try
            {
                // Initialize eSign
                var esign = new eSign(
                    PFXFilePath: "path/to/certificate.pfx",
                    PFXPassword: "password",
                    PFXAlias: "alias",
                    IsProxyRequired: false,
                    ProxyIP: null,
                    ProxyPort: 0,
                    ProxyUserName: null,
                    ProxyPassword: null,
                    ASPID: "YOUR_ASP_ID",
                    eSignURL: "https://gateway.example.com",
                    eSignURLV2: "https://gateway-v2.example.com",
                    eSignCheckStatusURL: "https://status.example.com"
                );

                // Get gateway parameters
                var result = esign.GetGateWayParam(
                    inputs,
                    signerID: "SIGNER_001",
                    transactionID: Guid.NewGuid().ToString(),
                    resposeURL: "https://yourapp.com/callback",
                    redirectUrl: "https://yourapp.com/success",
                    TempFolderPath: @"C:\Temp\eSign",
                    eSignAPIVersion: eSign.eSignAPIVersion.V3,
                    authMode: eSign.AuthMode.OTP
                );

                // ✅ Return result - no exceptions, follows eSignServiceReturn pattern
                return result;
            }
            catch (Exception ex)
            {
                // Return error in traditional format
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "ESS-999",
                    ErrorMessage = $"Signing process error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Example 7: Validation before processing
        /// </summary>
        public static eSignServiceReturn Example7_PreValidation(string pdfBase64, string searchText)
        {
            // Validate search configuration first
            var searchConfig = new PdfTextSearchSignature
            {
                SearchText = searchText,
                SignatureWidth = 150,
                SignatureHeight = 60,
                XOffset = 10
            };

            // ✅ Check configuration validity
            if (!searchConfig.Validate(out string validationError))
            {
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "VAL-001",
                    ErrorMessage = $"Invalid search configuration: {validationError}"
                };
            }

            // Configuration is valid, proceed with search
            PdfTextSearchResult searchResult;
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetSignaturePositionByTextSearch(searchConfig, out searchResult)
                .SetSignedBy("User")
                .Build();

            if (!searchResult.Found)
            {
                return new eSignServiceReturn
                {
                    ReturnStatus = eSign.status.Failure,
                    ErrorCode = "TXT-003",
                    ErrorMessage = searchResult.ErrorMessage
                };
            }

            return ProcessSigning(new List<eSignInput> { input });
        }
    }
}
