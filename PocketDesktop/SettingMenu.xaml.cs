using System.Windows;

namespace PocketDesktop
{
    public partial class SettingMenu
    {
        public SettingMenu()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            Topmost = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
    }
}
