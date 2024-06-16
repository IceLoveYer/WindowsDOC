using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WindowsDOC.Control
{
    /// <summary>
    /// IPAddressControl.xaml 的交互逻辑
    /// </summary>
    public partial class IPAddressControl : UserControl, INotifyPropertyChanged
    {
        // IP地址的四个部分
        private string _parts1 = "";
        private string _parts2 = "";
        private string _parts3 = "";
        private string _parts4 = "";

        public IPAddressControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        // 实现INotifyPropertyChanged接口，用于属性变更通知
        public event PropertyChangedEventHandler? PropertyChanged;

        // 触发属性变更事件的方法
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 定义一个依赖属性，用于存储IP地址的字符串表示形式
        public static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register(
                nameof(IPAddress),
                typeof(string),
                typeof(IPAddressControl),
                new PropertyMetadata(default(string), OnIPAddressChanged)
            );

        // 公共属性，用于获取或设置IP地址的字符串表示形式
        public string IPAddress
        {
            get => (string)GetValue(IPAddressProperty) == "..." ? "" : (string)GetValue(IPAddressProperty);
            set => SetValue(IPAddressProperty, value);
        }

        public void Clear()
        {
            IPAddress = "";
            Parts1 = Parts2 = Parts3 = Parts4 = "";
        }


        // 当IPAddress依赖属性的值发生变化时调用的静态回调方法
        private static void OnIPAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IPAddressControl control && e.NewValue is string newValue)
            {
                var parts = newValue.Split('.');
                if (parts.Length == 4)
                {
                    control.Parts1 = parts[0];
                    control.Parts2 = parts[1];
                    control.Parts3 = parts[2];
                    control.Parts4 = parts[3];
                }
            }
        }

        // 以下四个属性分别存储IP地址的四个部分
        public string Parts1
        {
            get => _parts1;
            set
            {
                if (_parts1 == value) return;
                _parts1 = value;
                OnPropertyChanged(nameof(Parts1));
                UpdateIPAddress();
            }
        }

        public string Parts2
        {
            get => _parts2;
            set
            {
                if (_parts2 == value) return;
                _parts2 = value;
                OnPropertyChanged(nameof(Parts2));
                UpdateIPAddress();
            }
        }

        public string Parts3
        {
            get => _parts3;
            set
            {
                if (_parts3 == value) return;
                _parts3 = value;
                OnPropertyChanged(nameof(Parts3));
                UpdateIPAddress();
            }
        }

        public string Parts4
        {
            get => _parts4;
            set
            {
                if (_parts4 == value) return;
                _parts4 = value;
                OnPropertyChanged(nameof(Parts4));
                UpdateIPAddress();
            }
        }

        // 更新IP地址的方法
        private void UpdateIPAddress()
        {
            IPAddress = $"{Parts1}.{Parts2}.{Parts3}.{Parts4}";
        }

        // 控制按键输入的方法
        private void TbxIP_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox)
            {
                switch (e.Key)
                {
                    case Key.D0:
                    case Key.D1:
                    case Key.D2:
                    case Key.D3:
                    case Key.D4:
                    case Key.D5:
                    case Key.D6:
                    case Key.D7:
                    case Key.D8:
                    case Key.D9:
                    case Key.NumPad0:
                    case Key.NumPad1:
                    case Key.NumPad2:
                    case Key.NumPad3:
                    case Key.NumPad4:
                    case Key.NumPad5:
                    case Key.NumPad6:
                    case Key.NumPad7:
                    case Key.NumPad8:
                    case Key.NumPad9:
                        // 允许数字输入
                        break;
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                    case Key.Tab:
                        // 允许使用Delete, Back, 左右方向键, Tab键
                        break;
                    default:
                        // 不处理其他按键
                        e.Handled = true;
                        break;
                }
            }
        }

        // 键盘按键抬起时的处理方法
        private void TbxIP_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tbx)
            {
                // 先判断数字键
                if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                {
                    // 如果文本以'0'开头且长度大于1，删除第一位
                    if (tbx.Text.StartsWith("0") && tbx.Text.Length > 1)
                    {
                        tbx.Text = tbx.Text.TrimStart('0'); // 删除开头0
                        if (tbx.Text.Length == 0) { tbx.Text = "0"; } // 如果全是0会被清空，这时补一个零
                        tbx.CaretIndex = tbx.Text.Length; // 将光标移动到文本的末尾
                    }

                    // 如果文本长度大于等于3
                    if (tbx.Text.Length >= 3)
                    {
                        tbx.Text = tbx.Text[..3]; // 保留前三位

                        if (int.TryParse(tbx.Text, out int value))
                        {
                            if (value < 0 || value > 255) // 如果不在0-255范围
                            {
                                tbx.Text = tbx.Text[..^1]; // 删除前三位的最后一位
                            }
                            else
                            {
                                SetTbxFocus(tbx, true); // 跳到下一个
                            }

                            tbx.CaretIndex = tbx.Text.Length; // 将光标移动到文本的末尾
                        }
                    }
                }
                else
                {
                    if (e.Key == Key.Left || e.Key == Key.Back) // 左方向键 或 退格键
                    {
                        if (tbx.CaretIndex == 0)
                        {
                            SetTbxFocus(tbx, false); // 跳转到前一个编辑框
                        }
                    }
                    else if (e.Key == Key.Right) // 右方向键
                    {
                        if (tbx.CaretIndex == tbx.Text.Length)
                        {
                            SetTbxFocus(tbx, true); // 跳转到下一个编辑框
                        }
                    }
                }
            }
        }

        // 设置文本框焦点的方法
        private void SetTbxFocus(TextBox currentTbx, bool isForward)
        {
            if (isForward)
            {
                if (currentTbx == Part1) MoveFocus(Part2);
                else if (currentTbx == Part2) MoveFocus(Part3);
                else if (currentTbx == Part3) MoveFocus(Part4);
            }
            else
            {
                if (currentTbx == Part4) MoveFocus(Part3);
                else if (currentTbx == Part3) MoveFocus(Part2);
                else if (currentTbx == Part2) MoveFocus(Part1);
            }
        }

        // 移动焦点到指定的文本框
        private static void MoveFocus(TextBox targetTbx)
        {
            targetTbx.Focus();
            targetTbx.SelectAll();
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Opacity = this.IsEnabled ? 1.0 : 0.5; // 禁用降低亮度
        }
    }
}
