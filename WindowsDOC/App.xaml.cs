using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;

namespace WindowsDOC
{
    public partial class App : Application
    {
        // 将互斥体作为应用程序的字段
        Mutex? mutex;


        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查是否已存在一个互斥体实例
            mutex = new Mutex(true, "WindowsDOC", out bool createdNew);
            if (createdNew)
            {
                // 设置启动画面
                SplashScreen splashScreen = new("Image/WindowsDOC_Start.png");
                splashScreen.Show(true);
            }
            else
            {
                MessageBox.Show("已经打开一个啦~开那么多干啥= =");
                Environment.Exit(0);
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.Close();  // 释放互斥体

            base.OnExit(e);
        }


        // JsonSerializerOptions实例
        public static readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
