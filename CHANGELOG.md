# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-12-24

### Initial Open Source Release

This is the first public release of the eSign Library Kit for .NET Framework.

#### Added
- **Core Functionality**:
  - PDF digital signature support with X.509 certificates
  - Multi-factor authentication (OTP, Fingerprint, IRIS, Face recognition)
  - Comprehensive signature appearance customization
  - Builder pattern for easy configuration

- **Signature Appearance Types**:
  - Standard signatures
  - Image-based signatures
  - One-liner signatures
  - Advanced signatures with custom styling
  - Colored graphics signatures
  - Background image signatures

- **Document Processing**:
  - Page-level signing control (All, Even, Odd, First, Last, custom pages)
  - Hash-based and full document signing
  - Document encryption/decryption capabilities
  - Content search functionality

- **Enterprise Features**:
  - Bank KYC integration
  - Proxy server support
  - Transaction management and verification
  - Full .NET Framework 4.8 support

- **Developer Features**:
  - Builder pattern for UserInfo and eSignInput
  - Comprehensive enumeration types
  - XML documentation for IntelliSense support
  - Error handling and validation

#### Platform Support
- .NET Framework 4.8
- Windows 7, 8, 10, 11
- Windows Server 2012+

#### Dependencies
- System.Security
- System.Drawing
- System.Web
- Embedded iText PDF library (AGPL-3.0)
- Embedded BouncyCastle cryptography library

---

## [Unreleased]

### Planned Features
- Additional authentication methods
- Enhanced documentation with more examples
- Unit test coverage
- Performance optimizations
- Additional PDF manipulation features

### Security Improvements Planned
- Enhanced input validation
- Improved credential handling
- SSL/TLS security hardening

---

## Release Notes

### Version 1.0.0 Notes

This release marks the transition to open source. Key highlights:

1. **License**: Released under AGPL-3.0 license
2. **Documentation**: Comprehensive README, CONTRIBUTING, and SECURITY documentation
3. **Compatibility**: Full .NET Framework 4.8 support for Windows applications
4. **Stability**: Production-ready codebase used in enterprise environments

### Known Issues

See [SECURITY.md](SECURITY.md) for important security considerations:
- Review SSL/TLS certificate validation implementation
- Use secure credential storage (Web.config encryption, Windows Credential Manager)
- Validate all user inputs before processing

---

## Support

For support with this release:
- GitHub Issues: Report bugs and feature requests
- GitHub Discussions: Ask questions and share ideas
- Security Issues: Email [security@example.com]

---

## Acknowledgments

Special thanks to:
- The iText PDF library team for PDF manipulation capabilities
- The BouncyCastle team for cryptographic operations
- All contributors who helped shape this library

---

## Versioning

This project uses [Semantic Versioning](https://semver.org/):
- **MAJOR**: Incompatible API changes
- **MINOR**: New functionality (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

---

**Last Updated**: 2025-12-24
