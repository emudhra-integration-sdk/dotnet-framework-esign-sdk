# Security Policy

## Overview

This document outlines security considerations, known vulnerabilities, and best practices for using and contributing to the eSign Library Kit (.NET).

---

## Reporting Security Vulnerabilities

If you discover a security vulnerability, please:

1. **DO NOT** open a public issue
2. Email security concerns to: [security@example.com]
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if available)

We will acknowledge receipt within 48 hours and provide a detailed response within 7 days.

---

## Known Security Issues

### Critical Vulnerabilities

#### 1. Disabled SSL/TLS Certificate Validation

**Severity**: CRITICAL
**Files Affected**: Classes handling HTTPS connections

**Issue**:
The library may contain code that bypasses SSL/TLS certificate validation, making applications vulnerable to Man-in-the-Middle (MITM) attacks.

**Impact**:
- Applications are vulnerable to Man-in-the-Middle (MITM) attacks
- Attackers can intercept and modify sensitive data (digital signatures, certificates, PDFs)
- Complete compromise of secure communications

**Recommendation**:
**DO NOT USE IN PRODUCTION** without proper SSL/TLS certificate validation.

**Proper Implementation**:
```csharp
// Use proper certificate validation
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        // Implement proper validation logic
        // Do NOT simply return true
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        // Log and investigate certificate errors
        _logger.LogWarning($"Certificate error: {errors}");
        return false;
    }
};

using var client = new HttpClient(handler);
```

**Status**: Review required - Users must verify SSL implementation

---

#### 2. Hardcoded Sensitive Information in Logs

**Severity**: HIGH
**Files Affected**: Logging implementation

**Issue**:
Log files may contain:
- IP addresses
- Transaction IDs
- Development environment paths
- User email addresses
- Potentially sensitive data

**Impact**:
- Information disclosure
- Exposure of internal infrastructure
- Potential credential leakage

**Recommendation**:
1. **Before open-sourcing**: Delete all log files
2. **In production**:
   - Configure logging to exclude sensitive data
   - Use log sanitization
   - Implement proper log rotation and retention policies
   - Restrict log file access

```csharp
// Configure logger to exclude sensitive information
_logger.LogInformation("Transaction initiated"); // Good
_logger.LogInformation($"Transaction for user: {email}"); // Bad - contains PII

// Use structured logging with sanitization
_logger.LogInformation("Transaction {TransactionId} initiated for user {UserId}",
    transactionId, SanitizeUserId(userId));
```

**Status**: Users should review logging implementation

---

#### 3. Plain-text Credential Storage

**Severity**: HIGH
**Files Affected**: Configuration and settings classes

**Issue**:
Credentials may be stored and passed as plain strings:
- Proxy passwords
- PKCS12 certificate passwords
- API keys (ASPID)

**Impact**:
- Memory dumps could expose credentials
- Debugging tools could reveal passwords
- Process monitoring could capture sensitive data

**Recommendation**:
1. Use `SecureString` for passwords (though deprecated in .NET Core, still better than plain strings):
```csharp
// Use secure credential storage
string password = Configuration["Password"]; // From Azure Key Vault or similar

// Or use SecretManager in development
string password = Configuration.GetConnectionString("SecurePassword");
```

2. Use secure credential storage:
```csharp
// Use environment variables
string aspId = Environment.GetEnvironmentVariable("ESIGN_ASP_ID");

// Or use secure vaults (Azure Key Vault, AWS Secrets Manager)
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
KeyVaultSecret secret = await client.GetSecretAsync("esign-proxy-password");
string password = secret.Value;
```

3. Never log passwords or sensitive credentials

**Status**: Users should implement secure credential management

---

### Medium Severity Issues

#### 4. XML External Entity (XXE) Injection Risk

**Severity**: MEDIUM
**Files Affected**: Classes using XML parsing

**Issue**:
XML parsers may be vulnerable to XXE attacks if not properly configured.

**Recommendation**:
```csharp
var settings = new XmlReaderSettings
{
    // Prevent XXE
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null,
    MaxCharactersFromEntities = 1024
};

using var reader = XmlReader.Create(stream, settings);
```

