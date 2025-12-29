# eSign Library Kit (.NET)

A comprehensive .NET SDK for digital signature operations, providing PDF signing capabilities with support for multiple authentication methods and signature appearance customization.

## Table of Contents
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [New Features](#new-features)
- [Integration Guide](#integration-guide)
- [API Documentation](#api-documentation)
- [Security Considerations](#security-considerations)
- [License](#license)
- [Contributing](#contributing)

---

## Features

### Core Capabilities
- **PDF Digital Signing**: Sign PDF documents with digital certificates (X.509)
- **Multi-factor Authentication Support**:
  - OTP (One-Time Password)
  - Fingerprint
  - IRIS recognition
  - Face recognition
- **Flexible Signature Positioning**:
  - 9-point coordinate system (Top/Middle/Bottom × Left/Center/Right)
  - Custom coordinates
  - **NEW: Text search-based positioning** - automatically find and place signatures
  - Page-level coordinates
- **Document Processing**:
  - Page-level signing control (All, Even, Odd, First, Last, or specify pages)
  - PDF, Hash, and eMandate XML signing
  - **NEW: Builder pattern for easy input creation**
- **Enterprise Features**:
  - Proxy server support
  - Transaction management and verification
  - Windows support (.NET Framework 4.8)

---

## Requirements

### Runtime Requirements
- **.NET Framework**: 4.8 or higher
- **Operating System**: Windows

### Build Requirements
- **.NET SDK**: 6.0 or higher
- **IDE** (optional): Visual Studio 2019/2022, VS Code, JetBrains Rider

---

## Installation

### Option 1: Build from Source

```bash
git clone https://github.com/emudhra-integration-sdk/dotnet-framework-esign-sdk.git
cd NetFramework_eSignLibKit
dotnet restore
dotnet build --configuration Release
```

The compiled DLL will be in `bin/Release/eSignASPLibrary.dll`

### Option 2: NuGet Package (Coming Soon)

```bash
dotnet add package eSignASPLibrary
```

---

## Quick Start

### Basic PDF Signing Example

```csharp
using eSignASPLibrary;
using System;
using System.Collections.Generic;
using System.IO;

public class QuickStart
{
    public static void Main(string[] args)
    {
        // 1. Load PDF and convert to Base64
        byte[] pdfBytes = File.ReadAllBytes("document.pdf");
        string pdfBase64 = Convert.ToBase64String(pdfBytes);

        // 2. Create signing input
        var input = new eSignInputBuilder()
            .SetDocBase64(pdfBase64)
            .SetDocInfo("Contract_2024")
            .SetSignedBy("John Doe")
            .SetLocation("New York, NY")
            .SetReason("Document Approval")
            .SignFirstPage()
            .SetCoordinates(eSign.Coordinates.Bottom_Right)
            .Build();

        // 3. Initialize eSign service
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
            eSignURL: "https://gateway.example.com/api/2.1/",
            eSignURLV2: "https://gateway.example.com/api/2.5/",
            eSignCheckStatusURL: "https://status.example.com"
        );

        // 4. Get gateway parameters
        var result = esign.GetGateWayParam(
            new List<eSignInput> { input },
            signerID: "SIGNER_001",
            transactionID: "TXN_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            resposeURL: "https://yourapp.com/callback",
            redirectUrl: "https://yourapp.com/success",
            tempFolderPath: @"C:\Temp\eSign",
            eSignAPIVersion: eSign.eSignAPIVersion.V3,
            authMode: eSign.AuthMode.OTP
        );

        // 5. Handle response
        if (result.ReturnStatus == eSign.status.Success)
        {
            Console.WriteLine($"Success! Gateway Parameter: {result.GatewayParameter}");
            // Redirect user to gateway for authentication
        }
        else
        {
            Console.WriteLine($"Error: {result.ErrorMessage} (Code: {result.ErrorCode})");
        }
    }
}
```

---

## New Features

### 1. Builder Pattern (NEW!)

Easy-to-use fluent API for creating signing inputs:

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Invoice_2024")
    .SetSignedBy("John Doe")
    .SetLocation("Los Angeles, CA")
    .SetReason("Invoice Approval")
    .SignAllPages()
    .SetCoordinates(eSign.Coordinates.Bottom_Right)
    .SetFontSize(10)
    .ShowGreenTick()
    .EnableCoSign()
    .Build();
```

### 2. Text Search-Based Signature Placement (NEW!)

Automatically find text in PDF and place signature. The search engine now handles multi-chunk text, meaning it can find text like "Inspector's Signature" even when the PDF stores it as separate chunks (e.g., "Inspector's " + "Signature").

```csharp
// Simple: Find "Sign here:" and place signature to the right
// Works even if text spans multiple PDF text chunks
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Contract")
    .SearchAndPlaceSignature("Sign here:", out searchResult)
    .SetSignedBy("Jane Smith")
    .SetLocation("Boston, MA")
    .Build();

// Check if text was found
if (!searchResult.Found)
{
    Console.WriteLine($"Text not found: {searchResult.ErrorMessage}");
    // Fallback to fixed coordinates...
}
else
{
    Console.WriteLine($"✅ Signature placed on page {searchResult.PageNumber}");
}
```

**Real-world Example:**
```csharp
// Find "Inspector's Signature" in inspection reports
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Inspector's Signature", 150, 60, SignaturePlacement.Below, out searchResult)
    .SetSignedBy("Inspector Name")
    .Build();
```

### 3. Advanced Text Search with Custom Placement

```csharp
var searchConfig = new PdfTextSearchSignature
{
    SearchText = "Authorized Signature:",
    SignatureWidth = 200,
    SignatureHeight = 80,
    XOffset = 15,
    YOffset = -5,
    PageNumber = 1,              // Search only page 1
    MatchIndex = 0,              // First occurrence
    IgnoreCase = true,           // Case-insensitive
    Placement = SignaturePlacement.Below
};

PdfTextSearchResult result;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Agreement")
    .SetSignaturePositionByTextSearch(searchConfig, out result)
    .SetSignedBy("Michael Brown")
    .Build();

if (result.Found)
{
    Console.WriteLine($"Signature at: {result.CoordinateString}");
}
```

---

## Integration Guide

### ASP.NET Core Integration

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register eSign as a scoped service
    services.AddScoped<eSign>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        return new eSign(
            pfxFilePath: config["eSign:PfxFilePath"],
            pfxPassword: config["eSign:PfxPassword"],
            pfxAlias: config["eSign:PfxAlias"],
            isProxyRequired: false,
            proxyIP: null,
            proxyPort: 0,
            proxyUserName: null,
            proxyPassword: null,
            aspID: config["eSign:ASPID"],
            eSignURL: config["eSign:GatewayURL"],
            eSignURLV2: config["eSign:GatewayURLV2"],
            eSignCheckStatusURL: config["eSign:StatusURL"]
        );
    });
}
```

In `appsettings.json`:
```json
{
  "eSign": {
    "ASPID": "YOUR_ASP_ID",
    "GatewayURL": "https://gateway.example.com/api/2.1/",
    "GatewayURLV2": "https://gateway.example.com/api/2.5/",
    "StatusURL": "https://status.example.com",
    "PfxFilePath": "certificates/signing.pfx",
    "PfxPassword": "your-password",
    "PfxAlias": "alias"
  }
}
```

### Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class SigningController : ControllerBase
{
    private readonly eSign _eSignService;

    public SigningController(eSign eSignService)
    {
        _eSignService = eSignService;
    }

    [HttpPost("initiate")]
    public IActionResult InitiateSigning([FromBody] SigningRequest request)
    {
        // Create signing input
        PdfTextSearchResult searchResult;
        var input = new eSignInputBuilder()
            .SetDocBase64(request.PdfBase64)
            .SetDocInfo(request.DocumentName)
            .SearchAndPlaceSignature(request.SearchText, out searchResult)
            .SetSignedBy(request.SignerName)
            .SetLocation(request.Location)
            .Build();

        // Get gateway parameters
        var result = _eSignService.GetGateWayParam(
            new List<eSignInput> { input },
            signerID: request.SignerId,
            transactionID: Guid.NewGuid().ToString(),
            resposeURL: "https://yourapp.com/api/signing/callback",
            redirectUrl: "https://yourapp.com/success",
            tempFolderPath: Path.GetTempPath()
        );

        if (result.ReturnStatus == eSign.status.Success)
        {
            return Ok(new { gatewayUrl = result.GatewayParameter });
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost("callback")]
    public IActionResult HandleCallback([FromForm] string responseXML)
    {
        // Process the signed document
        var result = _eSignService.GetSigedDocument(
            responseXML,
            "path/to/presigned/file"
        );

        if (result.ReturnStatus == eSign.status.Success)
        {
            string signedPdfBase64 = result.ReturnValues[0].SignedDocument;
            // Save or process the signed document
            return Ok("Document signed successfully");
        }

        return BadRequest("Signing failed");
    }
}
```

---

## API Documentation

### Main Classes

#### `eSign`
Main class for signing operations.

**Constructor**:
```csharp
public eSign(
    string pfxFilePath,           // Path to certificate file
    string pfxPassword,           // Certificate password
    string pfxAlias,              // Certificate alias
    bool isProxyRequired,         // Use proxy?
    string proxyIP,               // Proxy IP (if required)
    int proxyPort,                // Proxy port (if required)
    string proxyUserName,         // Proxy username (if required)
    string proxyPassword,         // Proxy password (if required)
    string aspID,                 // Application Service Provider ID
    string eSignURL,              // Gateway URL
    string eSignURLV2,            // Gateway URL V2
    string eSignCheckStatusURL    // Status check URL
)
```

**Key Methods**:
- `GetGateWayParam()` - Generate parameters for signing gateway
- `GetSigedDocument()` - Retrieve signed document after gateway callback
- `GetStatus()` - Check transaction status

#### `eSignInputBuilder` (NEW!)
Builder for creating `eSignInput` objects.

**Common Methods**:
- `SetDocBase64(string)` - Set PDF content
- `SetDocInfo(string)` - Set document identifier
- `SetSignedBy(string)` - Set signer name
- `SetLocation(string)` - Set signing location
- `SetReason(string)` - Set signing reason
- `SignAllPages()` / `SignFirstPage()` / `SignLastPage()` - Page selection
- `SetCoordinates(eSign.Coordinates)` - Set position
- `SearchAndPlaceSignature(string, out PdfTextSearchResult)` - **NEW: Text search**
- `Build()` - Create eSignInput object

#### `PdfTextSearchSignature` (NEW!)
Configuration for text search-based positioning.

**Properties**:
- `SearchText` - Text to find in PDF
- `SignatureWidth` / `SignatureHeight` - Signature box dimensions
- `XOffset` / `YOffset` - Position offsets
- `PageNumber` - Specific page to search (optional)
- `MatchIndex` - Which occurrence to use (0 = first)
- `IgnoreCase` - Case-insensitive search
- `Placement` - Position relative to found text

### Enumerations

#### `eSign.Coordinates`
```csharp
Top_Left, Top_Center, Top_Right,
Middle_Left, Middle_Center, Middle_Right,
Bottom_Left, Bottom_Center, Bottom_Right
```

#### `eSign.PageToBeSigned`
```csharp
ALL, FIRST, LAST, EVEN, ODD, SPECIFY, PAGE_LEVEL
```

#### `eSign.DocType`
```csharp
Pdf, Hash, eMandate
```

#### `eSign.AuthMode`
```csharp
OTP, FP, IRIS, FACE
```

#### `SignaturePlacement` (NEW!)
```csharp
RightOf, LeftOf, Above, Below, AtPosition
```

---

## Examples

### Example 1: Sign with Custom Coordinates

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Document_001")
    .SetCustomCoordinates("100,150,250,200")  // x1,y1,x2,y2
    .SetSignedBy("User Name")
    .Build();
```

### Example 2: Sign Specific Pages

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Multi_Page_Doc")
    .SignSpecificPages("1,3,5,7")  // Pages 1, 3, 5, and 7
    .SetCoordinates(eSign.Coordinates.Bottom_Right)
    .SetSignedBy("User Name")
    .Build();
```

### Example 3: Page-Level Coordinates

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Complex_Doc")
    .SetPageLevelCoordinates("1,50,700,200,750;2,100,650,250,700")
    // Page 1: (50,700) to (200,750)
    // Page 2: (100,650) to (250,700)
    .SetSignedBy("User Name")
    .Build();
```

### Example 4: Hash Signing

```csharp
var input = new eSignInputBuilder()
    .ForHashSigning(hashValue, "DocumentHash_12345")
    .SetPdfUrl("https://example.com/document.pdf")
    .Build();
```

### Example 5: eMandate XML Signing

```csharp
var input = new eSignInputBuilder()
    .ForeMandateSigning(xmlContent, "Mandate_67890")
    .SetPdfUrl("https://example.com/mandate.xml")
    .Build();
```

For more examples, see:
- [USAGE_EXAMPLES.md](eSign/USAGE_EXAMPLES.md)
- [TEXT_SEARCH_EXAMPLES.cs](eSign/TEXT_SEARCH_EXAMPLES.cs)
- [ERROR_HANDLING_PATTERN.md](ERROR_HANDLING_PATTERN.md)

---

## Security Considerations

### Important Security Warnings

1. **Certificate Security**:
   - Store certificates securely (Azure Key Vault, AWS Secrets Manager)
   - Never commit certificates to source control
   - Use strong passwords for certificate files
   - Rotate certificates regularly

2. **Credential Management**:
   ```csharp
   // ✅ GOOD - Use environment variables or configuration
   aspID: Environment.GetEnvironmentVariable("ESIGN_ASP_ID")

   // ❌ BAD - Never hardcode
   aspID: "hardcoded_asp_id"
   ```

3. **Input Validation**:
   - Always validate PDF content before processing
   - Sanitize user inputs (names, locations, reasons)
   - Validate Base64-encoded content

4. **Error Handling**:
   - Don't expose sensitive information in error messages
   - Log errors securely without PII
   - Use the built-in error handling pattern

5. **Network Security**:
   - Use HTTPS for all gateway communications
   - Verify SSL/TLS certificates
   - Configure proxy settings securely

### Best Practices

```csharp
// Store sensitive configuration securely
var aspId = Configuration["eSign:ASPID"];
var pfxPassword = Configuration["eSign:PfxPassword"];

// Validate inputs
if (!IsValidBase64(pdfBase64))
{
    throw new ArgumentException("Invalid PDF content");
}

// Use proper error handling (no exceptions thrown)
PdfTextSearchResult result;
var input = builder.SearchAndPlaceSignature(searchText, out result);
if (!result.Found)
{
    // Handle gracefully without crashing
    logger.LogWarning($"Text search failed: {result.ErrorMessage}");
}
```

---

## Troubleshooting

### Common Issues

**Issue**: Text search fails
**Solution**: Check if the search text exists in the PDF, use case-insensitive search, or provide fallback coordinates

**Issue**: Certificate errors
**Solution**: Verify certificate path, password, and alias are correct

**Issue**: Gateway timeout
**Solution**: Check network connectivity and gateway URL configuration

**Issue**: Invalid coordinates
**Solution**: Ensure coordinates are within PDF page bounds (standard A4: 595 x 842 points)

---

## License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

**Important**: This library includes iText PDF library, which is also AGPL-licensed. For commercial use without open-sourcing your application, you may need a commercial license from iText Software.

See [LICENSE.txt](LICENSE.txt) for details and [SECURITY.md](SECURITY.md) for security considerations.

---

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Quick Contribution Guide
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes
4. Run tests and ensure build succeeds
5. Commit: `git commit -m "feat: your feature description"`
6. Push: `git push origin feature/your-feature`
7. Create a Pull Request

---

## Changelog

### Version 2.0.0.17 (Latest)
- ✅ Builder pattern for eSignInput creation
- ✅ PDF text search with automatic signature placement
- ✅ Multiple placement strategies (RightOf, LeftOf, Above, Below, AtPosition)
- ✅ Graceful error handling (no exceptions)
- ✅ Comprehensive documentation and examples
- ✅ C# 7.3 compatible syntax (pre-declared out variables)

---

## Acknowledgments

- **iText PDF Library** - PDF manipulation
- **BouncyCastle** - Cryptographic operations
- **System.Security.Cryptography.Xml** - XML signature support

---

**Built with .NET Framework | Powered by Digital Signatures**

Repository: https://github.com/emudhra-integration-sdk/dotnet-framework-esign-sdk
