using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDOC
{
    /// <summary>
    /// SphericalProgressControl.xaml 的交互逻辑
    /// </summary>
    public partial class SphericalProgressControl : UserControl
    {
        public SphericalProgressControl()
        {
            InitializeComponent();
        }



        // 边框色
        public SolidColorBrush BorderColor
        {
            get { return (SolidColorBrush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(SolidColorBrush), typeof(SphericalProgressControl), new(Brushes.Black));

        // 背景色
        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(SolidColorBrush), typeof(SphericalProgressControl), new(Brushes.White));



        // 进度
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set
            {
                SetValue(ProgressProperty, value);
            }
        }
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(SphericalProgressControl), new(160.0, OnProgressChanged)); // 默认值为160，同时注册了属性变更的回调函数
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SphericalProgressControl control)
            {
                double progress = (double)e.NewValue;
                control.TranslateTransformProgress.Y = 160 - progress;
            }
        }


        // 进度背景色
        public SolidColorBrush ProgressColor
        {
            get { return (SolidColorBrush)GetValue(ProgressColorProperty); }
            set { SetValue(ProgressColorProperty, value); }
        }
        public static readonly DependencyProperty ProgressColorProperty =
            DependencyProperty.Register("ProgressColor", typeof(SolidColorBrush), typeof(SphericalProgressControl), new(Brushes.Black));

        // 进度文本
        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(SphericalProgressControl), new(string.Empty));

        // 进度文本颜色
        public SolidColorBrush ProgressTextColor
        {
            get { return (SolidColorBrush)GetValue(ProgressTextColorProperty); }
            set { SetValue(ProgressTextColorProperty, value); }
        }
        public static readonly DependencyProperty ProgressTextColorProperty =
            DependencyProperty.Register("ProgressTextColor", typeof(SolidColorBrush), typeof(SphericalProgressControl), new(Brushes.Blue));
    }
}
