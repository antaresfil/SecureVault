# 🔒 SecureVault - Multi-Factor File Encryption
> **v2.1.0 note:** New `.svlt` **v3** format authenticates the header (AES-GCM AAD) and encrypts the original filename. SecureVault no longer shows whether a keyfile was used; users must remember it.

A professional-grade file encryption application for Windows using AES-256-GCM with multi-factor authentication support.

## 🌟 Features

### Security
- **AES-256-GCM encryption** - NSA Suite B approved authenticated encryption
- **Argon2id key derivation** - Winner of the Password Hashing Competition
- **Authenticated encryption** - Prevents tampering and ensures data integrity
- **Secure file deletion** - 3-pass overwrite of original files
- **Zero-knowledge architecture** - Keys never stored on disk

### Authentication Factors
1. **Password** - Strong password-based authentication

### User Interface
- Modern WPF interface with real-time validation
- Comprehensive setup guide
- Progress indicators for long operations

## 📋 Requirements

### System Requirements
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime or SDK
- Administrator rights (for secure deletion feature)

### Optional Hardware

### Software Dependencies
All NuGet packages are automatically restored during build:
- `Konscious.Security.Cryptography.Argon2` (1.3.0)
- `OtpNet` (1.9.3)

## 🚀 Installation

### Building from Source

1. **Install .NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Clone or download this project**
   ```bash
   cd SecureVault
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Build the application**
   ```bash
   dotnet build --configuration Release
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   Or navigate to `bin/Release/net8.0-windows/` and run `SecureVault.exe`

### Alternative: Visual Studio

1. Open `SecureVault.sln` in Visual Studio 2022 or later
2. Right-click solution → Restore NuGet Packages
3. Build → Build Solution (F6)
4. Debug → Start Without Debugging (Ctrl+F5)

## 📖 Usage Guide

### Quick Start

1. **Launch SecureVault**
2. **Click "📖 Setup Guide"** for detailed instructions
3. **Select a file** to encrypt/decrypt
4. **Choose authentication factors**
5. **Click "🔒 Execute"**

### Encrypting a File

1. Click **"Browse..."** and select your file
2. Choose your authentication factors:
   - **Password**: Enter a strong password
3. Select **"Encrypt File"**
4. Optionally enable **"Securely delete original file"**
5. Click **"🔒 Execute"**
6. Output file will be created with `.svlt` extension

### Decrypting a File

1. Select your `.svlt` encrypted file
2. Enter the **SAME** authentication factors used during encryption:
   - Same password
3. Select **"Decrypt File"**
4. Click **"🔒 Execute"**
5. Original file will be restored

## 🔐 Security Details

### Encryption Scheme

```
Input File → Argon2id Key Derivation → AES-256-GCM Encryption → .svlt File
```

### File Format (.svlt)

```
[4 bytes]  Magic header: "SVLT"
[1 byte]   Version: 1
[32 bytes] Salt (for Argon2id)
[12 bytes] Nonce (for AES-GCM)
[16 bytes] Authentication tag (for AES-GCM)
[2 bytes]  Original filename length
[n bytes]  Original filename (UTF-8)
[n bytes]  Encrypted data
```

### Key Derivation Parameters

**Argon2id Configuration** (OWASP recommendations):
- Memory: 64 MB
- Iterations: 3
- Parallelism: 4 threads
- Output: 256 bits (32 bytes)

### Multi-Factor Combination

Factors are combined using HMAC-SHA256 cascade:
```
masterKey = HMAC-SHA256(
    HMAC-SHA256(
    )
)
```

## ⚙️ Authentication Setup

### Password Setup

**Recommendations:**
- Minimum 12 characters
- Mix of uppercase, lowercase, numbers, symbols
- Use a password manager
- Never reuse passwords

⚠️ **Warning**: If you lose your password, files CANNOT be recovered!


1. Click **"Generate New"** in SecureVault
2. Copy the displayed secret
3. Open your authenticator app (Google Authenticator, Authy, etc.)
4. Add a new account
5. Enter the secret manually
6. **Save the secret in a password manager**
7. Enter the 6-digit code when encrypting/decrypting


**Compatible Apps:**
- Google Authenticator
- Microsoft Authenticator
- Authy
- 1Password
- Bitwarden


**Requirements:**
- OATH credential configured

**Setup Steps:**
4. Configure OATH credential
6. Verify detection in status indicator


## 🛡️ Best Practices

### For Maximum Security
1. **Use at least 2 authentication factors** for sensitive files
3. **Keep backups** of authentication credentials
4. **Test decryption** immediately after encryption
5. **Use secure deletion** for original sensitive files
6. **Never share** authentication factors

### Backup Strategy
1. **Password**: Store in password manager
4. **Encrypted files**: Regular backups to external storage

### What NOT to do
- ❌ Don't encrypt files without testing decryption first
- ❌ Don't forget which factors you used for encryption
- ❌ Don't share authentication factors
- ❌ Don't rely on memory - save your secrets securely

## 🔧 Troubleshooting

- Try different USB port
- Check Windows Device Manager for driver issues

- Verify your device's time is synchronized
- Ensure authenticator app is updated
- Try the next code (30-second window)

### "Decryption Failed"
- Verify you're using the EXACT same authentication factors
- Check password for typos
- File may be corrupted - restore from backup

### "File Access Denied"
- Run as Administrator (for secure deletion)
- Check file permissions
- Close other programs using the file
- Verify disk has write permissions

## 📊 Technical Specifications

### Cryptography Standards
- **Encryption**: AES-256-GCM (NIST FIPS 197, NIST SP 800-38D)
- **Key Derivation**: Argon2id (RFC 9106)
- **Random Number Generation**: System.Security.Cryptography.RandomNumberGenerator

### Performance
- **Encryption speed**: ~50-100 MB/s (depends on hardware)
- **Memory usage**: ~100 MB + file size
- **Argon2id memory**: 64 MB during key derivation

### Supported File Types
- All file types (binary and text)
- No size limit (tested up to 10 GB)
- Folder encryption: zip folder first, then encrypt

## 🤝 Contributing

This is an open-source project. Contributions welcome!

### Development Setup
1. Fork the repository
2. Create feature branch
3. Make your changes
4. Add tests if applicable
5. Submit pull request

### Code Style
- Follow C# coding conventions
- Use async/await for I/O operations
- Add XML documentation comments
- Implement secure memory handling

## 📄 License

This project is licensed under the MIT License.

## ⚠️ Disclaimer

This software is provided "as is" without warranty. While it uses industry-standard cryptography, no software is 100% secure. Use at your own risk.

**Important Reminders:**
- Always backup important files before encryption
- Test decryption immediately after encryption
- Store authentication credentials securely
- No recovery is possible without proper credentials

## 🔗 Resources

- [NIST Cryptographic Standards](https://csrc.nist.gov/)
- [Argon2 Specification](https://github.com/P-H-C/phc-winner-argon2)

## 📞 Support

For issues, questions, or feature requests:
- Check the Setup Guide (📖 button in app)
- Review this README
- Check closed issues
- Open a new issue with details

---

**Version**: 1.0.0  
**Last Updated**: February 2026  
**Author**: SecureVault Development Team
