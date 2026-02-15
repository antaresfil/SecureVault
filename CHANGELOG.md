# Changelog
All notable changes to this project will be documented in this file.

The format is based on **Keep a Changelog**, and this project follows **Semantic Versioning**.

## [2.1.3] - 2026-02-15

### Changed
- About window now shows the build version automatically from assembly metadata.
- Updated application metadata (description) to remove MFA references.

### Added
- Embedded application icon for executable (Win32 icon) and WPF application icon.

## [2.1.0] - 2026-02-15
### Added
- **File format v3**:
  - AES-256-GCM with **AAD** (authenticated header)
  - **Encrypted filename** (no longer stored in cleartext)
- Safe ZIP extraction for folder restore (Zip Slip protection).
- Automatic unique output naming to prevent overwrites (`name (1).ext`, ...).

### Changed
- **Keyfile privacy behavior**:
  - SecureVault no longer stores or displays whether a keyfile was used.
  - Decryption errors are privacy-preserving and do not disclose authentication mode.
- `ReadFileMetadata()` now exposes only validity + version (no keyfile/password mode).

### Fixed
- Path traversal / arbitrary write prevention during decrypt (sanitize + path containment check).
- Overwrite prevention for both encrypt and decrypt outputs.
- ZIP Slip prevention during archive extraction.

### Removed
- NuGet dependencies:
  - MFA
  - MFA
- Documentation references to MFA/MFA (to be reintroduced in a future MFA release).

### Compatibility
- **Decrypt** supports v1/v2/v3.
- **Encrypt** produces v3.

