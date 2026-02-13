using System.Windows;

namespace SecureVault
{
    public partial class SetupGuideWindow : Window
    {
        public SetupGuideWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
