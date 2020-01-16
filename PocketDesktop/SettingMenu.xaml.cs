using System;
using System.Windows;

namespace PocketDesktop
{
    public partial class SettingMenu
    {
        private readonly MainDesktopWindow _parent;
        public SettingMenu(MainDesktopWindow parent)
        {
            InitializeComponent();
            _parent = parent;
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

        private void BtnReload_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.Reload();
            Hide();
        }
    }
}
