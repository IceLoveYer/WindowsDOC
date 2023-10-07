using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace WindowsDOC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // 创建一个JsonSerializerOptions实例
        public static readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
