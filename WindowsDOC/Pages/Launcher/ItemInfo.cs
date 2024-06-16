using System.Windows.Media.Imaging;

namespace WindowsDOC.Pages.Launcher
{
    public class ItemInfo // 定义项目类型
    {
        public BitmapSource? Image { get; set; } // 图像
        public string ImagePath { get; set; } = ""; // 图像文件路径
        public string Text { get; set; } = ""; // 标题
        public string Content { get; set; } = ""; // 运行：路径/命令
        public string Content64 { get; set; } = ""; // 运行64位应用程序路径
        public bool IsCommand { get; set; } = false; // 项目类型：CMD/路径
        public bool IsCommandWindowsHide { get; set; } = false; // 项目类型：CMD/路径
        public bool IsRelativePath { get; set; } = false; // 路径类型：相对/绝对路径
    }
}
