using Shortcut;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppObj = PocketDesktop.ApplicationObject.ApplicationObject;
using Brushes = System.Windows.Media.Brushes;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using Panel = System.Windows.Controls.Panel;
using Tree = PocketDesktop.FileTree.FileTree;

namespace PocketDesktop
{
    public partial class MainDesktopWindow
    {
        private readonly List<List<KeyValuePair<AppObj, Label>>> _appObjList;
        private readonly Tree _fileTree;
        private readonly SettingMenu _settingMenu;
        private readonly HotkeyBinder _hotKeyBinder;
        private bool _initFlag;
        private Queue<string> _historyQueue;
        private int _peekIndex;

        private readonly Hotkey[] _digitHotkeys =
        {
            new Hotkey(Modifiers.Control, Keys.D1),
            new Hotkey(Modifiers.Control, Keys.D2),
            new Hotkey(Modifiers.Control, Keys.D3),
            new Hotkey(Modifiers.Control, Keys.D4),
            new Hotkey(Modifiers.Control, Keys.D5),
            new Hotkey(Modifiers.Control, Keys.D6),
            new Hotkey(Modifiers.Control, Keys.D7),
            new Hotkey(Modifiers.Control, Keys.D8),
            new Hotkey(Modifiers.Control, Keys.D9)
        };

        public MainDesktopWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            _initFlag = true;

            _appObjList = new List<List<KeyValuePair<AppObj, Label>>>();
            _fileTree = new Tree();
            _settingMenu = new SettingMenu(this);
            _hotKeyBinder = new HotkeyBinder();
            _historyQueue = new Queue<string>();

            InitImage(SearchIcon, "magnify");
            InitImage(SettingGearImg, "gear");
            InitAppIcons();

            ShowPage();
            SearchInput.Focus();
            BindUniversalHotKey();
            ReadSetting();
            ReadHistory();
        }

        public void Reload()
        {
            _fileTree.Reload();
            ShowPage();
        }

        public void SaveSetting()
        {
            var fPath = Environment.CurrentDirectory + "/setting.ini";
            if (File.Exists(fPath)) File.Delete(fPath);
            using (var sw = new StreamWriter(fPath))
            {
                var opVal = BackgroundBorder.Opacity.ToString("0.#0");
                sw.WriteLine($"opacity={opVal}");
            }
        }

        private void ReadSetting()
        {
            var fPath = Environment.CurrentDirectory + "/setting.ini";
            if (!File.Exists(fPath)) return;
            using (var sr = new StreamReader(fPath))
            {
                var str = sr.ReadLine();
                if (str != null) BackgroundBorder.Opacity = double.Parse(str.Split('=')[1]);
            }
        }

        private void UpdateHistiory(string appName)
        {
            var fPath = Environment.CurrentDirectory + "/history.ini";
            _historyQueue.Enqueue(appName);
            while (_historyQueue.Count > 20) _historyQueue.Dequeue();

            if (File.Exists(fPath)) File.Delete(fPath);

            using (var writer = new StreamWriter(fPath))
                foreach (var app in _historyQueue)
                    if (app.Trim() != "")
                        writer.WriteLine(app.Trim());
        }

        private void ReadHistory()
        {
            var fPath = Environment.CurrentDirectory + "/history.ini";
            if (!File.Exists(fPath)) return;
            using (var reader = new StreamReader(fPath))
            {
                var histories = reader.ReadToEnd().Replace("\r", "").Split('\n');
                var list = new List<string>(histories);
                list.RemoveAt(list.Count - 1);
                _historyQueue = new Queue<string>(list);
            }
        }

        private void BindUniversalHotKey()
        {
            _hotKeyBinder.Bind(Modifiers.Shift ^ Modifiers.Win, Keys.D).To(() =>
            {
                if (!IsVisible || _initFlag)
                    Show();
                else
                    Hide();
            });
        }

