# eSign Library Usage Examples

## Table of Contents
1. [eSignInputBuilder - Basic Usage](#esigninputbuilder---basic-usage)
2. [PDF Text Search & Signature Placement](#pdf-text-search--signature-placement)
3. [Advanced Scenarios](#advanced-scenarios)

---

## eSignInputBuilder - Basic Usage

### Example 1: Simple PDF Signing

```csharp
using eSignASPLibrary;

// Simple signature on all pages at bottom-right
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Contract_2024_001")
    .SetSignedBy("John Doe")
    .SetLocation("New York, NY")
    .SetReason("Contract Approval")
    .SignAllPages()
    .SetCoordinates(eSign.Coordinates.Bottom_Right)
    .Build();
```

### Example 2: Sign Specific Pages with Custom Coordinates

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Legal_Document")
    .SetSignedBy("Jane Smith")
    .SetLocation("Los Angeles, CA")
    .SetReason("Legal Authorization")
    .SignSpecificPages("1,5,10")  // Pages 1, 5, and 10
    .SetCustomCoordinates("100,150,250,200")  // x1,y1,x2,y2
    .SetFontSize(10)
    .Build();
```

### Example 3: Page-Level Coordinates (Different positions on different pages)

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Multi_Page_Agreement")
    .SetSignedBy("Robert Johnson")
    .SetLocation("Chicago, IL")
    .SetPageLevelCoordinates("1,50,700,200,750;5,100,650,250,700")
    // Page 1: x1=50, y1=700, x2=200, y2=750
    // Page 5: x1=100, y1=650, x2=250, y2=700
    .Build();
```

---

## PDF Text Search & Signature Placement

### Example 4: Simple Text Search (Most Common Use Case)

```csharp
// Find "Sign here:" and place signature to the right
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Invoice_2024")
    .SearchAndPlaceSignature("Sign here:", xOffset: 10)
    .SetSignedBy("Alice Williams")
    .SetLocation("San Francisco, CA")
    .SetReason("Invoice Approval")
    .Build();
```

### Example 5: Advanced Text Search with Custom Dimensions

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Purchase_Order")
    .SearchAndPlaceSignature(
        searchText: "Authorized Signature:",
        width: 200,
        height: 80,
        placement: SignaturePlacement.Below,
        yOffset: -10
    )
    .SetSignedBy("Michael Brown")
    .Build();
```

### Example 6: Full Control with PdfTextSearchSignature Object

```csharp
var searchConfig = new PdfTextSearchSignature
{
    SearchText = "Sign here:",
    SignatureWidth = 150,
    SignatureHeight = 60,
    XOffset = 10,
    YOffset = 5,
    PageNumber = 1,           // Search only on page 1
    MatchIndex = 0,           // Use first occurrence
    IgnoreCase = true,        // Case-insensitive search
    Placement = SignaturePlacement.RightOf
};

var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Contract")
    .SetSignaturePositionByTextSearch(searchConfig)
    .SetSignedBy("Sarah Davis")
    .SetLocation("Boston, MA")
    .Build();
```

---

## Advanced Scenarios

### Example 7: Multiple Occurrences Handling

```csharp
// Find the SECOND occurrence of "Signature:"
var searchConfig = new PdfTextSearchSignature
{
    SearchText = "Signature:",
    MatchIndex = 1,  // 0 = first, 1 = second, etc.
    SignatureWidth = 150,
    SignatureHeight = 60,
    XOffset = 15
};

var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Multi_Signer_Document")
    .SetSignaturePositionByTextSearch(searchConfig)
    .SetSignedBy("David Miller")
    .Build();
```

### Example 8: Different Placement Strategies

```csharp
// Place signature ABOVE the text
var inputAbove = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Name:", 150, 60, SignaturePlacement.Above)
    .SetSignedBy("Person 1")
    .Build();

// Place signature BELOW the text
var inputBelow = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Title:", 150, 60, SignaturePlacement.Below)
    .SetSignedBy("Person 2")
    .Build();

// Place signature to the LEFT of the text
var inputLeft = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Date:", 150, 60, SignaturePlacement.LeftOf)
    .SetSignedBy("Person 3")
    .Build();
```

### Example 9: Custom Appearance Text

```csharp
var customAppearance = @"Digitally Signed by John Doe
Organization: Acme Corp
Date: 2024-12-27 10:30:00
Reason: Document Approval";

var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64String)
    .SetDocInfo("Corporate_Document")
    .SearchAndPlaceSignature("Approval:", xOffset: 20)
    .SetSignedBy("John Doe")
    .SetAppearanceText(customAppearance)
    .SetFontSize(9)
    .ShowGreenTick()
    .Build();
```

### Example 10: Hash Signing

```csharp
var input = new eSignInputBuilder()
    .ForHashSigning(hashValue, "DocumentHash_12345")
    .SetPdfUrl("https://example.com/document.pdf")
    .Build();
```

### Example 11: eMandate XML Signing

```csharp
var input = new eSignInputBuilder()
    .ForeMandateSigning(xmlContent, "Mandate_67890")
    .SetPdfUrl("https://example.com/mandate.xml")
    .Build();
```

### Example 12: Complete Workflow with Error Handling

```csharp
using eSignASPLibrary;
using System;

public class SigningExample
{
    public void SignDocumentWithTextSearch(string pdfBase64)
    {
        try
        {
            // Method 1: Simple search
            var input = new eSignInputBuilder()
                .SetDocBase64(pdfBase64)
                .SetDocInfo("Invoice_2024_Q4")
                .SearchAndPlaceSignature("Sign here:", xOffset: 10)
                .SetSignedBy("John Doe")
                .SetLocation("New York, NY")
                .SetReason("Invoice Approval")
                .EnableCoSign()
                .ShowGreenTick()
                .Build();

            // Now use this input with eSign class
            var esign = new eSign(
                pfxFilePath: "certificate.pfx",
                pfxPassword: "password",
                pfxAlias: "alias",
                isProxyRequired: false,
                proxyIP: null,
                proxyPort: 0,
                proxyUserName: null,
                proxyPassword: null,
                aspID: "YOUR_ASP_ID",
                eSignURL: "https://gateway.example.com",
                eSignURLV2: "https://gateway-v2.example.com",
                eSignCheckStatusURL: "https://status.example.com"
            );

            var result = esign.GetGateWayParam(
                new List<eSignInput> { input },
                signerID: "SIGNER_123",
                transactionID: "TXN_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                resposeURL: "https://yourapp.com/callback",
                redirectUrl: "https://yourapp.com/success",
                tempFolderPath: @"C:\Temp\eSign",
                eSignAPIVersion: eSign.eSignAPIVersion.V3,
                authMode: eSign.AuthMode.OTP
            );

            if (result.ReturnStatus == eSign.status.Success)
            {
                Console.WriteLine($"Success! Gateway Parameter: {result.GatewayParameter}");
            }
            else
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
        catch (InvalidOperationException ex)
        {
            // Text search failed
            Console.WriteLine($"Search error: {ex.Message}");
            // Fallback to fixed coordinates
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

---

## Placement Strategy Reference

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `RightOf` | Signature to the right of text | "Sign here: ______" |
| `LeftOf` | Signature to the left of text | "______ Signature" |
| `Above` | Signature above the text | Forms with signature above name |
| `Below` | Signature below the text | Forms with signature below name |
| `AtPosition` | Signature overlaps text | Replace placeholder text |

---

## Coordinate System Reference

PDF coordinate system:
- Origin (0, 0) is at **bottom-left** corner
- X increases to the right
- Y increases upward

```
(0, 842) ─────────────── (595, 842)  ← Top of A4 page
    │                           │
    │                           │
    │        Page Content       │
    │                           │
    │                           │
(0, 0) ───────────────── (595, 0)    ← Bottom (origin)
```

Standard A4 page: 595 x 842 points (at 72 DPI)

---

## Tips & Best Practices

### 1. Text Search
- Make search text unique to avoid unwanted matches
- Use `MatchIndex` when text appears multiple times
- Use `PageNumber` to limit search scope
- Enable `IgnoreCase` for more flexible matching

### 2. Signature Dimensions
- Default: 150 x 60 points (~2" x 0.83")
- Adjust based on your needs
- Consider PDF page size

### 3. Offsets
- Use positive X offset to move right
- Use negative X offset to move left
- Use positive Y offset to move up
- Use negative Y offset to move down

### 4. Error Handling
- Always wrap text search in try-catch
- Provide fallback to fixed coordinates
- Validate PDF content before processing

---

## Complete Integration Example

```csharp
using eSignASPLibrary;
using eSignASPLibrary.Util;
using System;
using System.Collections.Generic;

public class eSignIntegration
{
    public eSignServiceReturn SignPdfWithTextSearch(
        string pdfBase64,
        string searchText,
        string signerName,
        string location)
    {
        // Create signing input using text search
        var input = new eSignInputBuilder()
            .SetDocBase64(pdfBase64)
            .SetDocInfo($"Doc_{DateTime.Now:yyyyMMdd_HHmmss}")
            .SearchAndPlaceSignature(searchText, xOffset: 10)
            .SetSignedBy(signerName)
            .SetLocation(location)
            .SetReason("Document Approval")
            .Build();

        // Initialize eSign
        var esign = new eSign(
            pfxFilePath: "path/to/certificate.pfx",
            pfxPassword: "cert_password",
            pfxAlias: "cert_alias",
            isProxyRequired: false,
            proxyIP: "",
            proxyPort: 0,
            proxyUserName: "",
            proxyPassword: "",
            aspID: Environment.GetEnvironmentVariable("ESIGN_ASP_ID"),
            eSignURL: Environment.GetEnvironmentVariable("ESIGN_GATEWAY_URL"),
            eSignURLV2: Environment.GetEnvironmentVariable("ESIGN_GATEWAY_V2_URL"),
            eSignCheckStatusURL: Environment.GetEnvironmentVariable("ESIGN_STATUS_URL")
        );

        // Get gateway parameters
        return esign.GetGateWayParam(
            new List<eSignInput> { input },
            signerID: "SIGNER_001",
            transactionID: Guid.NewGuid().ToString(),
            resposeURL: "https://yourapp.com/callback",
            redirectUrl: "https://yourapp.com/success",
            tempFolderPath: @"C:\Temp\eSign"
        );
    }
}
```

---

**For more information, see the main README.md file.**
