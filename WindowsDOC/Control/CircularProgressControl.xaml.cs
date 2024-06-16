using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDOC.Control
{
    /// <summary>
    /// CircularProgressControl.xaml 的交互逻辑
    /// </summary>
    public partial class CircularProgressControl : UserControl
    {
        public CircularProgressControl()
        {
            InitializeComponent();
        }

        // 主色调1
        public Color PrimaryColor1
        {
            get { return (Color)GetValue(PrimaryColor1Property); }
            set { SetValue(PrimaryColor1Property, value); }
        }
        public static readonly DependencyProperty PrimaryColor1Property = DependencyProperty.Register(
            "PrimaryColor1", typeof(Color), typeof(CircularProgressControl), new PropertyMetadata(Colors.LightBlue));


        // 主色调2
        public Color PrimaryColor2
        {
            get { return (Color)GetValue(PrimaryColor2Property); }
            set { SetValue(PrimaryColor2Property, value); }
        }
        public static readonly DependencyProperty PrimaryColor2Property = DependencyProperty.Register(
            "PrimaryColor2", typeof(Color), typeof(CircularProgressControl), new PropertyMetadata(Colors.Blue));

        // 进度文本
        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(CircularProgressControl), new(string.Empty));

        // 进度厚度
        public double ProgressThickness
        {
            get { return (double)GetValue(ProgressThicknessProperty); }
            set { SetValue(ProgressThicknessProperty, value); }
        }
        public static readonly DependencyProperty ProgressThicknessProperty = DependencyProperty.Register(
            "ProgressThickness", typeof(double), typeof(CircularProgressControl), new PropertyMetadata(30.0));



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
            DependencyProperty.Register("Progress", typeof(double), typeof(CircularProgressControl), new(0.0, OnProgressChanged)); // 默认值为160，同时注册了属性变更的回调函数
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularProgressControl control)
            {
                double progress = (double)e.NewValue;
                control.Arc_Progress.EndAngle = 360 * (progress / 100);
            }
        }
    }
}
