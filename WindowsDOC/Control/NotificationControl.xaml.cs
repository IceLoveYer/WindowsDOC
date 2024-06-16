using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WindowsDOC.Control
{
    /// <summary>
    /// NotificationControl.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationControl : UserControl
    {
        // 用于添加通知的Panel
        private static Panel? _notificationPanel;

        public NotificationControl()
        {
            InitializeComponent();
        }

        // 设置用于显示通知的容器
        public static void SetNotificationPanel(Panel panel)
        {
            _notificationPanel = panel;
        }

        // 将通知添加到队列
        public static void Add(string message, int? durationInSeconds = 3, FontAwesome5.EFontAwesomeIcon icon = FontAwesome5.EFontAwesomeIcon.Solid_ExclamationCircle)
        {
            if (_notificationPanel == null) return;

            // 创建新消息控件
            var notification = new NotificationControl();

            // 设置消息和图标
            notification.MessageText.Text = message;
            notification.SvgAwesomeIcon.Icon = icon;

            // 如果指定了持续时间，则设置计时器；否则，通知将保持显示状态直到手动关闭
            if (durationInSeconds.HasValue && durationInSeconds.Value > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationInSeconds.Value * 1000) };
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    notification.CloseNotification();
                };
                timer.Start();
            }

            // 添加到队列
            _notificationPanel.Children.Add(notification);
        }


        // 关闭通知并从Panel中移除
        private void CloseNotification()
        {
            if (_notificationPanel == null) return;

            _notificationPanel.Children.Remove(this);
        }

        // 关闭按钮的事件处理
        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            CloseNotification();
        }
    }
}
