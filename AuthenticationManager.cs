using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Core
{
    /// <summary>
    /// Password is always required (Option A). Keyfile is optional.
    /// The keyfile factor is based on file CONTENT only (not name/path).
    /// </summary>
    public static class AuthenticationManager
    {
        public static byte[] ReadKeyFileBytes(string keyFilePath)
        {
            if (string.IsNullOrWhiteSpace(keyFilePath))
                throw new ArgumentException("Keyfile path is required.", nameof(keyFilePath));

            return File.ReadAllBytes(keyFilePath);
        }

        public static byte[] DeriveMasterKey(string password, byte[]? keyFileBytes)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            try
            {
                byte[] material;

                if (keyFileBytes != null && keyFileBytes.Length > 0)
                {
                    byte[] keyFileHash = SHA256.HashData(keyFileBytes);

                    material = new byte[passwordBytes.Length + 1 + keyFileHash.Length];
                    Buffer.BlockCopy(passwordBytes, 0, material, 0, passwordBytes.Length);
                    material[passwordBytes.Length] = 0x00;
                    Buffer.BlockCopy(keyFileHash, 0, material, passwordBytes.Length + 1, keyFileHash.Length);

                    CryptographicOperations.ZeroMemory(keyFileHash);
                }
                else
                {
                    material = (byte[])passwordBytes.Clone();
                }

                try
                {
                    return SHA256.HashData(material);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(material);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passwordBytes);
            }
        }
    }
}
