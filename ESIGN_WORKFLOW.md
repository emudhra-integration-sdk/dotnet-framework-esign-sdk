# eSign Library - Complete Workflow Guide

## Table of Contents
1. [Text Search Features](#text-search-features)
2. [eSign Workflow Overview](#esign-workflow-overview)
3. [Configuration](#configuration)
4. [Single Occurrence Text Search](#single-occurrence-text-search)
5. [Multi-Occurrence Text Search](#multi-occurrence-text-search)
6. [Complete eSign Flow](#complete-esign-flow)
7. [API Endpoints](#api-endpoints)

---

## Text Search Features

### Single Occurrence Text Search
Search for text and place **one signature** at the first match (or specific match index).

```csharp
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Document_Info")
    .SearchAndPlaceSignature("Inspector's Signature",
        150, 60, // Width, Height
        SignaturePlacement.Below,
        0, -10, // XOffset, YOffset
        out searchResult)
    .SetSignedBy("Inspector Name")
    .SetLocation("Inspection Site")
    .SetReason("Inspection Completed")
    .Build();

if (searchResult.Found)
{
    Console.WriteLine($"Found on page {searchResult.PageNumber}");
    Console.WriteLine($"Coordinates: {searchResult.CoordinateString}");
}
```

### Multi-Occurrence Text Search
Search for text and place **signatures at ALL occurrences** across all pages automatically.

```csharp
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Document_Info")
    .SearchAndPlaceSignatureAtAllOccurrences("Sign Here:",
        SignaturePlacement.Below,
        out searchResult)
    .SetSignedBy("Signer Name")
    .Build();

if (searchResult.Found)
{
    Console.WriteLine($"Found {searchResult.TotalMatches} occurrences");
    Console.WriteLine($"Placed {searchResult.AllMatches.Count} signatures");

    // Details of all matches
    foreach (var match in searchResult.AllMatches)
    {
        Console.WriteLine($"Page {match.PageNumber}: {match.CoordinateString}");
    }
}
```

**Key Differences:**

| Feature | Single Occurrence | Multi-Occurrence |
|---------|------------------|------------------|
| Method | `SearchAndPlaceSignature()` | `SearchAndPlaceSignatureAtAllOccurrences()` |
| Signatures Placed | 1 (first match or specified index) | All matches found |
| Mode Used | SPECIFY with CustomCoordinates | PAGE_LEVEL with PageLevelCoordinates |
| Use Case | Sign at one specific location | Sign at all occurrences (e.g., "Sign Here:" on every page) |

---

## eSign Workflow Overview

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Build eSignInput with text search                       │
│    - Search for text, calculate signature positions         │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Call GetGateWayParam                                     │
│    - Returns gateway parameters (txnref)                    │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Form POST to eMudhra Authentication URL                 │
│    - Aadhaar: AadhaareSign.jsp                             │
│    - PAN (v3): index.jsp                                   │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. User performs eSign on eMudhra page                     │
│    - OTP/Biometric authentication                          │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
         ┌───────┴────────┐
         │                │
         ▼                ▼
    ┌────────┐      ┌─────────┐
    │ v2 Flow│      │ v3 Flow │
    └───┬────┘      └────┬────┘
        │                │
        ▼                ▼
┌──────────────┐  ┌──────────────────┐
│ Redirect to  │  │ Redirect to      │
│ Response URL │  │ Redirect URL     │
│ (with XML)   │  │ (user page)      │
└──────┬───────┘  └────────┬─────────┘
       │                   │
       │                   ▼
       │          ┌─────────────────────┐
       │          │ eMudhra POSTs XML   │
       │          │ to Response URL     │
       │          │ (webhook)           │
       │          └──────────┬──────────┘
       │                     │
       ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. Response URL receives XML from eMudhra                  │
│    - Extract txn from XML                                  │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. Call GetStatus(txn)                                     │
│    - Check signing status                                  │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ 7. Call GetSignedDocument(responseXML, tempFilePath)       │
│    - Retrieve signed PDF(s)                                │
└─────────────────────────────────────────────────────────────┘
```

---

## Configuration

### Initialize eSign Object

```csharp
var esign = new eSign(
    PFXFilePath: @"C:\certificates\certificate.pfx",
    PFXPassword: "your_password",
    PFXAlias: "your_alias",
    IsProxyRequired: false,
    ProxyIP: null,
    ProxyPort: 0,
    ProxyUserName: null,
    ProxyPassword: null,
    ASPID: "YOUR_ASP_ID",

    // Sandbox URLs (eMudhra)
    eSignURL: "https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/eSignRequest",
    eSignURLV2: "https://authenticate.sandbox.emudhra.com/eSignExternal/v2_1/signDoc",
    eSignCheckStatusURL: "https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/checkSignStatusAPI",

    SignatureContents: 21000
);
```

### Production URLs
For production, replace with production URLs (contact eMudhra for production endpoints).

---

## Complete eSign Flow

### Step 1: Build eSignInput with Text Search

```csharp
// Single occurrence
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Contract_Agreement")
    .SearchAndPlaceSignature("Signature:", 150, 60,
        SignaturePlacement.Below, 0, -10, out searchResult)
    .SetSignedBy("John Doe")
    .SetLocation("Mumbai")
    .SetReason("Agreement Signing")
    .Build();

// OR Multi-occurrence
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SetDocInfo("Multi_Page_Contract")
    .SearchAndPlaceSignatureAtAllOccurrences("Sign Here:",
        SignaturePlacement.Below, out searchResult)
    .SetSignedBy("John Doe")
    .Build();
```

### Step 2: Get Gateway Parameters

```csharp
var result = esign.GetGateWayParam(
    eSignInputs: new List<eSignInput> { input },
    signerID: "SIGNER_001",
    transactionID: "TXN_" + DateTime.Now.ToString("yyyyMMddHHmmss"),

    // Response URL: PUBLIC endpoint where eMudhra will POST XML (webhook)
    resposeURL: "https://yourdomain.com/api/esign/response",

    // Redirect URL: Where user is redirected after signing (v3 only)
    redirectUrl: "https://yourdomain.com/success",

    TempFolderPath: @"C:\temp\esign",
    eSignAPIVersion: eSign.eSignAPIVersion.V3,
    authMode: eSign.AuthMode.OTP
);

if (result.ReturnStatus == eSign.status.Success)
{
    string gatewayParam = result.GatewayParam;
    // Store gatewayParam, ASPID, TransactionID for later use
}
```

**Important Notes about URLs:**

- **Response URL** (`resposeURL`):
  - MUST be a **publicly accessible** HTTPS endpoint
  - eMudhra will POST the signed XML to this URL
  - Acts as a webhook - your server should process the XML here
  - Required for both v2 and v3

- **Redirect URL** (`redirectUrl`):
  - Used only in **v3** flow
  - User is redirected here after clicking "Perform eSign"
  - This is where the user lands after signing
  - Show success/status page to the user here
  - In **v2**, user is redirected directly to Response URL

### Step 3: Form POST to eMudhra

Based on eSign type, post to different URLs:

#### For Aadhaar-based Signing (eSign v2/v3):
```html
<form action="https://authenticate.sandbox.emudhra.com/AadhaareSign.jsp"
      method="post" id="form1">
    <input type="text" name="txnref" id="txnref" value="<%= gatewayParam %>" />
</form>
<script>
    document.getElementById('form1').submit();
</script>
```

#### For PAN-based Signing (eSign v3):
```html
<form action="https://authenticate.sandbox.emudhra.com/index.jsp"
      method="post" id="form1">
    <input type="text" name="txnref" id="txnref" value="<%= gatewayParam %>" />
</form>
<script>
    document.getElementById('form1').submit();
</script>
```

**C# Example:**
```csharp
// Redirect user to signing page
string formHtml = $@"
<html>
<body onload='document.getElementById(""form1"").submit()'>
    <form action='https://authenticate.sandbox.emudhra.com/AadhaareSign.jsp'
          method='post' id='form1'>
        <input type='text' name='txnref' value='{gatewayParam}' />
    </form>
</body>
</html>";

Response.Write(formHtml);
```

### Step 4: User Performs eSign

- User is now on eMudhra's authentication page
- User enters OTP/biometric authentication
- User clicks "Perform eSign"

### Step 5: Handle Response

#### eSign v3 Flow (Recommended):

**Redirect URL (User sees this):**
```csharp
// https://yourdomain.com/success
protected void Page_Load(object sender, EventArgs e)
{
    // Show success message to user
    string txnId = Session["TransactionID"]?.ToString();
    lblMessage.Text = $"Signing completed! Transaction ID: {txnId}";
    lblMessage.Text += "<br/>Your document will be processed shortly.";
}
```

**Response URL (Webhook - processes in background):**
```csharp
// https://yourdomain.com/api/esign/response
protected void Page_Load(object sender, EventArgs e)
{
    try
    {
        // Read posted XML from eMudhra
        Stream str = Request.InputStream;
        if (str.Length > 0)
        {
            int strLen = Convert.ToInt32(str.Length);
            byte[] strArr = new byte[strLen];
            str.Read(strArr, 0, strLen);

            string response = Encoding.ASCII.GetString(strArr);
            string txnref = HttpUtility.UrlDecode(response);
            string[] array = txnref.Split('&');
            string value = array[0].Substring(7);

            // Decode the response
            string decodedResponse = Base64Decode(value);
            string txn = decodedResponse.Split('|')[0];

            // Initialize eSign object
            var esign = new eSign(
                PFXFilePath, PFXPassword, PFXAlias,
                false, null, 0, null, null,
                ASPID, eSignURL, eSignURLV2, eSignCheckStatusURL
            );

            // Get status
            var statusResult = esign.GetStatus(txn);

            if (statusResult.ReturnStatus == eSign.status.Success)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(statusResult.ResponseXML);

                var signRespElement = xml.SelectSingleNode("//EsignResp");
                string status = signRespElement.Attributes["status"].Value;
                string txnId = signRespElement.Attributes["txn"].Value;

                // Save the response XML
                string xmlPath = Path.Combine(signedFolder, $"{txnId}.xml");
                File.WriteAllText(xmlPath, statusResult.ResponseXML);

                if (status == "1") // Success
                {
                    // Get signed document
                    string tempSigFile = Path.Combine(tempFolder, $"{txnId}.sig");
                    var signedResult = esign.GetSignedDocument(
                        statusResult.ResponseXML,
                        tempSigFile
                    );

                    if (signedResult.ReturnStatus == eSign.status.Success)
                    {
                        List<eSignReturnDocument> returnDocuments = signedResult.ReturnValues;

                        for (int i = 0; i < returnDocuments.Count; i++)
                        {
                            string pdfBase64 = returnDocuments[i].SignedDocument;
                            byte[] signedBytes = Convert.FromBase64String(pdfBase64);

                            string pdfPath = Path.Combine(signedFolder,
                                $"{txnId}_doc{i}.pdf");
                            File.WriteAllBytes(pdfPath, signedBytes);

                            // Update database, send email, etc.
                        }
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Log error
        LogError(ex);
    }
}

private string Base64Decode(string base64EncodedData)
{
    byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
    return Encoding.UTF8.GetString(base64EncodedBytes);
}
```

#### eSign v2 Flow:

In v2, the XML comes directly to the Response URL (no separate redirect page).

```csharp
// Response URL processes everything and shows result to user
protected void Page_Load(object sender, EventArgs e)
{
    // Same processing as v3 Response URL above
    // But also show success page to user since they land here

    if (signingSuccessful)
    {
        Response.Write("<h2>Document signed successfully!</h2>");
        Response.Write($"<p>Transaction ID: {txnId}</p>");
    }
}
```

---

## API Endpoints

### Sandbox URLs (eMudhra)

| Purpose | URL |
|---------|-----|
| **Aadhaar Signing Page** | `https://authenticate.sandbox.emudhra.com/AadhaareSign.jsp` |
| **PAN Signing Page (v3)** | `https://authenticate.sandbox.emudhra.com/index.jsp` |
| **eSign v3 API** | `https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/eSignRequest` |
| **eSign v2 API** | `https://authenticate.sandbox.emudhra.com/eSignExternal/v2_1/signDoc` |
| **Check Status API** | `https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/checkSignStatusAPI` |

### Your Application Endpoints

| Endpoint | Type | Purpose |
|----------|------|---------|
| **Response URL** | Webhook (POST) | Receives XML from eMudhra after signing |
| **Redirect URL** | User Page (GET) | Where user lands after signing (v3 only) |

**Example Configuration:**
```
Response URL:  https://yourdomain.com/api/esign/response
Redirect URL:  https://yourdomain.com/esign/success
```

---

## Code Examples

### Complete Example: Single Signature with eSign v3

```csharp
// 1. Build input with text search
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignature("Sign here:", 150, 60,
        SignaturePlacement.Below, 0, -10, out searchResult)
    .SetSignedBy("Amit Kumar")
    .Build();

if (!searchResult.Found)
{
    Console.WriteLine("Text not found!");
    return;
}

// 2. Initialize eSign
var esign = new eSign(
    "cert.pfx", "password", "alias",
    false, null, 0, null, null,
    "YOUR_ASP_ID",
    "https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/eSignRequest",
    "https://authenticate.sandbox.emudhra.com/eSignExternal/v2_1/signDoc",
    "https://authenticate.sandbox.emudhra.com/eSignExternal/v3_0/checkSignStatusAPI"
);

// 3. Get gateway parameters
var result = esign.GetGateWayParam(
    new List<eSignInput> { input },
    "SIGNER_001",
    "TXN_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
    "https://yourdomain.com/api/esign/response",
    "https://yourdomain.com/success",
    @"C:\temp",
    eSign.eSignAPIVersion.V3,
    eSign.AuthMode.OTP
);

if (result.ReturnStatus == eSign.status.Success)
{
    // 4. Form POST to eMudhra
    string formHtml = $@"
    <html><body onload='document.getElementById(""f"").submit()'>
    <form id='f' action='https://authenticate.sandbox.emudhra.com/AadhaareSign.jsp' method='post'>
        <input type='text' name='txnref' value='{result.GatewayParam}' />
    </form>
    </body></html>";

    Response.Write(formHtml);
}
```

### Complete Example: Multiple Signatures with eSign v3

```csharp
// Search for "Sign Here:" on all pages and place signatures
PdfTextSearchResult searchResult;
var input = new eSignInputBuilder()
    .SetDocBase64(pdfBase64)
    .SearchAndPlaceSignatureAtAllOccurrences("Sign Here:",
        SignaturePlacement.Below, out searchResult)
    .SetSignedBy("Multi Signer")
    .Build();

if (searchResult.Found)
{
    Console.WriteLine($"Will place {searchResult.TotalMatches} signatures");

    // Rest of the flow is same as single signature
    // GetGateWayParam, Form POST, etc.
}
```

---

## Troubleshooting

### Common Issues

1. **Response URL not receiving XML:**
   - Ensure Response URL is **publicly accessible** (use tools like ngrok for local testing)
   - Check firewall settings
   - Verify HTTPS certificate is valid

2. **Text not found:**
   - Check PDF encoding
   - Verify exact text match (case-sensitive by default)
   - Use `IgnoreCase = true` in search config

3. **Multiple signatures not placed:**
   - Ensure using `SearchAndPlaceSignatureAtAllOccurrences()` method
   - Check that `AllMatches` is populated in result

4. **Wrong signature position:**
   - All bugs fixed in this version!
   - Y-coordinate calculation fixed
   - CustomCoordinates now properly used
   - PageNumbers correctly set

---

## Support

For issues or questions:
- GitHub Issues: https://github.com/anthropics/claude-code/issues
- eMudhra Support: Contact your eMudhra representative

---

**Version:** 1.0.0
**Last Updated:** December 2025
