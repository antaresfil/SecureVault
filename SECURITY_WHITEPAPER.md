# SecureVault Security Whitepaper

## Executive Summary

SecureVault is a professional-grade file encryption application designed for Windows that implements military-grade cryptography combined with multi-factor authentication. This document details the security architecture, cryptographic primitives, threat model, and implementation decisions.

## 1. Security Architecture

### 1.1 Design Principles

SecureVault follows these core security principles:

1. **Defense in Depth**: Multiple layers of security (encryption, authentication, secure deletion)
2. **Zero Knowledge**: No keys or secrets stored on disk
4. **Fail Secure**: Errors result in operation failure, not security degradation
5. **Minimal Trust**: Client-side only, no server dependencies

### 1.2 Component Architecture

```
┌─────────────────────────────────────────┐
│         User Interface Layer            │
│  (WPF - Input Validation & Display)     │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│    Authentication Manager Layer         │
│  • Password Processing                  │
│  • Multi-Factor Key Derivation         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Cryptography Engine Layer          │
│  • AES-256-GCM Encryption              │
│  • Argon2id Key Derivation             │
│  • Secure Random Generation            │
│  • Memory Protection                    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         File System Layer               │
│  • Secure File I/O                     │
│  • Secure Deletion                      │
│  • Atomic Operations                    │
└─────────────────────────────────────────┘
```

## 2. Cryptographic Primitives

### 2.1 Symmetric Encryption: AES-256-GCM

**Algorithm**: Advanced Encryption Standard (AES) in Galois/Counter Mode (GCM)

**Parameters**:
- Key Size: 256 bits
- Nonce Size: 96 bits (12 bytes)
- Tag Size: 128 bits (16 bytes)

**Rationale**:
- AES-256 is approved by NSA for TOP SECRET information (Suite B)
- GCM provides both confidentiality and authenticity
- Authenticated encryption prevents tampering attacks
- Well-studied and widely implemented in hardware (AES-NI)

**Security Properties**:
- IND-CPA (Indistinguishability under Chosen-Plaintext Attack)
- INT-CTXT (Integrity of Ciphertext)
- Authenticated Encryption with Associated Data (AEAD)

### 2.2 Key Derivation: Argon2id

**Algorithm**: Argon2id (hybrid mode of Argon2)

**Parameters** (OWASP recommended):
- Memory: 65,536 KB (64 MB)
- Iterations: 3
- Parallelism: 4
- Salt: 256 bits (32 bytes, randomly generated)
- Output: 256 bits (32 bytes)

**Rationale**:
- Winner of Password Hashing Competition (2015)
- Argon2id combines data-independent (Argon2i) and data-dependent (Argon2d) modes
- Resistant to GPU cracking attacks
- Resistant to side-channel attacks
- Memory-hard function increases cost of brute-force attacks

**Security Properties**:
- Time-memory trade-off resistance
- Side-channel attack resistance
- GPU/ASIC attack resistance


**Algorithm**: HMAC-SHA1 based Time-based OTP

**Parameters**:
- Secret: 160 bits (20 bytes)
- Time Step: 30 seconds
- Code Length: 6 digits
- Hash Function: SHA-1
- Window: ±1 step (allows 30s clock drift)

**Rationale**:
- Industry standard (Google Authenticator, Microsoft Authenticator)
- Simple user experience
- No network dependency
- Compatible with existing authenticator apps

**Security Properties**:
- One-time use codes
- Time-limited validity
- Offline operation



**Implementation**:
- OATH challenge-response
- Credential stored on hardware token

**Rationale**:
- Hardware-based secret storage
- Resistant to malware extraction
- FIDO Alliance certified device
- Phishing-resistant authentication

## 3. Key Derivation Flow

### 3.1 Multi-Factor Key Combination

SecureVault combines multiple authentication factors using a cryptographic cascade:

```
Step 1: Process Password
  if password provided:
    factor1 = UTF8(password)
  else:
    factor1 = empty

  else:
    factor2 = factor1

  else:
    factor3 = factor2

Step 4: Final Key Derivation
  masterKey = HMAC-SHA256(factor3)
```

### 3.2 From Master Key to Encryption Key

```
encryptionKey = Argon2id(
    password: masterKey,
    salt: random_256_bits,
    memoryCost: 64MB,
    iterations: 3,
    parallelism: 4,
    outputLength: 32 bytes
)
```

**Why This Design?**:
- Each factor contributes cryptographic entropy
- Order-independent combination (commutative)
- Failure of any factor doesn't reveal others
- HMAC provides cryptographic mixing

