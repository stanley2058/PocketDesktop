using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppObj = PocketDesktop.ApplicationObject.ApplicationObject;
using Brushes = System.Windows.Media.Brushes;
using Tree = PocketDesktop.FileTree.FileTree;

namespace PocketDesktop
{
    public partial class MainDesktopWindow
    {
        private readonly List<List<KeyValuePair<AppObj, Label>>> _appObjList;
        private readonly Tree _fileTree;

        public MainDesktopWindow()
        {
            InitializeComponent();

            _appObjList = new List<List<KeyValuePair<AppObj, Label>>>();
            _fileTree = new Tree();

            InitMagnify();
            InitAppIcons();

            ShowPage();
        }

        private void ShowPage()
        {
            int i = 0, j = 0;
            foreach (var name in _fileTree.GetPageView())
            {
                Trace.WriteLine(name);
            }

            foreach (var name in _fileTree.GetPageView())
            {
                _appObjList[i][j++].Key.UpdateInfos(name);
                if (j != 3) continue;
                j = 0; ++i;
            }
        }

        private void InitAppIcons()
        {
            for (var i = 0; i < 3; ++i)
            {
                var list = new List<KeyValuePair<AppObj, Label>>();
                for (var j = 0; j < 3; ++j)
                {
                    var label = new Label
                    {
                        Style = Resources["RoundLabel"] as Style,
                        FontSize = 16,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(0),
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.DarkSlateGray,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Visibility = Visibility.Hidden,
                        Background = new SolidColorBrush
                        {
                            Color = Colors.SkyBlue,
                            Opacity = 0.75
                        }
                    };

                    var app = new AppObj
                    {
                        Height = 100,
                        Width = 100,
                        Margin = new Thickness(0)
                    };
                    app.SourceUpdated += AppSourceUpdate;

                    var border = new Border
                    {
                        Width = 100,
                        Height = 100,
                        CornerRadius = new CornerRadius(10),
                        Child = new Grid
                        {
                            Children =
                            {
                                label,
                                app
                            }
                        }
                    };
                    border.MouseLeftButtonDown += MouseClickOnApp;

                    AppPanel.Children.Add(border);
                    Grid.SetColumn(border, j);
                    Grid.SetRow(border, i);
                    list.Add(new KeyValuePair<AppObj, Label>(app, label));
                }
                _appObjList.Add(list);
            }
        }

        private static void MouseClickOnApp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border border)) return;

            if (!(VisualTreeHelper.GetChild(border, 0) is Grid grid)) return;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
                if (VisualTreeHelper.GetChild(grid, i) is AppObj app)
                    app.StartApp();
        }

        private static void AppSourceUpdate(object sender, DataTransferEventArgs e)
        {
            if (!(sender is AppObj appObj)) return;
            if (!(appObj.Parent is Grid grid)) return;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                if (!(VisualTreeHelper.GetChild(grid, i) is Label label)) continue;
                var name = appObj.GetName() ?? "";
                label.Content = name;
                label.Visibility = name.Equals("") ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private void InitMagnify()
        {
            var bitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject("magnify");
            if (bitmap == null) return;
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                SearchIcon.Source = bitmapImage;
            }
        }

        private void AppPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Trace.WriteLine(e.Delta);
            if (e.Delta > 0) // Wheel up
            {
                _fileTree.PrevLine();
                ShowPage();
            }
            else // Wheel down
            {
                _fileTree.NextLine();
                ShowPage();
            }
        }
    }
}
