using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages.Setting
{
    // 链接信息 数据绑定
    public class DirInfo
    {
        // 属性声明
        public FA5Icon Icon { get; set; }
        public string Name { get; set; }
        public string Dir { get; set; }
        public string? Json { get; set; }

        // 构造函数
        public DirInfo(FA5Icon icon, string name, string dir, string? json)
        {
            Icon = icon;
            Name = name;
            Dir = dir;
            Json = json;
        }
    }
}
