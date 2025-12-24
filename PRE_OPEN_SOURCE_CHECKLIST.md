# Pre-Open Source Checklist

This checklist will help you prepare the eSign Library Kit (.NET) for public release. Complete all items before making the repository public.

---

## Critical Security Items

### Remove Sensitive Files

- [x] **Delete build output folders**:
  ```bash
  rm -rf bin/ obj/
  ```
  - Contains compiled binaries
  - May contain debug information

- [x] **Delete old/backup folders**:
  ```bash
  rm -rf *_Old/ *_New/
  ```
  - Contains outdated code
  - May contain experimental features

- [ ] **Remove license files** (if proprietary):
  - Check for any `*.lic` files referenced in code or configs
  - Remove development/QA license references

### Fix Security Vulnerabilities

- [ ] **Review SSL/TLS Certificate Validation**:

  Check all HTTP/HTTPS connection code for proper certificate validation.
  Search for certificate validation bypasses:
  ```bash
  grep -r "DangerousAcceptAnyServerCertificateValidator" .
  grep -r "ServerCertificateCustomValidationCallback" .
  ```

  Ensure proper implementation:
  ```csharp
  // Proper certificate validation
  var handler = new HttpClientHandler
  {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
      {
          return errors == SslPolicyErrors.None;
      }
  };
  ```

- [ ] **Review hardcoded paths**:
  Search for development paths:
  ```bash
  grep -r "D:\\" .
  grep -r "C:\\Users\\" .
  ```
  Remove or parameterize all hardcoded paths.

- [ ] **Review credentials**:
  Search for potential credentials:
  ```bash
  grep -ri "password" .
  grep -ri "apikey" .
  grep -ri "secret" .
  ```
  Ensure no credentials are hardcoded.

### Clean Code

- [ ] **Remove or anonymize developer information**:
  Search for personal information in code:
  ```bash
  grep -r "author" . --include="*.cs"
  ```
  Replace with generic names or team name:
  ```csharp
  // Before: /// <author>John Doe (john.doe@company.com)</author>
  // After: /// <author>eSign Development Team</author>
  ```

- [ ] **Remove TODO comments with internal references**:
  ```bash
  grep -r "TODO" . --include="*.cs"
  grep -r "FIXME" . --include="*.cs"
  grep -r "HACK" . --include="*.cs"
  ```
  Either complete TODOs or convert to GitHub issues.

- [ ] **Remove debug code**:
  Search for:
  - `Console.WriteLine` statements used for debugging
  - Debug logging at inappropriate levels
  - Commented-out code blocks
  ```bash
  grep -r "Console.WriteLine" . --include="*.cs"
  grep -r "//" . --include="*.cs" | grep -v "///"
  ```

---

## Documentation

### Required Documentation Files

- [x] **README.md** - Created
  - [ ] Review and customize:
    - Update repository URL
    - Add your contact information
    - Add support channels
    - Add badge shields (build status, license, etc.)

- [ ] **LICENSE.txt** - Create license file
  - [ ] Choose appropriate license (AGPL-3.0 recommended for compatibility)
  - [ ] Ensure compatibility with embedded libraries
  - [ ] Consider commercial licensing options if needed

- [x] **SECURITY.md** - Created
  - [ ] Add your security contact email
  - [ ] Add PGP key if available
  - [ ] Review vulnerability disclosure process

- [x] **CONTRIBUTING.md** - Created
  - [ ] Add community chat/forum links
  - [ ] Add maintainer contact information

- [ ] **CHANGELOG.md** - Create a changelog
  ```markdown
  # Changelog

  ## [2.0.0.17] - 2025-12-24

  ### Initial Open Source Release
  - Digital signature support for PDF documents
  - Multi-factor authentication (OTP, Fingerprint, IRIS, Face)
  - Multiple signature appearance types
  - Bank KYC integration
  - Cross-platform support with .NET Standard 2.0
  ```

