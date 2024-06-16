using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using WindowsDOC.Call;
using WindowsDOC.Control;
using WindowsDOC.Pages.Browser;
using WindowsDOC.Pages.Launcher;
using WindowsDOC.Pages.LinkConfig;
using WindowsDOC.Pages.NetworkConfig;
using WindowsDOC.Pages.Setting;
using WindowsDOC.Pages.SystemConfig;
using static WindowsDOC.App;

namespace WindowsDOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 激活按钮
        Button ActivtionButton = new();
        // 缓存容器，用于存储页面实例
        private readonly Dictionary<Type, Page> pagesCache = new();

        // 贴边隐藏
        readonly HideWindowHelper hideHelper;

        public MainWindow()
        {
            InitializeComponent();

            // 先隐藏，UI加载后更新界面在设置显示
            this.Opacity = 0;

            // 设置窗口最大尺寸为屏幕工作区尺寸，最大化时显示任务栏
            this.MaxHeight = SystemParameters.WorkArea.Size.Height;
            this.MaxWidth = SystemParameters.WorkArea.Size.Width;

            // 设置贴边隐藏属性
            hideHelper = HideWindowHelper.CreateFor(this)
            .AddHider<HideOnLeft>()
            .AddHider<HideOnRight>()
            .AddHider<HideOnTop>();

            // 通知队列初始化
            NotificationControl.SetNotificationPanel(StackPanelNotification);

            // 获取当前版本号
            TextBlockCurrentVersion.Text = "当前版本：" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            // 设置当前激活按钮与页面
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await SwitchToPageAsync(ButtonSetting, typeof(Setting)); // 加载皮肤
                await SwitchToPageAsync(ButtonLauncher, typeof(Launcher)); // 加载启动器
                this.Opacity = 100; // 显示窗口
                hideHelper.Start(); // 启动贴边隐藏
            }));
        }
        


        // 窗口控制钮
        private void Button_Click_Save(object sender, RoutedEventArgs e) // 保存
        {
            // 页面是否实现了ISaveable接口，有些页面有保存的方法
            if (MainContentFrame.Content is ISaveable saveable)
            {
                saveable.SaveDataAsync();
            }

            NotificationControl.Add("保存完成！");
        }
        private void Button_Click_Refresh(object sender, RoutedEventArgs e) // 刷新
        {
            // 重新加载当前页面
            if (MainContentFrame.Content is Page currentPage)
            {
                Type currentPageType = currentPage.GetType();
                // 使用反射创建页面实例
                object? instance = Activator.CreateInstance(currentPageType);
                if (instance is Page createdPage)
                {
                    pagesCache[currentPageType] = createdPage;
                    MainContentFrame.Navigate(createdPage);
                }

                NotificationControl.Add("刷新完成！");
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
            // 保存所有页面
            foreach (var pageEntry in pagesCache)
            {
                if (pageEntry.Value is ISaveable saveablePage)
                {
                    saveablePage.SaveDataAsync();
                }
            }

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
            _ = SwitchToPageAsync(sender, typeof(Launcher));
        }

        private  void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _ = SwitchToPageAsync(sender, typeof(SystemConfig));
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _ = SwitchToPageAsync(sender, typeof(NetworkConfig));
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            _ = SwitchToPageAsync(sender, typeof(LinkConfig));
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _ = SwitchToPageAsync(sender, typeof(Browser));
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            _ = SwitchToPageAsync(sender, typeof(Setting));
        }

        // 切换页面，使用缓存
        private async Task<bool> SwitchToPageAsync(object senderButton, Type pageType)
        {
            bool isPageSwitched = false; // 默认为未切换页面

            // 检查触发切换的按钮是否不是当前激活的按钮
            if (senderButton is Button TempButton && ActivtionButton != TempButton)
            {
                // 更新按钮的样式，取消之前激活按钮的激活状态
                ActivtionButton.Style = this.FindResource("MenuButton") as Style;
                ActivtionButton = TempButton; // 将当前点击的按钮设为激活按钮
                ActivtionButton.Style = this.FindResource("ActivatedMenuButton") as Style; // 设置当前按钮为激活状态

                // 用于存储或获取的页面实例
                Page? page = null;

                // 在UI线程上执行页面的获取或创建
                await this.Dispatcher.BeginInvoke(() =>
                {
                    // 尝试从缓存中获取页面实例，如果不存在则创建一个新的实例
                    if (!pagesCache.TryGetValue(pageType, out page))
                    {
                        // 使用反射创建页面实例
                        object? instance = Activator.CreateInstance(pageType);
                        if (instance is Page createdPage)
                        {
                            // 如果成功创建页面实例，存储到缓存中并更新局部变量
                            page = createdPage;
                            pagesCache[pageType] = page;
                            isPageSwitched = true; // 成功创建并添加到缓存，标记为已切换页面
                        }
                    }
                    else
                    {
                        isPageSwitched = true; // 页面已存在于缓存，标记为已切换页面
                    }
                });

                // 如果页面实例非空，则进行页面导航
                if (page != null)
                {
                    MainContentFrame.Navigate(page);
                }
            }

            return isPageSwitched;
        }



        // 禁用Frame按Backspace键导航历史记录
        private void MainContentFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // 可以根据e.NavigationMode来判断导航类型（前进、后退、新导航等）决定是否取消导航
            if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true; // 取消导航
            }
        }



        // 托盘图标 左键单击
        private void MyNotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            hideHelper.TryShow();
            this.Activate();
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

        // 更新托盘菜单主题，否则右键菜单背景不更换
        private void MyNotifyIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.UpdateDefaultStyle();
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
