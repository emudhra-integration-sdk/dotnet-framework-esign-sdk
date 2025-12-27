# Error Handling Pattern - Text Search Feature

## Overview

The text search feature follows the **traditional eSignServiceReturn pattern** used throughout the library:
- ✅ **No exceptions thrown** - application won't crash
- ✅ **Errors returned in result objects** - similar to `eSignServiceReturn`
- ✅ **Graceful error handling** - allows fallback strategies
- ✅ **Consistent with existing code** - matches library conventions

---

## ❌ OLD Pattern (Throws Exceptions - Don't Use)

```csharp
// ❌ This would crash the application if text not found
try
{
    var input = new eSignInputBuilder()
        .SetDocBase64(pdfBase64)
        .SearchAndPlaceSignature("NonExistentText")
        .Build();
}
catch (Exception ex)
{
    // Application stopped!
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## ✅ NEW Pattern (Returns Errors - Use This)

### **Pattern 1: Simple Error Checking**

```csharp
// ✅ Application continues even if text not found
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Sign here:", out var searchResult)
    .SetSignedBy("John Doe")
    .Build();

// Check if search was successful
if (!searchResult.Found)
{
    // Handle error gracefully
    Console.WriteLine($"Text search failed: {searchResult.ErrorMessage}");

    // Use fallback coordinates
    input = new eSignInputBuilder()
        .SetDocBase64(pdfBase64)
        .SetCoordinates(eSign.Coordinates.Bottom_Right)
        .SetSignedBy("John Doe")
        .Build();
}
else
{
    Console.WriteLine($"✅ Text found on page {searchResult.PageNumber}");
}
```

### **Pattern 2: Return Error Like eSignServiceReturn**

```csharp
public eSignServiceReturn SignDocument(string pdfBase64, string searchText)
{
    var input = new eSignInputBuilder()
        .SetDocBase64(pdfBase64)
        .SearchAndPlaceSignature(searchText, out var searchResult)
        .SetSignedBy("User")
        .Build();

    // ✅ Return error similar to eSignServiceReturn pattern
    if (!searchResult.Found)
    {
        return new eSignServiceReturn
        {
            ReturnStatus = eSign.status.Failure,
            ErrorCode = "TXT-001",
            ErrorMessage = $"Text search failed: {searchResult.ErrorMessage}"
        };
    }

    // Continue with signing...
    return ProcessSigning(input);
}
```

### **Pattern 3: With Validation**

```csharp
public eSignServiceReturn SignWithValidation(string pdfBase64)
{
    var searchConfig = new PdfTextSearchSignature
    {
        SearchText = "Sign:",
        SignatureWidth = 150,
        SignatureHeight = 60
    };

    // ✅ Validate configuration first
    if (!searchConfig.Validate(out string validationError))
    {
        return new eSignServiceReturn
        {
            ReturnStatus = eSign.status.Failure,
            ErrorCode = "VAL-001",
            ErrorMessage = $"Invalid configuration: {validationError}"
        };
    }

    // Proceed with search
    var input = new eSignInputBuilder()
        .SetDocBase64(pdfBase64)
        .SetSignaturePositionByTextSearch(searchConfig, out var searchResult)
        .Build();

    // ✅ Check search result
    if (!searchResult.Found)
    {
        return new eSignServiceReturn
        {
            ReturnStatus = eSign.status.Failure,
            ErrorCode = "TXT-002",
            ErrorMessage = searchResult.ErrorMessage
        };
    }

    // Success - continue signing
    return ProcessSigning(input);
}
```

---

## Error Codes Reference

| Error Code | Description | Source |
|------------|-------------|--------|
| `TXT-001` | Text not found in PDF | Text search failed |
| `TXT-002` | Page out of range | Invalid page number |
| `TXT-003` | PDF decode error | Corrupted/invalid PDF |
| `VAL-001` | Invalid configuration | Validation failed |
| `VAL-002` | DocBase64 not set | Missing required data |

---

## PdfTextSearchResult Object

```csharp
public class PdfTextSearchResult
{
    public bool Found { get; set; }              // true if text found
    public int PageNumber { get; set; }          // Page where text was found
    public float TextX { get; set; }             // Text X position
    public float TextY { get; set; }             // Text Y position
    public float TextWidth { get; set; }         // Text width
    public float TextHeight { get; set; }        // Text height
    public int SignatureX1 { get; set; }         // Signature X1 coordinate
    public int SignatureY1 { get; set; }         // Signature Y1 coordinate
    public int SignatureX2 { get; set; }         // Signature X2 coordinate
    public int SignatureY2 { get; set; }         // Signature Y2 coordinate
    public string CoordinateString { get; }      // "x1,y1,x2,y2"
    public string ErrorMessage { get; set; }     // Error details if Found = false
    public int TotalMatches { get; set; }        // Total occurrences found
    public string FoundText { get; set; }        // The actual text found
}
```

---

## Common Error Scenarios

### 1. Text Not Found

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("NonExistentText", out var result)
    .Build();

if (!result.Found)
{
    // ✅ No exception! Application continues
    Console.WriteLine(result.ErrorMessage);
    // Output: "Text 'NonExistentText' not found in the document."
}
```

### 2. Page Out of Range