- [ ] **CODE_OF_CONDUCT.md** - Add code of conduct
  You can use [Contributor Covenant](https://www.contributor-covenant.org/)

### Update Existing Documentation

- [ ] **Update .csproj file**:
  - Update project description
  - Verify version numbers
  - Add proper package metadata
  - Review dependencies

- [x] **Create .gitignore**:
  Already created with comprehensive .NET patterns

---

## Dependency Management

### Update Dependencies

- [ ] **Update NuGet packages**:
  Check current dependencies:
  ```bash
  dotnet list package --outdated
  dotnet list package --vulnerable
  ```

  Update packages:
  ```bash
  dotnet add package System.Security.Cryptography.Xml --version 7.0.1
  ```

- [ ] **Review embedded libraries**:
  - Document iText version (if embedded)
  - Document BouncyCastle version (if embedded)
  - Consider extracting as NuGet package references

### Create Dependency Documentation

- [ ] **Create DEPENDENCIES.md** or update README:
  ```markdown
  # Dependencies

  ## NuGet Packages

  | Package | Version | License | Purpose |
  |---------|---------|---------|---------|
  | System.Security.Cryptography.Xml | 7.0.1 | MIT | XML signature support |

  ## Embedded Libraries

  | Library | Version | License | Notes |
  |---------|---------|---------|-------|
  | iText | [version] | AGPL-3.0 | PDF manipulation |
  | BouncyCastle | [version] | MIT-style | Cryptography |
  ```

---

## Repository Setup

### GitHub Repository Configuration

- [ ] **Create repository on GitHub**:
  - Choose repository name: `NetStandard_eSignLibKit` or similar
  - Add description: ".NET SDK for digital signature operations with PDF signing and multi-factor authentication support"
  - Add topics: `csharp`, `dotnet`, `digital-signature`, `pdf`, `esign`, `cryptography`, `sdk`, `netstandard`

- [ ] **Configure repository settings**:
  - [ ] Enable Issues
  - [ ] Enable Discussions (recommended)
  - [ ] Enable Wiki (optional)
  - [ ] Disable unused features

- [ ] **Set up branch protection**:
  - Protect `main` branch
  - Require pull request reviews
  - Require status checks to pass

### GitHub Features

- [ ] **Enable Dependabot**:
  Create `.github/dependabot.yml`:
  ```yaml
  version: 2
  updates:
    - package-ecosystem: "nuget"
      directory: "/"
      schedule:
        interval: "weekly"
  ```

- [ ] **Add Issue Templates**:
  Create `.github/ISSUE_TEMPLATE/bug_report.md`:
  ```markdown
  ---
  name: Bug Report
  about: Report a bug to help us improve
  ---

  **Description**
  A clear description of the bug.

  **To Reproduce**
  Steps to reproduce the behavior.

  **Expected Behavior**
  What you expected to happen.

  **Environment**
  - .NET version:
  - OS:
  - Library version:
  ```

- [ ] **Add Pull Request Template**:
  Create `.github/PULL_REQUEST_TEMPLATE.md`

- [ ] **Add GitHub Actions** (optional):
  Create `.github/workflows/build.yml`:
  ```yaml
  name: Build and Test

  on: [push, pull_request]

  jobs:
    build:
      runs-on: ubuntu-latest
      steps:
        - uses: actions/checkout@v3
        - name: Setup .NET
          uses: actions/setup-dotnet@v3
          with:
            dotnet-version: '6.0.x'
        - name: Restore dependencies
          run: dotnet restore
        - name: Build
          run: dotnet build --configuration Release --no-restore
        - name: Test
          run: dotnet test --no-restore --verbosity normal
  ```

---

## Legal and Compliance

### License Review

- [ ] **Verify AGPL-3.0 compatibility**:
  - If embedding iText, must use AGPL-3.0
  - Your library must also be AGPL-3.0
  - Users must open-source their applications OR get commercial license

- [ ] **Consider dual licensing**:
  - AGPL-3.0 for open source use
  - Commercial license for proprietary applications
  - Document commercial licensing options in README

- [ ] **Review third-party licenses**:
  - Ensure all dependencies are compatible
  - Document all third-party licenses

### Attribution

- [ ] **Create NOTICE file**:
  ```
  eSign Library Kit (.NET)
  Copyright [Year] [Your Organization]

  This product includes software developed by:
  - iText Software Corp (iText PDF library) - AGPL-3.0
  - The Legion of the Bouncy Castle (BouncyCastle cryptography) - MIT-style
  - Microsoft (.NET Libraries) - MIT
  ```

- [ ] **Update copyright headers** (optional):
  Add to source files:
  ```csharp
  /*
   * Copyright (c) [Year] [Your Organization]
   *
   * This program is free software: you can redistribute it and/or modify
   * it under the terms of the GNU Affero General Public License as published
   * by the Free Software Foundation, either version 3 of the License, or
   * (at your option) any later version.
   */
  ```

---

## Testing

### Quality Assurance

- [ ] **Build and test**:
  ```bash
  dotnet clean
  dotnet restore
  dotnet build --configuration Release
  # Verify DLL is created successfully
  ls -lh bin/Release/netstandard2.0/eSignASPLibrary.dll
  ```

- [ ] **Test basic functionality**:
  - Create a simple test application
  - Test basic signing workflow
  - Verify no errors or warnings

- [ ] **Run security scans** (optional but recommended):
  ```bash
  # Check for vulnerable packages
  dotnet list package --vulnerable --include-transitive

  # Use security code scan
  dotnet add package SecurityCodeScan.VS2019
  dotnet build
  ```

---

## Final Steps

### Repository Upload

- [ ] **Initialize Git repository**:
  ```bash
  cd C:\Users\21729\Downloads\NetStandard_eSignLibKit
  git init
  git add .
  git commit -m "Initial commit: eSign Library Kit v2.0.0.17"
  ```

- [ ] **Add remote and push**:
  ```bash
  git remote add origin https://github.com/YOUR_USERNAME/NetStandard_eSignLibKit.git
  git branch -M main
  git push -u origin main
  ```

### Release

- [ ] **Create first release**:
  - Tag version: `v2.0.0.17`
  - Title: "eSign Library Kit v2.0.0.17 - Initial Open Source Release"
  - Upload DLL file as release asset
  - Upload NuGet package (if created)
  - Write release notes

- [ ] **Announce**:
  - Post on relevant forums/communities
  - Share on social media (if applicable)
  - Submit to NuGet.org (if appropriate)

---

## Post-Release

### Monitoring

- [ ] **Set up notifications**:
  - GitHub Issues
  - GitHub Discussions
  - Security advisories

- [ ] **Create maintenance plan**:
  - How often to review issues
  - How to handle security reports
  - Release schedule

### Community Building

- [ ] **Respond to early adopters**:
  - Answer questions promptly
  - Thank contributors
  - Fix reported bugs quickly

- [ ] **Create roadmap** (optional):
  - Document future features
  - Prioritize based on community feedback

---

## Quick Command Reference

### Cleanup Commands

```bash
# Remove sensitive files (already done)
rm -rf bin/ obj/ *_Old/ *_New/

# Search for sensitive data
grep -r "password" . --include="*.cs"
grep -r "D:\\" .
grep -r "Console.WriteLine" . --include="*.cs"

# Check file permissions
find . -type f -name "*.cs" | head -20
```

### Build Commands

```bash
# Clean and build
dotnet clean
dotnet restore
dotnet build --configuration Release

# Create NuGet package
dotnet pack --configuration Release

# Check package contents
unzip -l bin/Release/*.nupkg | head -20
```

### Git Commands

```bash
# Initial setup
git init
git add .
git commit -m "Initial commit"

# Add remote
git remote add origin <YOUR_REPO_URL>

# Push
git push -u origin main

# Tag release
git tag -a v2.0.0.17 -m "Initial open source release"
git push origin v2.0.0.17
```

---

## Checklist Summary

Print this checklist and check off items as you complete them:

**Critical** (Must complete):
- [x] Delete bin/, obj/ directories
- [x] Delete old folders (*_Old, *_New)
- [ ] Review SSL certificate validation
- [ ] Remove hardcoded credentials
- [ ] Update README with your information
- [ ] Create LICENSE.txt file
- [ ] Review license compatibility

**Important** (Should complete):
- [ ] Update NuGet dependencies
- [x] Add .gitignore
- [ ] Create CHANGELOG.md
- [ ] Create CODE_OF_CONDUCT.md
- [ ] Add copyright headers (optional)
- [ ] Test build process
- [ ] Create GitHub repository

**Nice to have** (Can complete later):
- [ ] Set up GitHub Actions
- [ ] Enable Dependabot
- [ ] Add comprehensive tests
- [ ] Create example applications
- [ ] Set up NuGet package publishing

---

**Estimated Time**: 4-8 hours for thorough preparation

**Questions?** Review SECURITY.md and CONTRIBUTING.md for additional guidance.

**Ready to go public?** Double-check all Critical items before making the repository public!
