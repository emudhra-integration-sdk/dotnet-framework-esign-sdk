# Contributing to eSign Library Kit (.NET)

Thank you for your interest in contributing to the eSign Library Kit! This document provides guidelines and instructions for contributing to this project.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Security](#security)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of experience level, background, or identity.

### Expected Behavior

- Be respectful and considerate
- Provide constructive feedback
- Focus on what is best for the community
- Show empathy towards other contributors

### Unacceptable Behavior

- Harassment or discriminatory language
- Trolling or insulting comments
- Personal or political attacks
- Publishing others' private information

---

## Getting Started

### Prerequisites

- .NET SDK 6.0 or higher
- Git
- IDE (Visual Studio, VS Code, or Rider)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/NetStandard_eSignLibKit.git
   cd NetStandard_eSignLibKit
   ```

3. Add upstream remote:
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/NetStandard_eSignLibKit.git
   ```

---

## Development Setup

### Building the Project

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run tests (if available)
dotnet test
```

### IDE Setup (Visual Studio)

1. Open Visual Studio
2. File → Open → Project/Solution
3. Select `eSignASPLibrary.sln`
4. Build → Build Solution
5. Ensure all dependencies are restored

### IDE Setup (Visual Studio Code)

1. Open VS Code
2. Open the project folder
3. Install C# extension (if not installed)
4. Press `Ctrl+Shift+B` to build
5. Use integrated terminal for .NET CLI commands

### IDE Setup (JetBrains Rider)

1. Open Rider
2. File → Open
3. Select the solution file
4. Build → Build Solution
5. All dependencies should restore automatically

---

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

1. **Bug Fixes**: Fix existing issues
2. **New Features**: Add new functionality
3. **Documentation**: Improve README, XML docs, or guides
4. **Tests**: Add unit tests or integration tests
5. **Performance**: Optimize existing code
6. **Security**: Fix security vulnerabilities
7. **Refactoring**: Improve code quality

### Finding Work

- Check the [Issues](../../issues) page for open issues
- Look for issues labeled `good first issue` for beginner-friendly tasks
- Look for issues labeled `help wanted` for areas needing assistance

### Before You Start

1. Check if an issue already exists for your proposed change
2. If not, create a new issue to discuss your proposal
3. Wait for maintainer feedback before starting major work
4. Assign yourself to the issue you're working on

---

## Coding Standards

### C# Code Style

Follow standard C# conventions and .NET naming guidelines:

```csharp
// Namespace: PascalCase
namespace eSign.Library
{
    // Class names: PascalCase
    public class UserInfoBuilder
    {
        // Constants: PascalCase
        private const int DefaultTimeout = 30000;

        // Private fields: camelCase with underscore prefix
        private string _userName;

        // Properties: PascalCase
        public string UserName { get; set; }

        // Methods: PascalCase
        public string GetUserName()
        {
            return _userName;
        }

        // Proper indentation (4 spaces)
        public void ProcessDocument(string input)
        {
            if (input != null)
            {
                // Process input
            }
        }
    }
}
```

### Documentation

#### XML Documentation Comments

All public classes and methods must have XML documentation:

```csharp
/// <summary>
/// Builds eSignInput objects using the builder pattern.
/// </summary>
/// <example>
/// <code>
/// var input = new eSignInputBuilder()
///     .SetUserInfo(userInfo)
///     .SetPDFBase64(pdfContent)
///     .Build();
/// </code>
/// </example>
/// <remarks>
/// This class follows the builder pattern for convenient object creation.
/// </remarks>
public class eSignInputBuilder
{
    /// <summary>
    /// Sets the user information for the signing request.
    /// </summary>
    /// <param name="userInfo">The user information object.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when userInfo is null.</exception>
    public eSignInputBuilder SetUserInfo(UserInfo userInfo)
    {
        if (userInfo == null)
        {
            throw new ArgumentNullException(nameof(userInfo));
        }
        _userInfo = userInfo;
        return this;
    }
}
```

#### Inline Comments

Use inline comments for complex logic:

```csharp
// Check if certificate is valid and not expired
if (certificate.Verify() && !IsRevoked(certificate))
{
    // Proceed with signature validation
    ValidateSignature(certificate);
}
```

### Code Organization

```csharp
public class ExampleClass
{
    // 1. Constants
    private const string DefaultValue = "default";

    // 2. Static fields
    private static int _instanceCount = 0;

    // 3. Private fields
    private string _name;
    private int _value;

    // 4. Constructors
    public ExampleClass() : this("default", 0)
    {
    }

    public ExampleClass(string name, int value)
    {
        _name = name;
        _value = value;
    }

    // 5. Properties
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    // 6. Public methods
    public string GetName()
    {
        return _name;
    }

    // 7. Private methods
    private void InternalMethod()
    {
        // Implementation
    }

    // 8. Nested classes (if needed)
    private class InnerHelper
    {
        // Implementation
    }
}
```

### Security Best Practices

1. **Never hardcode credentials**:
   ```csharp
   // BAD
   string password = "hardcoded123";

   // GOOD
   string password = Environment.GetEnvironmentVariable("PASSWORD");
   ```

2. **Validate all inputs**:
   ```csharp
   public void SetEmail(string email)
   {
       if (string.IsNullOrWhiteSpace(email) ||
           !Regex.IsMatch(email, @"^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$"))
       {
           throw new ArgumentException("Invalid email format", nameof(email));
       }
       _email = email;
   }
   ```

3. **Use cryptographically secure random**:
   ```csharp
   // BAD
   var random = new Random(); // Predictable

   // GOOD
   using var rng = RandomNumberGenerator.Create();
   ```

4. **Proper exception handling**:
   ```csharp
   try
   {
       ProcessDocument(input);
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Failed to process document");
       // Don't expose stack traces to users
       throw new ProcessingException("Document processing failed", ex);
   }
   ```

---

## Testing Guidelines

### Unit Tests

Write unit tests for all new functionality using xUnit, NUnit, or MSTest:

```csharp
using Xunit;

public class UserInfoBuilderTests
{
    [Fact]
    public void Build_WithValidData_ReturnsUserInfo()
    {
        // Arrange
        var builder = new UserInfoBuilder();

        // Act
        var user = builder
            .SeteMail("test@example.com")
            .SetFirstname("John")
            .SetLastname("Doe")
            .Build();

        // Assert
        Assert.Equal("test@example.com", user.eMail);
        Assert.Equal("John", user.Firstname);
        Assert.Equal("Doe", user.Lastname);
    }

    [Fact]
    public void SeteMail_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var builder = new UserInfoBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.SeteMail("invalid-email"));
    }
}
```

### Integration Tests

Test interaction between components:

```csharp
[Fact]
public async Task FullSigningWorkflow_Success()
{
    // Arrange
    var settings = CreateTestSettings();
    var userInfo = CreateTestUser();
    var pdfBase64 = LoadTestPDF();

    var input = new eSignInputBuilder()
        .SeteSignSettings(settings)
        .SetUserInfo(userInfo)
        .SetPDFBase64(pdfBase64)
        .Build();

    // Act
    var esign = new eSign();
    var result = esign.GenerateGatewayParameters(input);

    // Assert
    Assert.Equal("1", result.ReturnCode);
    Assert.NotNull(result.GatewayURL);
}
```

### Test Coverage

- Aim for at least 80% code coverage
- Focus on critical paths and edge cases
- Test both success and failure scenarios

---

## Pull Request Process

### Before Submitting

1. **Update your fork**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**:
   - Follow coding standards
   - Add tests
   - Update documentation

4. **Test your changes**:
   ```bash
   dotnet restore
   dotnet build
   dotnet test
   ```

5. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: add new signature appearance type

   - Implemented gradient signature appearance
   - Added configuration options for gradient colors
   - Updated documentation with examples

   Closes #123"
   ```

