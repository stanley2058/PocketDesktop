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
            _parent.BackgroundBorder.Opacity = opacitySlider.Value;
            opacityLabel.Content = opacitySlider.Value.ToString("0.#0");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit PocketDesktop?", "Exit", MessageBoxButton.OKCancel,
                MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                Environment.Exit(0);
        }

        private void BtnReload_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.Reload();
            Hide();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (opacityLabel == null) return;
            opacityLabel.Content = opacitySlider.Value.ToString("0.#0");
            _parent.BackgroundBorder.Opacity = opacitySlider.Value;
            _parent.SaveSetting();
        }
    }
}
