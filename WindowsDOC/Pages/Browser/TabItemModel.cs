using CefSharp;
using CefSharp.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsDOC.Pages.Browser
{
    // 定义标签页模型
    public class TabItemModel : INotifyPropertyChanged
    {
        // 定义事件
        public event Action<TabItemModel>? RequestCloseTab;



        private string _header = "新标签页"; // 初始标题
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header)); // 通知UI Header属性已更改
            }
        }

        public ChromiumWebBrowser Browser { get; set; }
        public Grid BrowserGrid { get; private set; } // 用于展示浏览器圆角布局
        public static Grid SetBrowserRoundedCorners(ChromiumWebBrowser chromiumWebBrowser) // 设置浏览器圆角
        {
            // 创建外层 Border
            var outerBorder = new Border
            {
                Background = Brushes.White,
                //CornerRadius = new CornerRadius(5)
            };

            // 创建用于 OpacityMask 的 VisualBrush
            var maskBrush = new VisualBrush();

            // 创建 VisualBrush 的 Visual 元素，即另一个 Border
            var maskVisual = new Border
            {
                Background = Brushes.Black,
                CornerRadius = new CornerRadius(5)
            };

            // 动态设置 maskVisual 的宽度和高度绑定
            maskVisual.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Border), 1)
            });
            maskVisual.SetBinding(FrameworkElement.HeightProperty, new Binding("ActualHeight")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Border), 1)
            });

            // 设置 VisualBrush 的 Visual
            maskBrush.Visual = maskVisual;

            // 将 VisualBrush 应用为外层 Border 的 OpacityMask
            outerBorder.OpacityMask = maskBrush;

            // 将 chromiumWebBrowser 添加到外层 Border
            outerBorder.Child = chromiumWebBrowser;

            // 创建 Grid 并添加外层 Border
            var grid = new Grid();
            grid.Children.Add(outerBorder);

            return grid;
        }


        public TabItemModel(string url)
        {
            Browser = new ChromiumWebBrowser(url);

            // 设置圆角浏览器
            BrowserGrid = SetBrowserRoundedCorners(Browser);

            // 设置自定义请求处理器
            var customRequestHandler = new CustomRequestHandler();
            Browser.RequestHandler = customRequestHandler;

            // 更新标签页标题为网页标题
            Browser.TitleChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Header = (string)args.NewValue;
                });
            };

            // 页面加载完成，为空白标签自动关闭
            Browser.LoadingStateChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!args.IsLoading)
                    {
                        Console.WriteLine(Browser);
                        if (Browser.Address == null)
                        {
                            // 认为页面是空的，执行关闭操作
                            RequestCloseTab?.Invoke(this);
                        }
                    }
                });
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