## 4. File Format Specification

### 4.1 .svlt File Structure

```
Offset  Size   Field               Description
------  -----  ------------------  ------------------------------------
0       4      Magic               "SVLT" (0x53 0x56 0x4C 0x54)
4       1      Version             Format version (0x01)
5       32     Salt                Argon2id salt (random)
37      12     Nonce               AES-GCM nonce (random)
49      16     Tag                 AES-GCM authentication tag
65      2      Filename Length     Length of original filename (n)
67      n      Filename            Original filename (UTF-8)
67+n    *      Ciphertext          Encrypted file data
```

### 4.2 Format Rationale

**Magic Header ("SVLT")**:
- Allows quick file type identification
- Prevents accidental processing of non-encrypted files

**Version Field**:
- Enables future format updates
- Allows backwards compatibility

**Salt (32 bytes)**:
- Ensures unique encryption key per file
- Prevents rainbow table attacks
- Cryptographically random

**Nonce (12 bytes)**:
- Required for AES-GCM
- Must be unique per encryption
- 96 bits provides 2^96 encryptions before collision

**Authentication Tag (16 bytes)**:
- Verifies ciphertext integrity
- Prevents tampering
- Computed by AES-GCM

**Original Filename**:
- Preserves file metadata
- Allows automatic file restoration
- UTF-8 encoding for international support

## 5. Security Features

### 5.1 Authenticated Encryption

AES-GCM provides both confidentiality and authenticity in a single operation:

**Properties**:
- Encryption prevents unauthorized reading
- Authentication prevents unauthorized modification
- Tag verification fails on any bit change

**Attack Prevention**:
- Ciphertext manipulation → decryption fails
- Bit flipping attacks → authentication tag mismatch
- Chosen-ciphertext attacks → detected before decryption

### 5.2 Secure Memory Handling

**Techniques**:
```csharp
// Zero memory after use
CryptographicOperations.ZeroMemory(sensitiveData);

// Use SecureString for passwords (where applicable)
SecureString securePassword = new SecureString();

// Immediate cleanup in finally blocks
try {
    // Use sensitive data
}
finally {
    CryptographicOperations.ZeroMemory(key);
}
```

**Rationale**:
- Prevents memory dumps from revealing secrets
- Reduces exposure window
- Protects against cold boot attacks
- Follows OWASP guidelines

### 5.3 Secure File Deletion

**Algorithm**: Multi-pass overwrite (default: 3 passes)

```
For each pass (1 to 3):
    Seek to start of file
    Overwrite entire file with cryptographically random data
    Flush to disk
Delete file
```

**Rationale**:
- Prevents file recovery from disk
- Meets DoD 5220.22-M standard (3 passes)
- Random data prevents pattern analysis
- Synchronous flush ensures data written

**Limitations**:
- Not effective on SSDs with wear leveling
- Not effective on COW filesystems
- Not effective on journaling filesystems
- Consider full disk encryption for SSD protection

### 5.4 Nonce Uniqueness

**Implementation**:
- Fresh cryptographically random nonce per encryption
- 96-bit nonce provides 2^96 unique values
- Birthday bound: safe for 2^48 encryptions

**Critical**: Never reuse (key, nonce) pair!

**Enforcement**:
- New nonce generated for every file
- Stored in file header
- Independent of user input

## 6. Threat Model

### 6.1 Threats In Scope

**1. Offline Attacks**:
- Attacker obtains encrypted file
- Attempts brute-force decryption
- **Mitigation**: Argon2id makes brute-force expensive

**2. Weak Password Attacks**:
- User chooses weak password
- Dictionary/rainbow table attacks
- **Mitigation**: Multi-factor authentication, Argon2id, random salt

**3. Malware on User System**:
- Keylogger captures password
- Memory scraping during operation
- **Mitigation**: Secure memory handling, minimal exposure time

**4. Tampering Attacks**:
- Modify encrypted file
- Bit-flipping attacks
- **Mitigation**: AES-GCM authentication tag

**5. Credential Theft**:
- **Mitigation**: Multi-factor requirement, hardware token

### 6.2 Threats Out of Scope

**1. Compromised Operating System**:
- Rootkit with kernel access
- **Note**: No software can protect against compromised OS

**2. Hardware Attacks**:
- Physical memory attacks (DMA)
- Hardware implants
- **Note**: Requires physical security controls

**3. Side-Channel Attacks**:
- Power analysis
- Timing attacks on local system
- **Note**: Relies on .NET cryptography implementation

