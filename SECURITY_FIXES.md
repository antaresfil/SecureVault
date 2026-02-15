# 🔒 SecureVault v2.1.0 — Security Hardening & Format v3

**Date:** 15 Feb 2026  
**Release:** 2.1.0  
**Highlights:** Authenticated header (AAD), encrypted filename, safe paths, no overwrite, safe ZIP extraction, privacy-preserving keyfile behavior.

---

## ✅ What’s fixed / improved

### 1) Authenticated header (AAD) — **FIXED**
**Issue:** File headers/metadata were not authenticated, allowing tampering of fields stored outside AES-GCM.

**Fix:** Version **v3** introduces **AES-GCM AAD**. The header (`magic + version + salt + nonce`) is authenticated as *Associated Authenticated Data*.

**Impact:**
- Any modification of header fields causes decryption failure (tag mismatch).

---

### 2) Path Traversal / Arbitrary write on decrypt — **FIXED**
**Issue:** A crafted filename could cause writes outside the chosen output directory.

**Fix:**
- Filename is sanitized (`Path.GetFileName`, invalid characters removed).
- Output path is validated with `GetFullPath()` and must remain within the selected directory.

---

### 3) Accidental overwrite (encrypt & decrypt) — **FIXED**
**Issue:** `FileMode.Create` / `WriteAllBytes` could overwrite existing files.

**Fix:**
- Encrypt/decrypt now write using `FileMode.CreateNew`.
- If the target exists, the app auto-generates `name (1).ext`, `name (2).ext`, etc.

---

### 4) ZIP Slip on folder extraction — **FIXED**
**Issue:** Unsafe ZIP extraction could allow archive entries to escape the destination folder.

**Fix:**
- Replaced `ZipFile.ExtractToDirectory(...)` with a safe extractor:
  - Validates each entry path remains inside the output directory.
  - Creates directories safely.
  - Avoids overwriting by generating unique filenames.

---

### 5) Filename stored in cleartext — **FIXED (v3)**
**Issue:** Original filename was stored in plaintext in the `.svlt` header.

**Fix (v3 format):**
- The filename is now included **inside the encrypted payload**:
  - Plaintext package: `[u16 filenameLen][filenameUtf8][fileBytes]`
- Result: the `.svlt` file no longer leaks the original filename.

> Note: Legacy v1/v2 files still contain plaintext filename. Decryption sanitizes and protects output paths.

---

### 6) Keyfile privacy behavior — **CHANGED**
**Goal:** The app must **not reveal** whether a keyfile was used.

**Change:**
- v3 files no longer store a `keyfile used` flag.
- UI/metadata no longer displays “password only” vs “password + keyfile”.
- Decryption error messages are generic and privacy-preserving:
  - “Incorrect password / missing or wrong keyfile (if you used one) / file corrupted”.

**Result:**
- The user must remember whether they used a keyfile.
- The file itself does not expose that information via UI metadata.

---

## 📦 File format versions & compatibility

- **Encrypt:** produces **v3** only.
- **Decrypt:** supports **v1, v2, v3**.
  - v1/v2: no AAD, plaintext filename in header (legacy)
  - v3: AAD + encrypted filename (recommended)

---

## 🧹 Removed (for future re-introduction)
- Removed NuGet dependencies and documentation references for:
  - `MFA`
  - `MFA`

These will be reintroduced in a future MFA-focused release.

---

## 🧪 Recommended tests
- Decrypt v1/v2 samples and confirm output path is safe & non-overwriting.
- Encrypt new v3 files and verify:
  - header tampering breaks decrypt
  - filename is not visible in the file header
- Folder encrypt/decrypt with crafted ZIP entries (e.g., `../evil.txt`) must fail.

