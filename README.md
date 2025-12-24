# eSign Library Kit (.NET Framework)

A comprehensive .NET Framework SDK for digital signature operations, providing PDF signing capabilities with support for multiple authentication methods and signature appearance customization.

## Table of Contents
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Building from Source](#building-from-source)
- [Quick Start](#quick-start)
- [Integration Guide](#integration-guide)
- [API Documentation](#api-documentation)
- [License](#license)
- [Security Considerations](#security-considerations)
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
- **Flexible Signature Appearance**:
  - Standard signatures
  - Image-based signatures
  - One-liner signatures
  - Advanced signatures with custom styling
  - Colored graphics signatures
  - Background image signatures
- **Document Processing**:
  - Page-level signing control (All, Even, Odd, First, Last, or specify pages)
  - Hash-based or full document signing
  - Document encryption/decryption
  - Content search functionality
- **Enterprise Features**:
  - Bank KYC integration
  - Proxy server support
  - Transaction management and verification
  - Full .NET Framework 4.8 support

---

## Requirements

### Runtime Requirements
- **.NET Framework**: 4.8 or higher
- **Operating System**: Windows (7, 8, 10, 11, Server 2012+)

### Build Requirements
- **Visual Studio**: 2017 or higher
- **.NET Framework SDK**: 4.8
- **MSBuild**: 15.0 or higher

---

## Installation

### Option 1: Use Pre-built DLL
The easiest way to use this library is with the pre-built DLL file:

1. Download `eSignASPLibrary.dll` from the releases page
2. Add it as a reference to your project in Visual Studio:
   - Right-click on References → Add Reference
   - Browse to the DLL location
   - Click OK

### Option 2: Build from Source
See [Building from Source](#building-from-source) section below

---

## Building from Source

### Using Visual Studio

1. **Clone or download this repository**:
   ```bash
   git clone <repository-url>
   cd NetFramework_eSignLibKit
   ```

2. **Open the solution**:
   - Open `eSignASPLibrary.sln` in Visual Studio

3. **Restore NuGet packages** (if any):
   - Right-click on Solution → Restore NuGet Packages

4. **Build the project**:
   - Build → Build Solution
   - Or press `Ctrl+Shift+B`

5. **Locate the built DLL**:
   The compiled DLL will be in `bin\Release\eSignASPLibrary.dll`

### Using MSBuild Command Line

```bash
# Restore packages (if using NuGet)
nuget restore eSignASPLibrary.sln

# Build in Release mode
msbuild eSignASPLibrary.sln /p:Configuration=Release

# The DLL will be in bin\Release\
```

### Build Output
- **DLL File**: `bin\Release\eSignASPLibrary.dll`
- **PDB File**: `bin\Release\eSignASPLibrary.pdb` (for debugging)

---

## Quick Start

### Basic PDF Signing Example

```csharp
using eSign;
using System;

public class QuickStart
{
    public static void Main(string[] args)
    {
        try
        {
            // 1. Configure eSign settings
            var settings = new eSignSettings
            {
                ASPID = "YOUR_ASP_ID",
                Gateway_URL = "https://gateway.example.com",
                ResponseURL = "https://yourapp.com/callback"
            };

            // 2. Build user information
            var userInfo = new UserInfoBuilder()
                .SeteMail("user@example.com")
                .SetFirstname("John")
                .SetLastname("Doe")
                .SetPhoneNumber("9876543210")
                .Build();

            // 3. Build eSign input
            var input = new eSignInputBuilder()
                .SeteSignSettings(settings)
                .SetUserInfo(userInfo)
                .SetPDFBase64("BASE64_ENCODED_PDF_CONTENT")
                .SetCoordinates(Coordinates.BOTTOMRIGHT)
                .SetAuthMode(AuthMode.OTP)
                .SetAppearanceType(AppearanceType.STANDARD)
                .Build();

            // 4. Initialize eSign and generate gateway parameters
            var esign = new eSign.eSign();
            var result = esign.GenerateGatewayParameters(input);

            // 5. Handle the response
            if (result.ReturnCode == "1")
            {
                Console.WriteLine($"Success! Gateway URL: {result.GatewayURL}");
                Console.WriteLine($"Response URL: {result.ResponseURL}");
                // Redirect user to gateway URL for signing
            }
            else
            {
                Console.WriteLine($"Error: {result.ErrorMsg}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
```

---

## Integration Guide

### ASP.NET Web Forms Integration

```csharp
// In your code-behind (.aspx.cs)
protected void SignDocument_Click(object sender, EventArgs e)
{
    var settings = new eSignSettings
    {
        ASPID = ConfigurationManager.AppSettings["eSignASPID"],
        Gateway_URL = ConfigurationManager.AppSettings["eSignGatewayURL"],
        ResponseURL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Callback.aspx"
    };

    var userInfo = new UserInfoBuilder()
        .SeteMail(txtEmail.Text)
        .SetFirstname(txtFirstName.Text)
        .SetLastname(txtLastName.Text)
        .SetPhoneNumber(txtPhone.Text)
        .Build();

    var input = new eSignInputBuilder()
        .SeteSignSettings(settings)
        .SetUserInfo(userInfo)
        .SetPDFBase64(GetPdfBase64())
        .SetCoordinates(Coordinates.BOTTOMRIGHT)
        .SetAuthMode(AuthMode.OTP)
        .Build();

    var esign = new eSign.eSign();
    var result = esign.GenerateGatewayParameters(input);

    if (result.ReturnCode == "1")
    {
        Response.Redirect(result.GatewayURL);
    }
    else
    {
        lblError.Text = result.ErrorMsg;
    }
}
```

In `Web.config`:
```xml
<appSettings>
  <add key="eSignASPID" value="YOUR_ASP_ID" />
  <add key="eSignGatewayURL" value="https://gateway.example.com" />
</appSettings>
```

### ASP.NET MVC Integration

```csharp
public class SigningController : Controller
{
    [HttpPost]
    public ActionResult InitiateSigning(SigningRequest request)
    {
        var settings = new eSignSettings
        {
            ASPID = ConfigurationManager.AppSettings["eSignASPID"],
            Gateway_URL = ConfigurationManager.AppSettings["eSignGatewayURL"],
            ResponseURL = Url.Action("Callback", "Signing", null, Request.Url.Scheme)
        };

        var userInfo = new UserInfoBuilder()
            .SeteMail(request.Email)
            .SetFirstname(request.FirstName)
            .SetLastname(request.LastName)
            .SetPhoneNumber(request.Phone)
            .Build();

        var input = new eSignInputBuilder()
            .SeteSignSettings(settings)
            .SetUserInfo(userInfo)
            .SetPDFBase64(request.PdfBase64)
            .Build();

        var esign = new eSign.eSign();
        var result = esign.GenerateGatewayParameters(input);

        if (result.ReturnCode == "1")
        {
            return Redirect(result.GatewayURL);
        }

        return View("Error", result.ErrorMsg);
    }

    [HttpPost]
    public ActionResult Callback(string response)
    {
        var esign = new eSign.eSign();
        var result = esign.CheckTransactionStatus(
            ConfigurationManager.AppSettings["eSignASPID"],
            Request.Form["txnId"],
            ConfigurationManager.AppSettings["eSignGatewayURL"]
        );

        if (result.ReturnCode == "1")
        {
            // Save signed document
            byte[] signedPdf = Convert.FromBase64String(result.SignedDoc);
            // Process the signed PDF
            return View("Success");
        }

        return View("Error", result.ErrorMsg);
    }
}
```

### Windows Forms Integration

```csharp
private void btnSign_Click(object sender, EventArgs e)
{
    var settings = new eSignSettings
    {
        ASPID = Properties.Settings.Default.eSignASPID,
        Gateway_URL = Properties.Settings.Default.eSignGatewayURL,
        ResponseURL = "http://localhost:8080/callback"
    };

    var userInfo = new UserInfoBuilder()
        .SeteMail(txtEmail.Text)
        .SetFirstname(txtFirstName.Text)
        .SetLastname(txtLastName.Text)
        .SetPhoneNumber(txtPhone.Text)
        .Build();

    var input = new eSignInputBuilder()
        .SeteSignSettings(settings)
        .SetUserInfo(userInfo)
        .SetPDFBase64(GetPdfBase64FromFile())
        .Build();

    var esign = new eSign.eSign();
    var result = esign.GenerateGatewayParameters(input);

    if (result.ReturnCode == "1")
    {
        System.Diagnostics.Process.Start(result.GatewayURL);
    }
    else
    {
        MessageBox.Show(result.ErrorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

---

## API Documentation

### Main Classes

#### `eSign`
The main facade class for all signing operations.

**Key Methods**:
- `GenerateGatewayParameters(eSignInput input)` - Generate parameters for signing gateway
- `CheckTransactionStatus(string aspId, string txnId, string gatewayUrl)` - Check signing status
- `VerifySignature(string signedPdfBase64)` - Verify digital signature
- `SignDocument(eSignInput input, string pkcs12Path, string password)` - Direct document signing

#### `eSignInputBuilder`
Builder pattern for creating `eSignInput` objects.

**Example**:
```csharp
var input = new eSignInputBuilder()
    .SeteSignSettings(settings)
    .SetUserInfo(userInfo)
    .SetPDFBase64(pdfBase64)
    .SetCoordinates(Coordinates.BOTTOMRIGHT)
    .SetAuthMode(AuthMode.OTP)
    .SetAppearanceType(AppearanceType.STANDARD)
    .SetPageNo(PageNo.ALL)
    .Build();
```

#### `eSignSettings`
Configuration holder for eSign service settings.

**Properties**:
- `ASPID` - Application Service Provider ID (required)
- `Gateway_URL` - eSign gateway URL (required)
- `ResponseURL` - Callback URL for async responses (required)
- `Timeout` - Connection timeout in milliseconds (default: 30000)
- `ProxyHost`, `ProxyPort`, `ProxyUserID`, `ProxyUserPassword` - Proxy configuration

#### `UserInfoBuilder`
Builder for creating user information objects.

**Example**:
```csharp
var user = new UserInfoBuilder()
    .SeteMail("user@example.com")
    .SetFirstname("John")
    .SetLastname("Doe")
    .SetPhoneNumber("9876543210")
    .SetCity("Bangalore")
    .SetState("Karnataka")
    .SetCountry("India")
    .Build();
```

### Enumerations

#### `AuthMode`
Authentication methods:
- `OTP` - One-Time Password
- `FP` - Fingerprint
- `IRIS` - IRIS recognition
- `FACE` - Face recognition

#### `AppearanceType`
Signature appearance types:
- `STANDARD` - Standard signature
- `SIGNATUREIMG` - Image-based signature
- `ONELINER` - Single line signature
- `ADVANCED` - Advanced signature with custom styling
- `COLOREDGRAPHIC` - Colored graphics signature
- `BGIMG` - Background image signature

#### `Coordinates`
9-point positioning system:
- `TOPLEFT`, `TOPMIDDLE`, `TOPRIGHT`
- `CENTERLEFT`, `CENTERMIDDLE`, `CENTERRIGHT`
- `BOTTOMLEFT`, `BOTTOMMIDDLE`, `BOTTOMRIGHT`

#### `PageNo`
Page selection for signing:
- `ALL` - All pages
- `EVEN` - Even pages only
- `ODD` - Odd pages only
- `LAST` - Last page only
- `FIRST` - First page only
- `SPECIFY` - Specify custom pages

---

## Advanced Features

### Custom Signature Appearance

```csharp
var advSig = new AdvanceSignature
{
    SignerName = "John Doe",
    SignerLocation = "Bangalore",
    SignerReason = "Document Approval",
    SignerContactInfo = "+91-9876543210"
};

var style = new CustomStyle
{
    BackgroundColor = "#FFFFFF",
    TextColor = "#000000",
    FontSize = 12
};

var input = new eSignInputBuilder()
    // ... other settings
    .SetAppearanceType(AppearanceType.ADVANCED)
    .SetAdvanceSignature(advSig)
    .SetCustomStyle(style)
    .Build();
```

### Proxy Configuration

```csharp
var settings = new eSignSettings
{
    ASPID = "YOUR_ASP_ID",
    Gateway_URL = "https://gateway.example.com",
    ProxyHost = "proxy.company.com",
    ProxyPort = 8080,
    ProxyUserID = "proxyuser",
    ProxyUserPassword = "proxypass"
};
```

---

## License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

### Important Licensing Notes:

1. **AGPL-3.0 Requirements**:
   - Any modifications to this library must be open-sourced
   - Applications using this library over a network must provide source code to users
   - Commercial use is allowed, but derivative works must remain open source

2. **Embedded Libraries**:
   - **iText PDF Library**: This library embeds iText, which is also AGPL-licensed
   - **BouncyCastle**: Licensed under MIT-style license (permissive)

See [LICENSE.txt](LICENSE.txt) for full license text.

---

## Security Considerations

### Important Security Warnings

1. **Credential Handling**:
   - Never hardcode credentials in source code
   - Use `Web.config` or `App.config` for configuration
   - Use `ConfigurationManager.AppSettings` for sensitive values
   - Consider using encryption for connection strings

2. **Input Validation**:
   - Always validate and sanitize user inputs
   - Validate PDF content before processing
   - Sanitize email addresses and phone numbers

3. **Logging**:
   - Review log configurations to avoid logging sensitive data
   - Don't log passwords or API keys

### Recommended Security Practices

```csharp
// Use configuration files for credentials
var settings = new eSignSettings
{
    ASPID = ConfigurationManager.AppSettings["eSignASPID"],
    Gateway_URL = ConfigurationManager.AppSettings["eSignGatewayURL"]
};

// Or use encrypted configuration
var encryptedSection = (AppSettingsSection)ConfigurationManager
    .GetSection("secureAppSettings");
```

---

## Troubleshooting

### Common Issues

**Issue**: `FileNotFoundException` or DLL loading errors
**Solution**: Ensure all dependent DLLs are in the same directory or in the GAC

**Issue**: SSL/TLS errors
**Solution**: Ensure .NET Framework 4.8 is installed and TLS 1.2 is enabled

**Issue**: "Invalid ASPID" errors
**Solution**: Verify your ASPID is correctly registered

---

## Support and Contact

For issues, questions, or contributions:
- Open an issue on GitHub
- Contact: [your-contact@example.com]

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## Acknowledgments

- **iText PDF Library** - PDF manipulation capabilities
- **BouncyCastle** - Cryptographic operations

---

**Built with .NET Framework | Powered by Digital Signatures**
