using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            foreach (var list in _appObjList)
            {
                foreach (var pair in list)
                {
                    pair.Key.Clear();
                    pair.Value.Content = "";
                    pair.Value.ToolTip = "";
                    pair.Value.Visibility = Visibility.Hidden;
                }
            }

            int i = 0, j = 0;
            foreach (var name in _fileTree.GetPageView())
            {
                _appObjList[i][j].Key.UpdateInfos(name);
                var lTitle = _appObjList[i][j].Key.GetName() ?? "";
                _appObjList[i][j].Value.Content = lTitle;
                _appObjList[i][j].Value.ToolTip = lTitle;
                _appObjList[i][j++].Value.Visibility = lTitle.Equals("") ? Visibility.Hidden : Visibility.Visible;
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
                            Opacity = 0.5
                        }
                    };

                    var app = new AppObj
                    {
                        Height = 100,
                        Width = 100,
                        Margin = new Thickness(0)
                    };

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
                    Panel.SetZIndex(label, 2);
                    Panel.SetZIndex(app, 1);
                    list.Add(new KeyValuePair<AppObj, Label>(app, label));
                }
                _appObjList.Add(list);
            }
        }

        private void MouseClickOnApp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border border)) return;

            if (!(VisualTreeHelper.GetChild(border, 0) is Grid grid)) return;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
                if (VisualTreeHelper.GetChild(grid, i) is AppObj app)
                    if (!app.StartApp())
                    {
                        _fileTree.OpenDir(app.GetPath());
                        ShowPage();
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

        private void AppPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_fileTree.IsSearchPage())
            {
                SearchInput.Text = "";
                _fileTree.UnSearch();
            }
            else
                _fileTree.GoBackDir();
            ShowPage();
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchInput.Text.Equals(""))
            {
                _fileTree.UnSearch();
            }
            else
                _fileTree.Search(SearchInput.Text);
            ShowPage();
        }
    }
}
