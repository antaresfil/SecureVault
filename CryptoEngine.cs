using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace SecureVault.Core
{
    /// <summary>
    /// Secure cryptography engine using AES-256-GCM with Argon2id key derivation
    /// </summary>
    public class CryptoEngine
    {
        private const int SaltSize = 32; // 256 bits
        private const int NonceSize = 12; // 96 bits for GCM
        private const int TagSize = 16; // 128 bits authentication tag
        private const int KeySize = 32; // 256 bits
        
        // Argon2id parameters (OWASP recommendations)
        private const int Argon2Iterations = 3;
        private const int Argon2MemorySize = 65536; // 64 MB
        private const int Argon2Parallelism = 4;

        /// <summary>
        /// Encrypts a file with multi-factor derived key
        /// </summary>
        public static void EncryptFile(string inputPath, string outputPath, byte[] masterKey, bool usedKeyfile)
        {
            // Generate random salt and nonce
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            
            // Derive encryption key using Argon2id
            byte[] encryptionKey = DeriveKey(masterKey, salt);
            
            try
            {
                // Read input file
                byte[] plaintext = File.ReadAllBytes(inputPath);
                
                // Encrypt with AES-256-GCM
                byte[] ciphertext = new byte[plaintext.Length];
                byte[] tag = new byte[TagSize];
                
                using (var aes = new AesGcm(encryptionKey, TagSize))
                {
                    aes.Encrypt(nonce, plaintext, ciphertext, tag);
                }
                
                // Write encrypted file with header
                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    // Magic header for verification
                    bw.Write(Encoding.ASCII.GetBytes("SVLT")); // SecureVault signature
                    bw.Write((byte)2); // Version

                    // Flags
                    // bit0: password (always 1)
                    // bit1: keyfile used
                    byte flags = 0x01;
                    if (usedKeyfile) flags |= 0x02;
                    bw.Write(flags);
                    
                    // Crypto parameters
                    bw.Write(salt);
                    bw.Write(nonce);
                    bw.Write(tag);
                    
                    // Original filename (encrypted separately for metadata protection)
                    byte[] filenameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(inputPath));
                    bw.Write((ushort)filenameBytes.Length);
                    bw.Write(filenameBytes);
                    
                    // Encrypted data
                    bw.Write(ciphertext);
                }
                
                // Secure cleanup
                CryptographicOperations.ZeroMemory(plaintext);
                CryptographicOperations.ZeroMemory(ciphertext);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
            }
        }

        /// <summary>
        /// Reads metadata from an encrypted file without decrypting it
        /// </summary>
        public static FileMetadata ReadFileMetadata(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    // Verify magic header
                    byte[] magic = br.ReadBytes(4);
                    if (Encoding.ASCII.GetString(magic) != "SVLT")
                        return null; // Not a SecureVault file

                    byte version = br.ReadByte();
                    
                    byte flags = 0x01;
                    if (version >= 2)
                    {
                        flags = br.ReadByte();
                    }
                    else if (version != 1)
                    {
                        return null; // Unsupported version
                    }

                    bool usesPassword = (flags & 0x01) != 0;
                    bool usesKeyfile = (flags & 0x02) != 0;

                    return new FileMetadata
                    {
                        IsValid = true,
                        Version = version,
                        UsesPassword = usesPassword,
                        UsesKeyfile = usesKeyfile
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Metadata information about an encrypted file
        /// </summary>
        public class FileMetadata
        {
            public bool IsValid { get; set; }
            public byte Version { get; set; }
            public bool UsesPassword { get; set; }
            public bool UsesKeyfile { get; set; }

            public string GetAuthenticationInfo()
            {
                if (!IsValid)
                    return "Not a valid SecureVault file";

                if (UsesPassword && UsesKeyfile)
                    return "🔐 Password + Keyfile required";
                else if (UsesPassword)
                    return "🔑 Password only";
                else if (UsesKeyfile)
                    return "🔐 Keyfile only";
                else
                    return "⚠️ Unknown authentication";
            }
        }

        /// <summary>
        /// Decrypts a file with multi-factor derived key
        /// </summary>
        public static void DecryptFile(string inputPath, string outputPath, byte[] masterKey, byte[]? keyFileBytes)
        {
            using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // Verify magic header
                byte[] magic = br.ReadBytes(4);
                if (Encoding.ASCII.GetString(magic) != "SVLT")
                    throw new InvalidDataException("Invalid encrypted file format");
                
                // BUG FIX #1: Leggi la versione PRIMA di usarla
                byte version = br.ReadByte();
                
                // BUG FIX #2: Gestisci correttamente versione 1 e 2
                byte flags = 0x01; // Default per versione 1 (solo password)
                if (version >= 2)
                {
                    flags = br.ReadByte();
                }
                else if (version != 1)
                {
                    throw new InvalidDataException($"Unsupported file version: {version}");
                }
                
                // BUG FIX #3: Verifica keyfile PRIMA di leggere i dati
                bool requiresKeyfile = (flags & 0x02) != 0;
                if (requiresKeyfile && (keyFileBytes == null || keyFileBytes.Length == 0))
                    throw new UnauthorizedAccessException("This file was encrypted with a keyfile. Please provide the correct keyfile.");
                
                // Read crypto parameters
                byte[] salt = br.ReadBytes(SaltSize);
                byte[] nonce = br.ReadBytes(NonceSize);
                byte[] tag = br.ReadBytes(TagSize);
                
                // Read original filename
                ushort filenameLength = br.ReadUInt16();
                byte[] filenameBytes = br.ReadBytes(filenameLength);
                string originalFilename = Encoding.UTF8.GetString(filenameBytes);
                
                // Read encrypted data
                byte[] ciphertext = br.ReadBytes((int)(fs.Length - fs.Position));
                
                // Derive decryption key
                byte[] decryptionKey = DeriveKey(masterKey, salt);
                
                try
                {
                    // Decrypt with AES-256-GCM
                    byte[] plaintext = new byte[ciphertext.Length];
                    
                    using (var aes = new AesGcm(decryptionKey, TagSize))
                    {
                        aes.Decrypt(nonce, ciphertext, tag, plaintext);
                    }
                    
                    // Determine output path
                    string finalOutputPath = outputPath;
                    if (Directory.Exists(outputPath))
                    {
                        finalOutputPath = Path.Combine(outputPath, originalFilename);
                    }
                    
                    // Write decrypted file
                    File.WriteAllBytes(finalOutputPath, plaintext);
                    
                    // Secure cleanup
                    CryptographicOperations.ZeroMemory(plaintext);
                }
                catch (CryptographicException)
                {
                    // BUG FIX #5: Messaggio più chiaro per distinguere i problemi
                    if (requiresKeyfile && keyFileBytes != null && keyFileBytes.Length > 0)
                    {
                        throw new UnauthorizedAccessException(
                            "Decryption failed!\n\n" +
                            "Possible causes:\n" +
                            "• Incorrect password\n" +
                            "• Wrong keyfile selected\n" +
                            "• File corrupted or tampered\n\n" +
                            "Please verify your password and keyfile are correct.");
                    }
                    else
                    {
                        throw new UnauthorizedAccessException(
                            "Decryption failed!\n\n" +
                            "Possible causes:\n" +
                            "• Incorrect password\n" +
                            "• File corrupted or tampered\n\n" +
                            "Please verify your password is correct.");
                    }
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(decryptionKey);
                    CryptographicOperations.ZeroMemory(ciphertext);
                }
            }
        }

        /// <summary>
        /// Derives a 256-bit key using Argon2id
        /// </summary>
        private static byte[] DeriveKey(byte[] password, byte[] salt)
        {
            using (var argon2 = new Argon2id(password))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = Argon2Parallelism;
                argon2.Iterations = Argon2Iterations;
                argon2.MemorySize = Argon2MemorySize;
                
                return argon2.GetBytes(KeySize);
            }
        }

        /// <summary>
        /// Securely deletes original file by overwriting with random data
        /// </summary>
        public static void SecureDelete(string filePath, int passes = 3)
        {
            if (!File.Exists(filePath))
                return;

            var fileInfo = new FileInfo(filePath);
            long fileLength = fileInfo.Length;
            
            byte[] buffer = new byte[Math.Min(4096, fileLength)];
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                for (int pass = 0; pass < passes; pass++)
                {
                    fs.Position = 0;
                    
                    for (long written = 0; written < fileLength; written += buffer.Length)
                    {
                        int toWrite = (int)Math.Min(buffer.Length, fileLength - written);
                        RandomNumberGenerator.Fill(buffer.AsSpan(0, toWrite));
                        fs.Write(buffer, 0, toWrite);
                    }
                    
                    fs.Flush(true);
                }
            }
            
            File.Delete(filePath);
        }
    }
}
