using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsDOC.Control;
using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages.LinkConfig
{
    /// <summary>
    /// LinkConfig.xaml 的交互逻辑
    /// </summary>
    public partial class LinkConfig : Page
    {
        public LinkConfig()
        {
            InitializeComponent();

            // 读取链接配置
            Task.Run(async () => { await ReadLinkConfigurationAsync(); });
        }



        // 读取链接配置
        public async Task ReadLinkConfigurationAsync()
        {
            var linkInfos = await Task.Run(() =>
            {
                var sharedFolders = SystemUtilities.GetSharedFolders();  // 获取共享文件夹
                var list = new List<LinkInfo>
                {
                    new (FA5Icon.Solid_Desktop, "桌面路径", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                    new (FA5Icon.Solid_Download, "下载路径",SystemUtilities.GetDownloadsPath()),
                    new (FA5Icon.Solid_FileAlt, "文档路径", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                    new (FA5Icon.Solid_Image, "图片路径", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                    new (FA5Icon.Solid_Music, "音乐路径", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                    new (FA5Icon.Solid_Video, "视频路径", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
                };

                // 添加共享文件夹
                foreach (var folder in sharedFolders)
                {
                    list.Add(new(FA5Icon.Solid_ExternalLinkAlt, "共享路径", folder));
                }

                return list;
            });

            // 更新UI元素
            Dispatcher.Invoke((() =>
            {
                ListViewItems.ItemsSource = linkInfos; // 绑定到ListView
            }));
        }



        // 打开对应属性
        private void ButtonFolderProperties_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LinkInfo linkInfo)
            {
                try
                {
                    if (linkInfo.Name != "共享路径")
                    {
                        SystemUtilities.PropertiesDialog.Show(SystemUtilities.PropertiesDialog.ObjectType.FilePath, linkInfo.Value, "位置");
                    }
                    else
                    {
                        SystemUtilities.PropertiesDialog.Show(SystemUtilities.PropertiesDialog.ObjectType.FilePath, linkInfo.Value, "共享");
                    }
                }
                catch (Exception)
                {
                    NotificationControl.Add("打开属性失败！");
                }
            }
        }
    }
}