**Status**: NEEDS VERIFICATION

---

#### 5. Insufficient Input Validation

**Severity**: MEDIUM
**Files Affected**: Multiple input handling classes

**Issue**:
Limited validation of user inputs:
- Email addresses
- Phone numbers
- Base64-encoded PDF content
- File paths

**Recommendation**:
```csharp
public class InputValidator
{
    private static readonly Regex EmailPattern =
        new Regex(@"^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$", RegexOptions.Compiled);

    private static readonly Regex PhonePattern =
        new Regex(@"^[0-9]{10}$", RegexOptions.Compiled);

    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
               EmailPattern.IsMatch(email);
    }

    public static bool IsValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
```

**Status**: PARTIAL - Some validation exists, needs enhancement

---

## Security Best Practices

### For Library Users

#### 1. Secure Configuration Management

```csharp
// NEVER hardcode credentials
// BAD:
settings.ASPID = "hardcoded-asp-id";
settings.ProxyUserPassword = "hardcoded-password";

// GOOD - Use environment variables:
settings.ASPID = Environment.GetEnvironmentVariable("ESIGN_ASP_ID");
settings.ProxyUserPassword = Environment.GetEnvironmentVariable("PROXY_PASSWORD");

// BETTER - Use configuration with secret management:
services.AddOptions<eSignSettings>()
    .Configure<IConfiguration>((settings, config) =>
    {
        config.GetSection("eSignSettings").Bind(settings);
    });
```

#### 2. Input Validation

```csharp
// Always validate inputs before passing to the library
public eSignInput BuildSecureInput(UserRequest request)
{
    // Validate email
    if (!IsValidEmail(request.Email))
    {
        throw new ArgumentException("Invalid email", nameof(request.Email));
    }

    // Validate phone
    if (!IsValidPhone(request.Phone))
    {
        throw new ArgumentException("Invalid phone", nameof(request.Phone));
    }

    // Validate PDF content
    if (!IsValidBase64PDF(request.PdfContent))
    {
        throw new ArgumentException("Invalid PDF content", nameof(request.PdfContent));
    }

    return new eSignInputBuilder()
        .SetUserInfo(BuildUserInfo(request))
        .SetPDFBase64(request.PdfContent)
        .Build();
}
```

#### 3. Secure Logging

```csharp
// Configure logging to avoid logging sensitive data
using Microsoft.Extensions.Logging;

public class SecureLogging
{
    private readonly ILogger<SecureLogging> _logger;

    public SecureLogging(ILogger<SecureLogging> logger)
    {
        _logger = logger;
    }

    public void LogTransaction(string transactionId, string userEmail)
    {
        // NEVER log full email or PII
        var maskedEmail = MaskEmail(userEmail);
        _logger.LogInformation(
            "Transaction {TransactionId} initiated for user {MaskedEmail}",
            transactionId, maskedEmail);
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            return "***";

        var parts = email.Split('@');
        return $"{parts[0][0]}***@{parts[1]}";
    }
}
```

#### 4. Network Security

```csharp
// Ensure proper SSL/TLS certificate validation
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // DON'T USE THIS
};

// Instead, use proper validation:
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        if (errors == SslPolicyErrors.None)
            return true;

        // Implement proper certificate validation
        _logger.LogWarning("SSL Certificate error: {Errors}", errors);
        return false;
    }
};
```

#### 5. Dependency Management

```xml
<!-- Regularly update NuGet packages to patch vulnerabilities -->
<ItemGroup>
  <PackageReference Include="System.Security.Cryptography.Xml" Version="7.0.1" />
  <!-- Check for latest secure versions -->
</ItemGroup>
```

```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update packages
dotnet add package System.Security.Cryptography.Xml
```

### For Contributors

#### 1. Code Review Checklist

