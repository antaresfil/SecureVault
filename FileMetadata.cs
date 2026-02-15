using System;

namespace SecureVault
{
    /// <summary>
    /// Minimal, privacy-preserving metadata returned by ReadFileMetadata().
    /// Intentionally does NOT reveal whether a keyfile was used.
    /// </summary>
    public sealed class FileMetadata
    {
        public bool IsValid { get; set; }

        /// <summary>
        /// SecureVault container version (1/2 legacy, 3 current).
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Returns a short UI string (privacy-preserving).
        /// </summary>
        public string GetAuthenticationInfo()
        {
            if (!IsValid)
                return "Not a SecureVault file";

            // Do not disclose keyfile usage.
            return "SecureVault encrypted file";
        }
    }
}
