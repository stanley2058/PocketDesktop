using System;
using System.Windows;

namespace PocketDesktop
{
    public partial class SettingMenu
    {
        public SettingMenu()
        {
            InitializeComponent();
            Hide();
            Topmost = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