        private void BindHotKey()
        {
            _hotKeyBinder.Bind(Modifiers.None, Keys.Escape).To(EscapeKeyEvent);
            _hotKeyBinder.Bind(Modifiers.None, Keys.Up).To(() =>
            {
                if (_peekIndex > 0) --_peekIndex;
                SearchInput.Text = _historyQueue.ToArray()[_peekIndex];

            });
            _hotKeyBinder.Bind(Modifiers.None, Keys.Down).To(() =>
            {
                if (_peekIndex < _historyQueue.Count) ++_peekIndex;
                SearchInput.Text = _peekIndex == _historyQueue.Count ? "" : _historyQueue.ToArray()[_peekIndex];
            });

            for (var i = 0; i < _digitHotkeys.Length; ++i)
            {
                int row = i / 3, col = i % 3;
                _hotKeyBinder.Bind(_digitHotkeys[i]).To(() => { StartApp(row, col); });
            }
        }

        private void UnBindHotKey()
        {
            if (_hotKeyBinder.IsHotkeyAlreadyBound(new Hotkey(Modifiers.None, Keys.Escape)))
                _hotKeyBinder.Unbind(Modifiers.None, Keys.Escape);

            if (_hotKeyBinder.IsHotkeyAlreadyBound(new Hotkey(Modifiers.None, Keys.Up)))
                _hotKeyBinder.Unbind(Modifiers.None, Keys.Up);
            if (_hotKeyBinder.IsHotkeyAlreadyBound(new Hotkey(Modifiers.None, Keys.Down)))
                _hotKeyBinder.Unbind(Modifiers.None, Keys.Down);

            foreach (var t in _digitHotkeys)
                if (_hotKeyBinder.IsHotkeyAlreadyBound(t))
                    _hotKeyBinder.Unbind(t);
        }

        private new void Show()
        {
            _peekIndex = _historyQueue.Count;
            if (_initFlag)
            {
                Visibility = Visibility.Visible;
                _initFlag = false;
            }

            EscapeKeyEvent();
            BindHotKey();

            base.Show();
            PocketDesktop.CenterWindow(this);
            Activate();
            Focus();
        }

        private new void Hide()
        {
            UnBindHotKey();
            _settingMenu.Hide();
            base.Hide();
        }

        private void EscapeKeyEvent()
        {
            SearchInput.Text = "";
            SearchInput.Focus();
            _fileTree.GoHome();
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
                    border.MouseDown += MouseClickOnApp;

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
            {
                if (!(VisualTreeHelper.GetChild(grid, i) is AppObj app)) continue;

                if (!app.StartApp())
                {
                    if (e.MiddleButton == MouseButtonState.Pressed)
                    {
                        Process.Start(app.GetPath());
                        Hide();
                    }
                    else
                    {
                        _fileTree.OpenDir(app.GetPath());
                        ShowPage();
                    }
                }
                else
                {
                    UpdateHistiory(app.GetName());
                    Hide();
                }
            }
        }

        private static void InitImage(Image toMod, string prop)
        {
            var bitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject(prop);
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

                toMod.Source = bitmapImage;
            }
        }

        private void AppPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // Wheel up
                _fileTree.PrevLine();
            else // Wheel down
                _fileTree.NextLine();
            ShowPage();
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
                _fileTree.UnSearch();
            else
                _fileTree.Search(SearchInput.Text);
            ShowPage();
        }

        private void SearchInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Start first app
            if (e.Key == Key.Enter) StartApp(0, 0);
        }

        private void SettingGear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PocketDesktop.CenterWindow(_settingMenu);
            _settingMenu.Show();
        }

        private void StartApp(int row, int col)
        {
            try
            {
                if (_appObjList[row][col].Key.StartApp())
                {
                    Hide();
                    UpdateHistiory(_appObjList[row][col].Key.GetName());
                    return;
                }
                _fileTree.OpenDir(_appObjList[row][col].Key.GetPath());
                ShowPage();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