### Commit Message Format

Follow conventional commits:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Build process or auxiliary tool changes

**Examples**:
```
feat(signing): add support for custom fonts in signatures

fix(validation): correct email validation regex
Closes #456

docs(readme): update integration examples

test(builder): add tests for UserInfoBuilder edge cases
```

### Submitting the Pull Request

1. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. Create a pull request on GitHub

3. Fill out the PR template:
   ```markdown
   ## Description
   Brief description of changes

   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Documentation update
   - [ ] Refactoring

   ## Testing
   - [ ] Unit tests added/updated
   - [ ] Integration tests added/updated
   - [ ] Manual testing performed

   ## Checklist
   - [ ] Code follows project style guidelines
   - [ ] Self-review completed
   - [ ] XML comments added for public APIs
   - [ ] Documentation updated
   - [ ] No new warnings generated
   - [ ] Tests pass locally

   ## Related Issues
   Closes #123
   ```

4. Wait for code review

### Code Review Process

1. Maintainers will review your PR within 7 days
2. Address feedback by pushing new commits
3. Once approved, your PR will be merged
4. Your contribution will be acknowledged in the changelog

---

## Security

### Reporting Security Issues

**DO NOT** open public issues for security vulnerabilities.

Instead:
1. Email security concerns to: [security@example.com]
2. Include detailed information about the vulnerability
3. Wait for acknowledgment before public disclosure

See [SECURITY.md](SECURITY.md) for more details.

### Security Review Checklist

Before submitting code that handles:
- Cryptographic operations
- User credentials
- Network communications
- File I/O operations

Ensure:
- [ ] No hardcoded credentials
- [ ] Proper input validation
- [ ] Secure random number generation
- [ ] Strong cryptographic algorithms
- [ ] Proper exception handling
- [ ] No sensitive data in logs

---

## Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Project README (for significant contributions)

---

## Questions?

- Open a [discussion](../../discussions) for general questions
- Join our community chat: [link to chat]
- Email maintainers: [maintainer@example.com]

---

## License

By contributing to this project, you agree that your contributions will be licensed under the AGPL-3.0 License.

---

**Thank you for contributing to eSign Library Kit!**
