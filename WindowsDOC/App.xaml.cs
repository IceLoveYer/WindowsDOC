using CefSharp;
using CefSharp.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace WindowsDOC
{
    public partial class App : Application
    {
        Mutex? mutex; // 将互斥体作为应用程序的字段

        // 定义全局目录
        public static class GlobalDir
        {
            // 获取可执行文件的目录路径
            public static readonly string Dir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "C:\\Temp", "Data");

            public static readonly string LauncherDir = Path.Combine(Dir, "Launcher");
            public static readonly string LauncherJson = Path.Combine(LauncherDir, "Data.json");

            public static readonly string BrowserDir = Path.Combine(Dir, "Browser");
            public static readonly string BrowserJson = Path.Combine(BrowserDir, "Data.json");

            public static readonly string SettingDir = Path.Combine(Dir, "Setting");
            public static readonly string SettingJson = Path.Combine(SettingDir, "Data.json");

            // 静态构造函数
            static GlobalDir()
            {
                // 确保目录存在
                Directory.CreateDirectory(LauncherDir);
                Directory.CreateDirectory(BrowserDir);
                Directory.CreateDirectory(SettingDir);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查是否已存在一个互斥体实例
            mutex = new Mutex(true, "WindowsDOC", out bool createdNew);
            if (createdNew)
            {
                // 设置启动画面
                SplashScreen splashScreen = new("Image/WindowsDOC_Start.png");
                splashScreen.Show(true);

                // 初始化浏览器的内部设置
                if (!Cef.IsInitialized)
                {
                    var settings = new CefSettings()
                    {
                        //CachePath = Path.Combine(GlobalDir.BrowserDir, "Cache"), // 自定义缓存路径
                        LogFile = Path.Combine(GlobalDir.BrowserDir, "debug.log"), // 设置debug.log文件的自定义路径
                        AcceptLanguageList = "zh-CN,zh;q=0.9", // 设置接受语言为中文简体
                        Locale = "zh-CN", // 设置locale为中文简体，以便正确显示中文
                    };

                    // 为避免警告，确保设置了不同于默认值的缓存路径
                    //if (!Directory.Exists(settings.CachePath))
                    //{
                    //    Directory.CreateDirectory(settings.CachePath);
                    //}

                    Cef.Initialize(settings);
                }

                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("已经打开一个啦~开那么多干啥= =", "WindowsDOC");
                Environment.Exit(0);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.Close(); // 互斥体
            Cef.Shutdown(); //Cef

            base.OnExit(e);
        }


 
        // JsonSerializerOptions实例
        public static readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        // 页面保存接口
        public interface ISaveable
        {
            Task SaveDataAsync();
        }
    }
}
