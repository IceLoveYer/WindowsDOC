using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDOC.Pages
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page
    {
        // 定义项目为全局
        private readonly string DataDirectory; // 当前工作目录

        public Setting()
        {
            // 设置数据目录
            DataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data\\Setting");
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
            //Console.WriteLine("当前数据目录：" + DataDirectory);

            InitializeComponent();

            // 添加颜色预设
            AddPresetColors();

            // 读取数据
            ReadData();
        }

        // 颜色选择器改变，更新颜色
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (sender is Xceed.Wpf.Toolkit.ColorPicker colorPicker)
            {
                // 检查 e.NewValue 是否为空
                if (!e.NewValue.HasValue) return;

                // 获取新选择的颜色
                Color newColor = e.NewValue.Value;
                SolidColorBrush newBrush = new(newColor);

                // 获取应用程序资源字典
                ResourceDictionary appResources = Application.Current.Resources;

                // 获取当前选择器名称
                string resourceName = colorPicker.Name;
                if (resourceName.StartsWith("ColorPicker"))
                {
                    resourceName = resourceName["ColorPicker".Length..];
                    // 确认资源存在
                    if (appResources.Contains(resourceName))
                    {
                        // 判断前缀是否为ColorBrush，否则为Color
                        resourceName = resourceName[(resourceName.StartsWith("ColorBrush") ? "ColorBrush" : "Color").Length..];

                        appResources["Color" + resourceName] = newColor;
                        appResources["ColorBrush" + resourceName] = newBrush;
                    }
                }
            }
        }



        // 保存
        public void SaveData()
        {
            // 获取当前的 Application 对象并访问其 Resources 字典
            ResourceDictionary appResources = Application.Current.Resources;

            // 创建一个嵌套的字典来存储颜色资源
            var ColorsResources = new Dictionary<string, Dictionary<string, string>>
            {
                ["Colors"] = new()
            };


            // 遍历资源字典并获取颜色资源
            foreach (var key in appResources.Keys)
            {
                // 只保留Color开头的颜色值
                if (!key.ToString()!.Contains("ColorBrush") && key.ToString()!.Contains("Color"))
                {
                    ColorsResources["Colors"][key.ToString()!] = appResources[key].ToString()!;
                }
            }

            // 保存至Json，且每项不能为空并格式化
            string JsonFilePath = Path.Combine(DataDirectory, "Data.json");
            string JsonData = JsonSerializer.Serialize(ColorsResources, App.jsonOptions);
            File.WriteAllText(JsonFilePath, JsonData, Encoding.UTF8);
        }

        // 读取
        private void ReadData()
        {
            string JsonFilePath = Path.Combine(DataDirectory, "Data.json");
            if (File.Exists(JsonFilePath))
            {
                string JsonData = File.ReadAllText(JsonFilePath, Encoding.UTF8);

                // 将JSON数据解析为嵌套的字典
                var wrappedResources = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(JsonData);

                // 确保解析成功并且包含"Colors"键
                if (wrappedResources != null && wrappedResources.TryGetValue("Colors", out Dictionary<string, string>? colorResources))
                {
                    // 创建一个映射，将颜色控件的名称映射到相应的ColorPickerControl实例
                    var colorPickers = new Dictionary<string, Xceed.Wpf.Toolkit.ColorPicker>
                    {
                        { "ColorMain1", ColorPickerColorMain1 },
                        { "ColorMain2", ColorPickerColorMain2 },
                        { "ColorMain3", ColorPickerColorMain3 },
                        { "ColorBackground", ColorPickerColorBackground },
                        { "ColorBackgroundBorder", ColorPickerColorBackgroundBorder },
                        { "ColorSidebarButton", ColorPickerColorSidebarButton },
                        { "ColorTopBar", ColorPickerColorTopBar },
                        { "ColorMainFrame", ColorPickerColorMainFrame }
                    };

                    // 遍历颜色资源并更新对应的ColorPickerControl
                    foreach (var colorResource in colorResources)
                    {
                        if (colorPickers.TryGetValue(colorResource.Key, out var colorPicker))
                        {
                            colorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(colorResource.Value);
                        }
                    }
                }
            }
        }



        // 定义预设颜色类型
        public class ColorPreset
        {
            public string Name { get; set; }
            public Color ColorMain1 { get; set; }
            public Color ColorMain2 { get; set; }
            public Color ColorMain3 { get; set; }
            public Color ColorBackground { get; set; }
            public Color ColorBackgroundBorder { get; set; }
            public Color ColorSidebarButton { get; set; }
            public Color ColorTopBar { get; set; }
            public Color ColorMainFrame { get; set; }

            public ColorPreset(string name, string colorMain1, string colorMain2, string colorMain3, string colorBackground, string colorBackgroundBorder, string colorSidebarButton, string colorTopBar, string colorMainFrame)
            {
                Name = name;
                ColorMain1 = (Color)ColorConverter.ConvertFromString(colorMain1);
                ColorMain2 = (Color)ColorConverter.ConvertFromString(colorMain2);
                ColorMain3 = (Color)ColorConverter.ConvertFromString(colorMain3);
                ColorBackground = (Color)ColorConverter.ConvertFromString(colorBackground);
                ColorBackgroundBorder = (Color)ColorConverter.ConvertFromString(colorBackgroundBorder);
                ColorSidebarButton = (Color)ColorConverter.ConvertFromString(colorSidebarButton);
                ColorTopBar = (Color)ColorConverter.ConvertFromString(colorTopBar);
                ColorMainFrame = (Color)ColorConverter.ConvertFromString(colorMainFrame);
            }
        }

        // 添加预设颜色
        private void AddPresetColors()
        {
            var presets = new List<ColorPreset>
            {
                new ("蓝紫","#F00ECDF0","#F0D453EE","#FFFFFFFF","#FFFFFFFF","#FFCACACA","Transparent","#FFFFFFFF","#FFFFFFFF"),
                new ("夏末","#FFF0F8FF","#FF00FFFF","#FF000000","#FF000000","#FFCACACA","Transparent","Transparent","Transparent"),
            };

            ComboBoxPresetColors.ItemsSource = presets;
        }

        // 更新预设颜色
        private void ComboBoxPresetColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxPresetColors.SelectedItem is ColorPreset selectedPreset)
            {
                ColorPickerColorMain1.SelectedColor = selectedPreset.ColorMain1;
                ColorPickerColorMain2.SelectedColor = selectedPreset.ColorMain2;
                ColorPickerColorMain3.SelectedColor = selectedPreset.ColorMain3;
                ColorPickerColorBackground.SelectedColor = selectedPreset.ColorBackground;
                ColorPickerColorBackgroundBorder.SelectedColor = selectedPreset.ColorBackgroundBorder;
                ColorPickerColorSidebarButton.SelectedColor = selectedPreset.ColorSidebarButton;
                ColorPickerColorTopBar.SelectedColor = selectedPreset.ColorTopBar;
                ColorPickerColorMainFrame.SelectedColor = selectedPreset.ColorMainFrame;

                // 重置 ComboBox 选中项
                ComboBoxPresetColors.SelectedItem = null;

                NotificationControl.Add("呀，这难道是传说中的主题？", 2);
            }
        }
    }
}
