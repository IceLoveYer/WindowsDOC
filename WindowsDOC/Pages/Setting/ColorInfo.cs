using System.Windows.Media;

namespace WindowsDOC.Pages.Setting
{
    // 定义预设颜色类型
    public class ColorInfo
    {
        public string Name { get; set; }
        public string ColorMain1 { get; set; }
        public string ColorMain2 { get; set; }
        public string ColorMain3 { get; set; }
        public string ColorBackground { get; set; }
        public string ColorBackgroundBorder { get; set; }
        public string ColorSidebarButton { get; set; }
        public string ColorTopBar { get; set; }
        public string ColorMainFrame { get; set; }
        public ColorInfo(string name, string colorMain1, string colorMain2, string colorMain3, string colorBackground, string colorBackgroundBorder, string colorSidebarButton, string colorTopBar, string colorMainFrame)
        {
            Name = name;
            ColorMain1 = colorMain1;
            ColorMain2 = colorMain2;
            ColorMain3 = colorMain3;
            ColorBackground = colorBackground;
            ColorBackgroundBorder = colorBackgroundBorder;
            ColorSidebarButton = colorSidebarButton;
            ColorTopBar = colorTopBar;
            ColorMainFrame = colorMainFrame;
        }
    }
}
