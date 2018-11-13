using System.Collections.Generic;
using System.Diagnostics;
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

namespace PocketDesktop
{
    public partial class MainDesktopWindow
    {
        private List<List<KeyValuePair<AppObj, Label>>> AppObjList;

        public MainDesktopWindow()
        {
            InitializeComponent();

            AppObjList = new List<List<KeyValuePair<AppObj, Label>>>();
            InitAppIcons();
            InitMagnify();
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
                AppObjList.Add(list);
            }
        }

        private static void MouseClickOnApp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border border)) return;

            if (!(VisualTreeHelper.GetChild(border, 0) is Grid grid)) return;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                var child = VisualTreeHelper.GetChild(grid, i);
                if (child is AppObj app)
                {
                    app.StartApp();
                }
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
                // TODO: Move apps down 1 block
            }
            else // Wheel down
            {
                // TODO: Move apps up 1 block
            }
        }
    }
}
