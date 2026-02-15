# 🔒 SecureVault

SecureVault is a Windows (WPF, .NET 8) application that encrypts and decrypts **files** and **folders** into a single `.svlt` container using **AES-256-GCM** and **Argon2id**.

**v2.0.0 highlights**

* New `.svlt` **v3** format: **header authenticated** (AES-GCM AAD) and **original filename encrypted**
* **No overwrite** by default (auto `name (1).ext` collision naming)
* **Safe folder decrypt/extract** (ZIP Slip & path traversal mitigations)
* **Keyfile privacy**: the app does **not** reveal whether a keyfile was used (users must remember it)

---

## 🌟 Features

### Security

* **AES-256-GCM** authenticated encryption (confidentiality + integrity)
* **Argon2id** KDF (RFC 9106) with per-file salt
* **Header authentication (v3)** via AES-GCM **AAD** to prevent metadata tampering
* **Filename privacy (v3)**: original filename is stored **inside the encrypted payload**
* **No-overwrite protection** for both encrypt and decrypt outputs
* **Safe folder extraction**: ZIP Slip mitigation during folder decrypt/extract
* **Secure deletion (best effort)**: overwrite attempt (see note below)

### Authentication modes

* **Password** (required)
* **Optional keyfile**

  * If a keyfile was used at encryption time, the **same keyfile** must be provided to decrypt.
  * The application intentionally does **not** indicate whether a keyfile is required.

### Inputs supported

* **Single file** → produces `*.svlt`
* **Folder** → folder is zipped, then encrypted to `*.svlt` (decrypt extracts safely)

---

## 📋 Requirements

* Windows 10/11 (64-bit)
* .NET 8.0 Runtime (or SDK for building)

### Dependencies (NuGet)

* `Konscious.Security.Cryptography.Argon2`

---

## 🧠 Security notes (important)

### Keyfile privacy

SecureVault avoids leaking whether a keyfile was used. Decryption errors are intentionally generic (password/keyfile incorrect *or* file corrupted).

### Secure deletion disclaimer

“Secure delete” is **best effort** only. On SSDs, journaling filesystems, snapshots, and wear-leveling can prevent guaranteed physical erasure. Use full-disk encryption + operational controls if you need strong deletion guarantees.

---

## 📦 File format `.svlt`

### v3 (recommended; used by default for encryption)

* Magic: `SVLT`
* Version: `3`
* Salt (Argon2id)
* Nonce (AES-GCM)
* Tag (AES-GCM)
* Ciphertext payload: `filenameLen + filenameUtf8 + fileBytes`
* **AAD** authenticates the canonical header (magic/version/salt/nonce)

### v1/v2 (legacy)

* Supported **only for decryption** (backward compatibility)
* Legacy formats may not authenticate all metadata; mitigations are applied at decrypt time (sanitization + path checks)

---

## ▶️ Usage (GUI)

1. Choose **Encrypt** or **Decrypt**
2. Select **File** or **Folder**
3. Enter **password** (and optionally select **keyfile**)
4. Choose output location (outputs never overwrite; collisions create `(...1)` automatically)

---

## 🛠️ Building

### Standard build (framework-dependent)

Produces an app that requires .NET 8 runtime on the target machine.

```bat
build.bat
```

### Portable build (self-contained)

Produces a self-contained portable build (no .NET runtime required on target).

```bat
build-portable.bat
```

### Release packaging helpers

Optional helper scripts (if present in this repo):

* `create-portable-release.bat`
* `create-source-package.bat`

---

## 🧭 Roadmap (technical)

* **Streaming / chunked format** (avoid whole-file-in-RAM for very large files)
* Further UX hardening around edge cases (permissions, long paths, locked files)

---

## 📝 License

See the repository license file (if present). If none is included yet, treat this project as “all rights reserved” until a license is added.
