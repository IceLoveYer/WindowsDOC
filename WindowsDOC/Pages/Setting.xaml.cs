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

            // 读取数据
            ReadData();
        }

        // 颜色选择器改变，更新颜色
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            // 检查 e.NewValue 是否为空
            if (!e.NewValue.HasValue)
            {
                return;  // 如果为空，则直接返回
            }

            // 获取新选择的颜色
            Color NewColor = e.NewValue.Value;
            // 创建一个新的 SolidColorBrush 实例
            SolidColorBrush NewBrush = new(NewColor);

            // 获取当前的 Application 对象并访问其 Resources 字典
            ResourceDictionary appResources = Application.Current.Resources;

            if (sender is Xceed.Wpf.Toolkit.ColorPicker ColorPicker)
            {
                // 获取当前选择器名称
                string ResourceName = ColorPicker.Name;
                if (ResourceName.StartsWith("ColorPicker"))
                {
                    ResourceName = ResourceName["ColorPicker".Length..];

                    // 确认存在于字典中
                    if (appResources.Contains(ResourceName))
                    {
                        // 判断前缀是否为ColorBrush，否则为Color
                        ResourceName = ResourceName[(ResourceName.StartsWith("ColorBrush") ? "ColorBrush" : "Color").Length..];

                        appResources["Color" + ResourceName] = NewColor;
                        appResources["ColorBrush" + ResourceName] = NewBrush;
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
                if (wrappedResources != null && wrappedResources.TryGetValue("Colors", out Dictionary<string, string>? value))
                {
                    Dictionary<string, string> colorResources = value;

                    // 获取当前的 Application 对象并访问其 Resources 字典
                    ResourceDictionary appResources = Application.Current.Resources;

                    // 将颜色资源加载回资源字典
                    foreach (var item in colorResources)
                    {
                        Color NewColor = (Color)ColorConverter.ConvertFromString(item.Value);
                        appResources[item.Key] = NewColor;
                        SolidColorBrush NewBrush = new(NewColor);
                        appResources["ColorBrush" + item.Key["Color".Length..]] = NewBrush;
                    }
                }
            }
        }
    }
}
