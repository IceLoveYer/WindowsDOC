using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsDOC.Call
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            // 创建并运行 WPF 应用程序的实例
            App app = new ();
            app.InitializeComponent();
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // 当尝试启动一个新的应用程序实例时执行
            base.OnStartupNextInstance(eventArgs);

            // 如果主窗口已经存在，则激活它
            System.Windows.Application.Current.MainWindow?.Activate();
        }
    }
}