**4. Coercion**:
- Rubber hose cryptanalysis
- Legal compulsion
- **Note**: No technical solution

## 7. Implementation Security

### 7.1 Random Number Generation

**Source**: System.Security.Cryptography.RandomNumberGenerator

**Properties**:
- Cryptographically secure PRNG
- Uses OS entropy sources (Windows CNG)
- Automatically reseeded
- Thread-safe

**Usage**:
```csharp
byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
```

### 7.2 Input Validation

**File Paths**:
- Validated for existence
- Checked for write permissions
- Directory traversal prevention

- Exactly 6 digits
- Validated against current time window
- ±1 step tolerance for clock drift

**Passwords**:
- No client-side length restrictions (user choice)
- Securely cleared from memory after use

### 7.3 Error Handling

**Principles**:
- Fail securely
- No sensitive information in error messages
- Constant-time comparison for authentication

**Examples**:
```csharp
// Good: Generic error
throw new UnauthorizedAccessException(
    "Decryption failed - incorrect key or corrupted file"
);

// Bad: Reveals information
throw new Exception("Password incorrect");
```

## 8. Known Limitations

### 8.1 SSD Secure Deletion

**Issue**: Secure deletion may not work on SSDs

**Reason**:
- Wear leveling
- TRIM commands
- Over-provisioning

**Recommendation**: Use full disk encryption (BitLocker/VeraCrypt)

### 8.2 Memory Protection

**Issue**: Complete memory protection is difficult

**Challenges**:
- Garbage collection in .NET
- Memory swapping to disk
- Hibernation files

**Mitigations**:
- CryptographicOperations.ZeroMemory()
- Minimal exposure time
- Immediate cleanup



**Implications**:

**Recommendation**:
- Use multiple authentication factors
- Test decryption before relying on encryption

## 9. Compliance and Standards

### 9.1 Cryptographic Standards

- **AES**: FIPS 197
- **AES-GCM**: NIST SP 800-38D
- **Argon2**: RFC 9106
- **HMAC**: FIPS 198-1

### 9.2 Industry Compliance

**NIST Guidelines**:
- Uses NIST-approved algorithms
- Follows NIST key management guidelines
- Implements NIST-recommended parameters

**OWASP**:
- Follows OWASP secure coding practices
- Uses OWASP-recommended Argon2id parameters
- Implements OWASP input validation guidelines

## 10. Security Audit Considerations

### 10.1 Code Review Focus Areas

1. **Cryptographic Implementation**:
   - Correct use of .NET cryptography APIs
   - Proper nonce/IV generation
   - Secure key derivation

2. **Memory Management**:
   - Sensitive data cleanup
   - Buffer overrun protection
   - Use-after-free prevention

3. **Input Validation**:
   - Path traversal prevention
   - Integer overflow checks
   - Format string safety

4. **Error Handling**:
   - Information leakage prevention
   - Resource cleanup in error paths
   - Constant-time comparisons

### 10.2 Testing Recommendations

**Cryptographic Testing**:
- Known answer tests for AES-GCM
- Argon2id output verification

**Security Testing**:
- Fuzzing file format parser
- Invalid authentication testing
- Corrupted file handling

**Integration Testing**:
- Multi-factor combinations
- Secure deletion verification

## 11. Future Enhancements

### 11.1 Potential Improvements

1. **Public Key Encryption**: Add recipient-based encryption
2. **Key Escrow**: Optional secure key backup
3. **File Sharing**: Secure multi-user file sharing
4. **Cloud Integration**: Direct encryption to cloud storage
5. **Mobile Support**: Cross-platform implementation

### 11.2 Research Areas

1. **Post-Quantum Cryptography**: Prepare for quantum computers
2. **Threshold Cryptography**: Split key among multiple parties
3. **Zero-Knowledge Proofs**: Prove decryption capability without revealing key

## 12. Conclusion

SecureVault implements a defense-in-depth approach to file encryption:

- **Strong Cryptography**: Military-grade AES-256-GCM
- **Key Hardening**: Argon2id password hashing
- **Data Integrity**: Authenticated encryption
- **Secure Deletion**: Multi-pass file overwrite

The system is designed with security best practices and follows established cryptographic standards. While no system is perfectly secure, SecureVault provides strong protection for sensitive files against realistic threat scenarios.

**Remember**: Security is only as strong as the weakest link. Users must:
- Choose strong passwords
- Keep backup credentials
- Test decryption after encryption

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Authors**: SecureVault Security Team
