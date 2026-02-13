using System.Windows;

namespace SecureVault
{
    public partial class HelpWindowEN : Window
    {
        public HelpWindowEN()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
