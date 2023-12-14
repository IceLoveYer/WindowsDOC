using AutoUpdaterDotNET;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WindowsDOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 激活按钮、缓存页面实例
        Button ActivtionButton = new();
        readonly Pages.Launcher Launcher = new();
        readonly Pages.SystemConfig SystemConfig = new();
        readonly Pages.NetworkConfig NetworkConfig = new();
        readonly Pages.LinkConfig LinkConfig = new();
        readonly Pages.Setting Setting = new();

        // 贴边隐藏
        readonly HideWindowHelper hideHelper;

        public MainWindow()
        {
            InitializeComponent();


            // 设置窗口最大尺寸为屏幕工作区尺寸，最大化时显示任务栏
            this.MaxHeight = SystemParameters.WorkArea.Size.Height;
            this.MaxWidth = SystemParameters.WorkArea.Size.Width;


            // 设置当前激活按钮与页面
            SwitchPages(ButtonLauncher, Launcher);


            // 设置贴边隐藏
            hideHelper = HideWindowHelper.CreateFor(this)
                .AddHider<HideOnLeft>()
                .AddHider<HideOnRight>()
                .AddHider<HideOnTop>()
                .Start();

            // 通知队列初始化
            NotificationControl.SetNotificationPanel(StackPanelNotification);

            // 获取当前版本号
            TextBlockCurrentVersion.Text = "当前版本：" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }



        // 窗口控制钮
        private void Button_Click_Save(object sender, RoutedEventArgs e) // 保存
        {
            Launcher.SaveData();
            Setting.SaveData();

            NotificationControl.Add("保存成功！");
        }
        private void Button_Click_Refresh(object sender, RoutedEventArgs e) // 刷新
        {
            if (ActivationFrame.Content is Page currentPage)
            {
                Type currentPageType = currentPage.GetType();
                ActivationFrame.Navigate(Activator.CreateInstance(currentPageType));

                NotificationControl.Add("刷新成功！");
            }
        }
        private void Button_Click_Minimize(object sender, RoutedEventArgs e) // 最小化
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_Maximize(object sender, RoutedEventArgs e) // 放大还原
        {
            ToggleWindowState();
        }
        private void ToggleWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
            }
            else
            {
                ResizeMode = ResizeMode.NoResize; // 关闭调整大小， 否则全屏无法全覆盖有缝隙
                WindowState = WindowState.Maximized;
            }
        }

        private void Button_Click_Close(object sender, RoutedEventArgs e) // 关闭
        {
            // 保存
            Button_Click_Save(sender, e);

            // 强制退出，WPF不要用Close
            Application.Current.Shutdown();
        }


        // 设置窗口最大化、还原窗口的控制钮图标；调整BorderMain的Margin属性
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //// 窗口的宽和高
            //Console.WriteLine(this.Width);
            //Console.WriteLine(this.Height);
            //// 屏幕工作区的宽和高
            //Console.WriteLine(SystemParameters.WorkArea.Size.Width);
            //Console.WriteLine(SystemParameters.WorkArea.Size.Height);
            //Console.WriteLine("");


            // 设置放大、还原图标
            SvgAwesomeMaximize.Icon = WindowState == WindowState.Maximized ? FontAwesome5.EFontAwesomeIcon.Regular_WindowRestore : FontAwesome5.EFontAwesomeIcon.Regular_WindowMaximize;

            // 设置阴影边框
            if (this.Height == this.MaxHeight && this.Width == this.MaxWidth)
            {
                BorderMain.Margin = new Thickness(0);
            }
            else if (this.Height == this.MaxHeight)
            {
                this.Top = 0;
                if (this.Left == 0)
                {
                    BorderMain.Margin = new Thickness(0, 0, 10, 0);
                }
                else if (this.Left >= this.MaxWidth / 2)
                {
                    BorderMain.Margin = new Thickness(10, 0, 0, 0);
                }
                else
                {
                    BorderMain.Margin = new Thickness(10, 0, 10, 0);
                }
            }
            else if (this.Width == this.MaxWidth)
            {
                this.Left = 0;
                if (this.Top == 0)
                {
                    BorderMain.Margin = new Thickness(0, 0, 0, 10);
                }
                else if (this.Top >= this.MaxHeight / 2)
                {
                    BorderMain.Margin = new Thickness(0, 10, 0, 0);
                }
                else
                {
                    BorderMain.Margin = new Thickness(0, 10, 0, 10);
                }
            }
            else
            {
                BorderMain.Margin = new Thickness(10);
            }
        }

        // 设置窗口移动
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                WindowState TempWindowState = WindowState;

                this.DragMove();

                // 这里解决Windows10以上，鼠标拖动上方使软件全屏导致全屏无法全覆盖有缝隙
                if (TempWindowState != WindowState && WindowState == WindowState.Maximized)
                {
                    ToggleWindowState();
                    ToggleWindowState();
                }
            }
        }

        // 设置窗口双击全屏
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleWindowState();
            }
        }



        // 激活按钮切换页面
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SwitchPages(sender, Launcher);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SwitchPages(sender, SystemConfig);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            SwitchPages(sender, NetworkConfig);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            SwitchPages(sender, LinkConfig);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            NotificationControl.Add("这个暂时不知道写啥呢，别点了~");


        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            SwitchPages(sender, Setting);
        }

        // 切换页面
        private void SwitchPages(object sender, object page)
        {
            if (sender is Button TempButton && ActivtionButton != TempButton) // 激活按钮不能为当前点击的按钮
            {
                // 取消 按钮激活状态
                ActivtionButton.Style = this.FindResource("MenuButton") as Style;

                // 更新 当前激活按钮
                ActivtionButton = TempButton;

                // 设置 按钮激活状态
                ActivtionButton.Style = this.FindResource("ActivatedMenuButton") as Style;

                // 切换页面
                ActivationFrame.Navigate(page);
            }
        }



        // 托盘图标 左键单击
        private void MyNotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            hideHelper.TryShow();
        }

        // 托盘图标 显示窗口
        private void MyNotifyIconMenuItemShow_Click(object sender, RoutedEventArgs e)
        {
            hideHelper.TryShow();
            this.Activate();
        }

        // 托盘图标 退出程序
        private void MyNotifyIconMenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        // 侧边栏收缩
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GridLayout.ColumnDefinitions[0].Width = new GridLength(GridLayout.ColumnDefinitions[0].Width.Value == 150 ? 78 : 150);
            TextBlockLogo.FontSize = TextBlockLogo.FontSize == 30 ? 20 : 30;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 设置 AutoUpdater.NET
            AutoUpdater.Start("http://update.iceyer.cn/WindowsDOC/Update.xml");
        }

    }
}
