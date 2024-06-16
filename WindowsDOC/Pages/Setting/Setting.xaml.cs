using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsDOC.Control;
using static WindowsDOC.App;
using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages.Setting
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page, ISaveable
    {
        public Setting()
        {
            InitializeComponent();

            // 读取数据
            Task.Run(async () => { await ReadDataAsync(); });
        }



        // 颜色选择器改变，更新颜色
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (sender is Xceed.Wpf.Toolkit.ColorPicker colorPicker && e.NewValue.HasValue)
            {
                // 获取新选择的颜色和对应的SolidColorBrush
                Color newColor = e.NewValue.Value;
                SolidColorBrush newBrush = new(newColor);

                // 获取应用程序资源字典
                ResourceDictionary appResources = Application.Current.Resources;

                //更新应用程序资源字典中的颜色资源
                string resourceName = colorPicker.Name.Replace("ColorPicker", "Color");
                if (appResources.Contains(resourceName)) appResources[resourceName] = newColor;

                // 更新对应的SolidColorBrush资源
                string brushResourceName = colorPicker.Name.Replace("ColorPicker", "ColorBrush");
                if (appResources.Contains(brushResourceName)) appResources[brushResourceName] = newBrush;
            }
        }


        // 预设改变，更新颜色
        private void ComboBoxColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxColors.SelectedItem is ColorInfo selectedColor)
            {
                ColorPickerMain1.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorMain1);
                ColorPickerMain2.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorMain2);
                ColorPickerMain3.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorMain3);
                ColorPickerBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorBackground);
                ColorPickerBackgroundBorder.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorBackgroundBorder);
                ColorPickerSidebarButton.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorSidebarButton);
                ColorPickerTopBar.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorTopBar);
                ColorPickerMainFrame.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedColor.ColorMainFrame);

                if (ComboBoxColors.SelectedIndex > 0) NotificationControl.Add("呀，这难道是传说中的主题？", 2);

                // 重置 ComboBox 选中项
                ComboBoxColors.SelectedItem = null;
            }
        }



        // 读取目录
        public void ReadDir()
        {
            var dirInfos = new List<DirInfo>
            {
                new (FA5Icon.Solid_Home, "主目录", GlobalDir.Dir,null),
                new (FA5Icon.Solid_Rocket, "启动器", GlobalDir.LauncherDir,GlobalDir.LauncherJson),
                new (FA5Icon.Solid_Globe, "浏览器", GlobalDir.BrowserDir,GlobalDir.BrowserJson),
                new (FA5Icon.Solid_Cog, "设置", GlobalDir.SettingDir,GlobalDir.SettingJson),
            };

            // 更新UI元素
            Dispatcher.Invoke((() =>
            {
                ListViewItems.ItemsSource = dirInfos; // 绑定到ListView
            }));
        }
        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DirInfo dirInfo)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{dirInfo.Dir}\"")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    NotificationControl.Add("打开文件夹失败！");
                }
            }
        }
        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DirInfo dirInfo)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{dirInfo.Json}\"")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    NotificationControl.Add("打开配置文件失败！");
                }
            }
        }



        // 关于中标签的点击操作
        private void TextBlockQq_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText("1750310791");
            NotificationControl.Add("已复制QQ号至剪切板~");
        }
        private void TextBlockQqGroup_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText("200507900");
            NotificationControl.Add("已复制QQ群号至剪切板~");
        }
        private void TextBlockWeb_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 打开默认浏览器至指定的URL
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.iceyer.cn",
                UseShellExecute = true // 在.NET Core中，确保设置UseShellExecute为true
            });
            NotificationControl.Add("正在跳转作者官网哦~");
        }
        private void TextBlockWebHelp_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 打开默认浏览器至指定的URL
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/IceLoveYer/WindowsDOC",
                UseShellExecute = true // 在.NET Core中，确保设置UseShellExecute为true
            });
            NotificationControl.Add("正在跳转开源地址哦~");
        }



        // 保存
        public async Task SaveDataAsync()
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    ["Colors"] = GetColorResources(),
                    ["Position"] = GetWindowPosition()
                };

                // 保存至Json，且每项不能为空并格式化
                string jsonData = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 避免中文字符被转码
                });
                await File.WriteAllTextAsync(GlobalDir.SettingJson, jsonData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                NotificationControl.Add("SaveData捕获错误：" + ex.Message);
            }
        }
        private List<ColorInfo> GetColorResources() // 获取颜色资源
        {
            // 创建一个新的ColorInfo对象，它反映当前颜色选择器的选择
            List<ColorInfo> newColorInfos = (List<ColorInfo>)ComboBoxColors.ItemsSource;
            if (newColorInfos.Count > 0)
            {
                newColorInfos[0].ColorMain1 = ColorPickerMain1.SelectedColor.ToString() ?? "#F00ECDF0";
                newColorInfos[0].ColorMain2 = ColorPickerMain2.SelectedColor.ToString() ?? "#F0D453EE";
                newColorInfos[0].ColorMain3 = ColorPickerMain3.SelectedColor.ToString() ?? "#FFFFFFFF";
                newColorInfos[0].ColorBackground = ColorPickerBackground.SelectedColor.ToString() ?? "#FFFFFFFF";
                newColorInfos[0].ColorBackgroundBorder = ColorPickerBackgroundBorder.SelectedColor.ToString() ?? "#FFCACACA";
                newColorInfos[0].ColorSidebarButton = ColorPickerSidebarButton.SelectedColor.ToString() ?? "#Transparent";
                newColorInfos[0].ColorTopBar = ColorPickerTopBar.SelectedColor.ToString() ?? "#FFFFFFFF";
                newColorInfos[0].ColorMainFrame = ColorPickerMainFrame.SelectedColor.ToString() ?? "#FFFFFFFF";
            }

            return newColorInfos;
        }
        private static WindowPosition GetWindowPosition() // 获取窗口位置
        {
            var mainWindow = Application.Current.MainWindow;
            var position = new WindowPosition
            {
                LeftPercentage = mainWindow.WindowState == WindowState.Maximized ? 0 : mainWindow.Left / SystemParameters.WorkArea.Width,
                TopPercentage = mainWindow.WindowState == WindowState.Maximized ? 0 : mainWindow.Top / SystemParameters.WorkArea.Height,
                WidthPercentage = mainWindow.Width / SystemParameters.PrimaryScreenWidth,
                HeightPercentage = mainWindow.Height / SystemParameters.PrimaryScreenHeight
            };

            return position;
        }


        // 读取
        public async Task ReadDataAsync()
        {
            ReadDir();

            if (File.Exists(GlobalDir.SettingJson))
            {
                string jsonData = await File.ReadAllTextAsync(GlobalDir.SettingJson, Encoding.UTF8);
                var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

                if (settings != null)
                {
                    if (settings.ContainsKey("Colors"))
                    {
                        var colorInfo = JsonSerializer.Deserialize<List<ColorInfo>>(settings["Colors"].GetRawText());

                        Dispatcher.Invoke(() =>
                        {
                            ComboBoxColors.ItemsSource = colorInfo;
                            ComboBoxColors.SelectedIndex = 0;
                        });
                    }


                    if (settings.ContainsKey("Position"))
                    {
                        var position = JsonSerializer.Deserialize<WindowPosition>(settings["Position"].GetRawText());
                        if (position != null)
                        {
                            ApplyWindowPosition(position);
                        }
                    }
                }

            }
            else
            {
                // 若无配置文件的初始值
                var colorInfo = new List<ColorInfo>
                    {
                        new ("默认","#F00ECDF0","#F0D453EE","#FFFFFFFF","#FFFFFFFF","#FFCACACA","Transparent","#FFFFFFFF","#FFFFFFFF"),
                        new ("蓝紫","#F00ECDF0","#F0D453EE","#FFFFFFFF","#FFFFFFFF","#FFCACACA","Transparent","#FFFFFFFF","#FFFFFFFF"),
                        new ("骚粉","#FFFF00FF","#F0D453EE","#FFFFFFFF","#FFFFFFFF","#FFCACACA","Transparent","#FFFFFFFF","#FFFFFFFF"),
                        new ("氷叶","#FF00BFFF","#DE04A95C","#FFFFFFFF","#FFFFFFFF","#00FFFFFF","#00FFFFFF","#FFFFFFFF","#FFFFFFFF"),
                        new ("夏末","#FFF0F8FF","#FF00FFFF","#FF000000","#FF000000","#FFCACACA","Transparent","Transparent","Transparent"),
                    };

                Dispatcher.Invoke(() =>
                {
                    ComboBoxColors.ItemsSource = colorInfo;
                    ComboBoxColors.SelectedIndex = 0;
                });
            }
        }
        private void ApplyWindowPosition(WindowPosition position) // 应用窗口位置
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;

                // 如果上一次关闭时最大化状态
                if (position.LeftPercentage == 0 && position.TopPercentage == 0 && position.WidthPercentage == 1 && position.HeightPercentage >= 0.95)
                {
                    mainWindow.ResizeMode = ResizeMode.NoResize; // 关闭调整大小， 否则全屏无法全覆盖有缝隙
                    mainWindow.WindowState = WindowState.Maximized;
                }
                else
                {
                    mainWindow.Left = position.LeftPercentage * SystemParameters.WorkArea.Width;
                    mainWindow.Top = position.TopPercentage * SystemParameters.WorkArea.Height;
                    mainWindow.Width = position.WidthPercentage * SystemParameters.PrimaryScreenWidth;
                    mainWindow.Height = position.HeightPercentage * SystemParameters.PrimaryScreenHeight;
                }
            });
        }

    }
}
