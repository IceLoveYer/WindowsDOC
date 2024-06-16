using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace WindowsDOC.Pages.Launcher
{
    public class ItemGroupInfo // 定义分组类型
    {
        public BitmapSource? Image { get; set; } // 图像
        public string ImagePath { get; set; } = ""; // 图像文件路径
        public string Text { get; set; } = ""; // 标题
        public ObservableCollection<ItemInfo> Items { get; set; } = new();// 项目
    }
}
