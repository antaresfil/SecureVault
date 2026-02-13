using System.Windows;

namespace SecureVault
{
    public partial class HelpWindowIT : Window
    {
        public HelpWindowIT()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
