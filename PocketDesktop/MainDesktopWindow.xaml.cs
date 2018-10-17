using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PocketDesktop
{
    /// <summary>
    /// MainDesktopWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainDesktopWindow : Window
    {
        public MainDesktopWindow()
        {
            InitializeComponent();


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
    }
}
