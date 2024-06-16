using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages.LinkConfig
{
    // 链接信息 数据绑定
    public class LinkInfo
    {
        // 属性声明
        public FA5Icon Icon { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        // 构造函数
        public LinkInfo(FA5Icon icon, string name, string value)
        {
            Icon = icon;
            Name = name;
            Value = value;
        }
    }
}
