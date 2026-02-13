using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SecureVault.Core;

namespace SecureVault
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ValidateInputs();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Mostra menu per scegliere file o cartella
            var choice = MessageBox.Show(
                "What do you want to select?\n\n" +
                "Click YES for a FILE\n" +
                "Click NO for a FOLDER\n" +
                "Click CANCEL to abort",
                "Select File or Folder",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Cancel)
                return;

            if (choice == MessageBoxResult.Yes)
            {
                // ===== SELEZIONA FILE =====
                var ofd = new OpenFileDialog
                {
                    Title = "Select a file to encrypt or decrypt",
                    CheckFileExists = true,
                    Filter = "All files (*.*)|*.*|Encrypted files (*.svlt)|*.svlt"
                };

                if (ofd.ShowDialog() == true)
                {
                    FilePathTextBox.Text = ofd.FileName;
                    UpdateFileInfo();
                }
            }
            else
            {
                // ===== SELEZIONA CARTELLA =====
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.Description = "Select a folder to encrypt (will be saved as .zip.svlt)";
                    fbd.ShowNewFolderButton = false;

                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        FilePathTextBox.Text = fbd.SelectedPath;
                        UpdateFileInfo();
                    }
                }
            }
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFileInfo();
        }

        private void UpdateFileInfo()
        {
            if (FileInfoPanel == null || FileInfoText == null || FileInfoDetails == null)
                return;

            string filePath = FilePathTextBox.Text;

            // Nascondi il panel se non c'è file o se stiamo crittografando
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || EncryptRadioButton.IsChecked == true)
            {
                FileInfoPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Mostra info solo per file .svlt in modalità decrypt
            if (!filePath.EndsWith(".svlt", StringComparison.OrdinalIgnoreCase))
            {
                FileInfoPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Leggi i metadata del file
            var metadata = CryptoEngine.ReadFileMetadata(filePath);
            
            if (metadata != null && metadata.IsValid)
            {
                FileInfoPanel.Visibility = Visibility.Visible;
                FileInfoText.Text = metadata.GetAuthenticationInfo();
                
                // Dettagli aggiuntivi
                string details = $"File version: {metadata.Version} | ";
                if (metadata.UsesPassword && metadata.UsesKeyfile)
                    details += "You need BOTH password and the correct keyfile to decrypt this file.";
                else if (metadata.UsesPassword)
                    details += "You need the password to decrypt this file.";
                else if (metadata.UsesKeyfile)
                    details += "You need the keyfile to decrypt this file.";
                
                FileInfoDetails.Text = details;
            }
            else
            {
                FileInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowseKeyFileButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select keyfile",
                CheckFileExists = true
            };

            if (ofd.ShowDialog() == true)
            {
                KeyFilePathTextBox.Text = ofd.FileName;
                UseKeyFileCheckBox.IsChecked = true;
            }
        }

        private void SetupGuideButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new SetupGuideWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void HelpIT_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindowIT();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }

        private void HelpEN_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindowEN();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void AuthFactor_Changed(object sender, RoutedEventArgs e)
        {
            ValidateInputs();
        }

        private void AuthFactor_Changed(object sender, TextChangedEventArgs e)
        {
            ValidateInputs();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            try
            {
                string inputPath = FilePathTextBox.Text;

                // Determina se è un file o una cartella
                bool isFolder = Directory.Exists(inputPath);
                bool isFile = File.Exists(inputPath);

                if (!isFolder && !isFile)
                {
                    MessageBox.Show("Selected path does not exist.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string password = PasswordBox.Password;
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Password is required.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isEncrypting = EncryptRadioButton.IsChecked == true;

                // Se è una cartella e stiamo crittografando, analizza prima
                if (isFolder && isEncrypting)
                {
                    var analysis = FolderCryptoHelper.AnalyzeFolder(inputPath);
                    
                    if (!analysis.IsValid)
                    {
                        MessageBox.Show($"Cannot encrypt folder:\n\n{analysis.ErrorMessage}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Mostra warning se cartella grande
                    if (analysis.RequiresWarning)
                    {
                        var result = MessageBox.Show(
                            $"{analysis.WarningMessage}\n\n" +
                            $"Size: {analysis.GetSizeFormatted()}\n" +
                            $"Files: {analysis.FileCount}\n\n" +
                            "This may take several minutes. Continue?",
                            "Large Folder Warning",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result != MessageBoxResult.Yes)
                            return;
                    }
                }

                ProgressPanel.Visibility = Visibility.Visible;
                ExecuteButton.IsEnabled = false;
                ProgressText.Text = isEncrypting ? "Encrypting..." : "Decrypting...";

                try
                {
                    await Task.Run(() =>
                    {
                        byte[]? keyFileBytes = null;
                        bool useKeyfile = false;

                        Dispatcher.Invoke(() =>
                        {
                            useKeyfile = UseKeyFileCheckBox.IsChecked == true;

                            if (useKeyfile)
                            {
                                if (string.IsNullOrWhiteSpace(KeyFilePathTextBox.Text) || !File.Exists(KeyFilePathTextBox.Text))
                                    throw new UnauthorizedAccessException("Keyfile is required but was not selected or does not exist.");

                                keyFileBytes = AuthenticationManager.ReadKeyFileBytes(KeyFilePathTextBox.Text);
                            }
                        });

                        byte[] masterKey = AuthenticationManager.DeriveMasterKey(password, keyFileBytes);

                        bool secureDelete = false;
                        Dispatcher.Invoke(() =>
                        {
                            secureDelete = SecureDeleteCheckBox.IsChecked == true;
                        });

                        string? tempZipPath = null;

                        try
                        {
                            if (isEncrypting)
                            {
                                string fileToEncrypt = inputPath;

                                // Se è una cartella, crea ZIP temporaneo
                                if (isFolder)
                                {
                                    Dispatcher.Invoke(() => ProgressText.Text = "Creating ZIP archive...");
                                    tempZipPath = FolderCryptoHelper.CreateTemporaryZip(inputPath);
                                    fileToEncrypt = tempZipPath;
                                }

                                Dispatcher.Invoke(() => ProgressText.Text = "Encrypting...");
                                string outputPath = inputPath + (isFolder ? ".zip.svlt" : ".svlt");
                                CryptoEngine.EncryptFile(fileToEncrypt, outputPath, masterKey, useKeyfile);

                                // Secure delete dell'originale (file o cartella)
                                if (secureDelete)
                                {
                                    if (isFolder)
                                    {
                                        Dispatcher.Invoke(() => ProgressText.Text = "Securely deleting folder...");
                                        Directory.Delete(inputPath, true); // Cancella cartella
                                    }
                                    else
                                    {
                                        CryptoEngine.SecureDelete(inputPath);
                                    }
                                }

                                // Pulisci ZIP temporaneo
                                if (tempZipPath != null)
                                {
                                    FolderCryptoHelper.CleanupTempFile(tempZipPath);
                                }
                            }
                            else
                            {
                                // Decrittografia
                                string outputPath;
                                
                                if (inputPath.EndsWith(".zip.svlt", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Era una cartella
                                    outputPath = inputPath.Substring(0, inputPath.Length - 9) + "_decrypted.zip";
                                }
                                else if (inputPath.EndsWith(".svlt", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Era un file
                                    outputPath = inputPath.Substring(0, inputPath.Length - 5);
                                }
                                else
                                {
                                    outputPath = inputPath + ".decrypted";
                                }

                                CryptoEngine.DecryptFile(inputPath, outputPath, masterKey, keyFileBytes);
                            }
                        }
                        finally
                        {
                            CryptographicOperations.ZeroMemory(masterKey);
                            if (keyFileBytes != null) CryptographicOperations.ZeroMemory(keyFileBytes);
                            
                            // Cleanup finale ZIP temporaneo (se esiste ancora)
                            if (tempZipPath != null)
                            {
                                FolderCryptoHelper.CleanupTempFile(tempZipPath);
                            }
                        }
                    });

                    if (isEncrypting)
                        OutputPathTextBox.Text = inputPath + (isFolder ? ".zip.svlt" : ".svlt");
                    else
                    {
                        if (inputPath.EndsWith(".zip.svlt", StringComparison.OrdinalIgnoreCase))
                            OutputPathTextBox.Text = inputPath.Substring(0, inputPath.Length - 9) + "_decrypted.zip";
                        else if (inputPath.EndsWith(".svlt", StringComparison.OrdinalIgnoreCase))
                            OutputPathTextBox.Text = inputPath.Substring(0, inputPath.Length - 5);
                        else
                            OutputPathTextBox.Text = inputPath + ".decrypted";
                    }

                    MessageBox.Show(isEncrypting ? "Encryption successful!" : "Decryption successful!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Reset del form dopo successo
                    ResetForm();
                }
                catch (Exception ex)
                {
                    // BUG FIX #5: Assicurati che gli errori vengano sempre mostrati
                    MessageBox.Show($"Operation failed!\n\nError: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    throw; // Rilancia per il catch esterno
                }
            }
            catch (Exception ex)
            {
                // Catch di sicurezza - cattura TUTTO
                if (!ex.Message.Contains("Operation failed"))
                {
                    MessageBox.Show($"Unexpected error!\n\n{ex.GetType().Name}: {ex.Message}", 
                        "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                ProgressPanel.Visibility = Visibility.Collapsed;
                ExecuteButton.IsEnabled = true;
                ValidateInputs();
            }
        }

        private void ValidateInputs()
        {
            if (FilePathTextBox == null || PasswordBox == null || UseKeyFileCheckBox == null ||
                KeyFilePathTextBox == null || ExecuteButton == null)
                return;

            // Accetta sia file che cartelle
            string path = FilePathTextBox.Text;
            bool hasValidPath = !string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path));
            bool hasPassword = !string.IsNullOrEmpty(PasswordBox.Password);

            bool keyfileOk = true;
            if (UseKeyFileCheckBox.IsChecked == true)
            {
                keyfileOk = !string.IsNullOrEmpty(KeyFilePathTextBox.Text) && File.Exists(KeyFilePathTextBox.Text);
            }

            ExecuteButton.IsEnabled = hasValidPath && hasPassword && keyfileOk;
        }

        private void ResetForm()
        {
            // Pulisce tutti i campi dopo un'operazione completata
            FilePathTextBox.Text = string.Empty;
            OutputPathTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            KeyFilePathTextBox.Text = string.Empty;
            
            // Reset dei checkbox ai valori di default
            UseKeyFileCheckBox.IsChecked = false;
            SecureDeleteCheckBox.IsChecked = true;
            
            // Reset dell'operazione a Encrypt di default
            EncryptRadioButton.IsChecked = true;
            
            // Nascondi file info panel
            if (FileInfoPanel != null)
                FileInfoPanel.Visibility = Visibility.Collapsed;
            
            // Aggiorna lo stato dei pulsanti
            ValidateInputs();
        }
    }
}