Before submitting code, ensure:
- [ ] No hardcoded credentials or API keys
- [ ] No sensitive information in comments
- [ ] Proper input validation for all user inputs
- [ ] Secure random number generation (use `RandomNumberGenerator`, not `Random`)
- [ ] No usage of deprecated cryptographic algorithms (MD5, SHA1 for signatures)
- [ ] Proper exception handling (don't expose stack traces to users)
- [ ] SSL/TLS connections use proper certificate validation
- [ ] Logging doesn't include PII or credentials

#### 2. Cryptographic Operations

```csharp
// Use strong algorithms
// BAD:
using var md5 = MD5.Create(); // Broken
using var sha1 = SHA1.Create(); // Weak for signatures

// GOOD:
using var sha256 = SHA256.Create();
using var sha512 = SHA512.Create();

// Use cryptographically secure random
// BAD:
var random = new Random(); // Predictable

// GOOD:
using var rng = RandomNumberGenerator.Create();
var randomBytes = new byte[32];
rng.GetBytes(randomBytes);
```

#### 3. Secure Coding Standards

Follow OWASP Secure Coding Practices:
- https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/
- https://cheatsheetseries.owasp.org/

---

## Dependency Security

### Current Dependencies

| Dependency | Version | Known Vulnerabilities | Status |
|------------|---------|----------------------|--------|
| System.Security.Cryptography.Xml | 4.7.0 | Check NuGet for updates | UPDATE RECOMMENDED to 7.0+ |
| .NET Standard | 2.0 | None (platform version) | OK |

### Recommendations

1. **Update System.Security.Cryptography.Xml** to latest version
2. **Implement dependency scanning** in CI/CD pipeline
3. **Regular security audits** of NuGet packages

```bash
# Check for vulnerable packages
dotnet list package --vulnerable --include-transitive

# Update to latest secure versions
dotnet add package System.Security.Cryptography.Xml --version 7.0.1
```

---

## Compliance Considerations

### Data Privacy

This library may process:
- Personal Identifiable Information (PII): names, emails, phone numbers
- Biometric data: fingerprints, IRIS scans, face recognition
- Digital signatures and certificates

**Compliance Requirements**:
- **GDPR** (Europe): Ensure proper consent and data minimization
- **CCPA** (California): Provide data access and deletion capabilities
- **HIPAA** (Healthcare): Additional safeguards for medical documents
- **eIDAS** (EU Digital Signatures): Compliance for qualified electronic signatures

### Recommendations for Compliance

```csharp
// Implement data minimization
public class PrivacyAwareUserInfo
{
    // Only collect necessary data
    public string Email { get; set; } // Required
    public string FirstName { get; set; } // Required
    public string PhoneNumber { get; set; } // Required
    // Don't collect unnecessary PII
}

// Implement data retention
public async Task DeleteUserDataAsync(string userId)
{
    // Delete user information after retention period
    // Implement right to be forgotten (GDPR Article 17)
    await _repository.DeleteUserAsync(userId);
}

// Implement audit logging
public void AuditLog(string action, string userId, string resource)
{
    // Log who did what and when (GDPR Article 30)
    _auditLogger.LogInformation(
        "User {UserId} performed {Action} on {Resource}",
        userId, action, resource);
}
```

---

## Security Checklist Before Open-Sourcing

- [ ] Remove all log files
- [ ] Remove development configuration files
- [ ] Search for and remove hardcoded credentials
- [ ] Review SSL certificate validation implementation
- [ ] Update dependencies to latest secure versions
- [ ] Add security warnings to README
- [ ] Review code for sensitive comments or TODOs
- [ ] Set up vulnerability disclosure process
- [ ] Enable GitHub security features (Dependabot, CodeQL)

---

## Security Resources

### Tools for Security Testing

1. **OWASP Dependency-Check for .NET**:
   ```bash
   dotnet list package --vulnerable --include-transitive
   ```

2. **Security Code Scan**: Static analysis analyzer
   ```xml
   <ItemGroup>
     <PackageReference Include="SecurityCodeScan.VS2019" Version="5.6.7" PrivateAssets="all" />
   </ItemGroup>
   ```

3. **SonarQube**: Code quality and security analysis

### References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)

---

## Contact

For security concerns: [security@example.com]

**PGP Key**: [Your PGP key fingerprint]

---

**Last Updated**: 2025-12-24
**Version**: 1.0