```csharp
var searchConfig = new PdfTextSearchSignature
{
    SearchText = "Sign:",
    PageNumber = 999  // PDF only has 5 pages
};

var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetSignaturePositionByTextSearch(searchConfig, out var result)
    .Build();

if (!result.Found)
{
    Console.WriteLine(result.ErrorMessage);
    // Output: "Page number 999 is out of range. PDF has 5 pages."
}
```

### 3. Invalid Configuration

```csharp
var searchConfig = new PdfTextSearchSignature
{
    SearchText = "",  // Empty!
    SignatureWidth = -10  // Negative!
};

if (!searchConfig.Validate(out string error))
{
    Console.WriteLine(error);
    // Output: "SearchText cannot be null or empty."
}
```

### 4. PDF Decode Error

```csharp
var input = new eSignInputBuilder()
    .SetDocBase64("invalid_base64")
    .SearchAndPlaceSignature("Text", out var result)
    .Build();

if (!result.Found)
{
    Console.WriteLine(result.ErrorMessage);
    // Output: "Error searching PDF: [decode error details]"
}
```

### 5. DocBase64 Not Set

```csharp
var input = new eSignInputBuilder()
    // Forgot to set DocBase64!
    .SearchAndPlaceSignature("Text", out var result)
    .Build();

if (!result.Found)
{
    Console.WriteLine(result.ErrorMessage);
    // Output: "DocBase64 must be set before performing text search."
}
```

---

## Complete Workflow Example

```csharp
public eSignServiceReturn CompleteSigningWorkflow(string pdfBase64)
{
    // Step 1: Try text search
    var input = new eSignInputBuilder()
        .SetDocBase64(pdfBase64)
        .SetDocInfo("Document_001")
        .SearchAndPlaceSignature("Sign here:", out var searchResult)
        .SetSignedBy("John Doe")
        .SetLocation("New York")
        .SetReason("Approval")
        .Build();

    // Step 2: Check result and apply fallback if needed
    if (!searchResult.Found)
    {
        Console.WriteLine($"⚠️ Text search failed: {searchResult.ErrorMessage}");
        Console.WriteLine("⚠️ Using fallback coordinates...");

        // Rebuild with fallback coordinates
        input = new eSignInputBuilder()
            .SetDocBase64(pdfBase64)
            .SetDocInfo("Document_001")
            .SetCoordinates(eSign.Coordinates.Bottom_Right)
            .SignFirstPage()
            .SetSignedBy("John Doe")
            .SetLocation("New York")
            .SetReason("Approval")
            .Build();
    }
    else
    {
        Console.WriteLine($"✅ Text found on page {searchResult.PageNumber}");
        Console.WriteLine($"   Signature will be at: {searchResult.CoordinateString}");
    }

    // Step 3: Initialize eSign service
    var esign = new eSign(
        pfxFilePath: "certificate.pfx",
        pfxPassword: "password",
        pfxAlias: "alias",
        isProxyRequired: false,
        proxyIP: null,
        proxyPort: 0,
        proxyUserName: null,
        proxyPassword: null,
        aspID: Environment.GetEnvironmentVariable("ESIGN_ASP_ID"),
        eSignURL: Environment.GetEnvironmentVariable("ESIGN_GATEWAY_URL"),
        eSignURLV2: Environment.GetEnvironmentVariable("ESIGN_GATEWAY_V2_URL"),
        eSignCheckStatusURL: Environment.GetEnvironmentVariable("ESIGN_STATUS_URL")
    );

    // Step 4: Get gateway parameters
    var result = esign.GetGateWayParam(
        new List<eSignInput> { input },
        signerID: "SIGNER_001",
        transactionID: Guid.NewGuid().ToString(),
        resposeURL: "https://yourapp.com/callback",
        redirectUrl: "https://yourapp.com/success",
        tempFolderPath: @"C:\Temp\eSign"
    );

    // Step 5: Return result (follows eSignServiceReturn pattern)
    if (result.ReturnStatus == eSign.status.Success)
    {
        Console.WriteLine($"✅ Success! Gateway Parameter: {result.GatewayParameter}");
    }
    else
    {
        Console.WriteLine($"❌ Error: {result.ErrorMessage} (Code: {result.ErrorCode})");
    }

    return result;
}
```

---

## Key Benefits

| Benefit | Description |
|---------|-------------|
| **No Application Crashes** | Errors returned, not thrown |
| **Graceful Degradation** | Can fallback to default coordinates |
| **Consistent Pattern** | Matches existing `eSignServiceReturn` |
| **Better UX** | Application continues running |
| **Easy Integration** | Works with existing error handling |

---

## Migration Guide

If you were using the old exception-throwing pattern, update your code:

### Before (Old - Don't Use)
```csharp
try
{
    var input = new eSignInputBuilder()
        .SearchAndPlaceSignature("Text")
        .Build();
}
catch (Exception ex)
{
    // Handle exception
}
```

### After (New - Use This)
```csharp
var input = new eSignInputBuilder()
    .SearchAndPlaceSignature("Text", out var result)
    .Build();

if (!result.Found)
{
    // Handle error gracefully
    Console.WriteLine(result.ErrorMessage);
    // Apply fallback...
}
```

---

**This pattern ensures your application remains stable and follows the library's existing conventions!** ✅
