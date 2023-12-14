using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages
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
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await ReadLinkConfigurationAsync();
            }));
        }


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


        // 读取链接配置
        public async Task ReadLinkConfigurationAsync()
        {
            var linkInfos = await Task.Run(() =>
            {
                var sharedFolders = GetSharedFolders();  // 获取共享文件夹
                var list = new List<LinkInfo>
                {
                    new (FA5Icon.Solid_Desktop, "桌面路径", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                    new (FA5Icon.Solid_Download, "下载路径",GetDownloadsPath()),
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

            // 在主线程上更新 ListViewItems 的 ItemsSource
            Dispatcher.Invoke(() =>
            {
                ListViewItems.ItemsSource = linkInfos;
            });
        }


        // 获取下载路径
        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
        private static readonly Guid DownloadsFolderGuid = new("374DE290-123F-4565-9164-39C4925E467B");
        public static string GetDownloadsPath()
        {
            if (SHGetKnownFolderPath(DownloadsFolderGuid, 0, IntPtr.Zero, out IntPtr pszPath) == 0)
            {
                string? path = Marshal.PtrToStringAuto(pszPath);
                Marshal.FreeCoTaskMem(pszPath);
                return path ?? string.Empty;
            }
            return string.Empty;
        }


        // 获取共享文件夹
        private static List<string> GetSharedFolders()
        {
            List<string> sharedFolders = new();

            ProcessStartInfo startInfo = new("net", "share")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                using StreamReader? reader = process?.StandardOutput;
                string? line;
                while ((line = reader?.ReadLine()) != null)
                {
                    if (line.Contains("共享名")) continue;
                    if (line.Trim() == "") continue;
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && !parts[0].EndsWith("$")) // 排除以 $ 结尾的共享名称
                    {
                        sharedFolders.Add(parts[1]);
                    }
                }
            }

            return sharedFolders;
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
                        PropertiesDialog.Show(PropertiesDialog.ObjectType.FilePath, linkInfo.Value, "位置");
                    }
                    else
                    {
                        PropertiesDialog.Show(PropertiesDialog.ObjectType.FilePath, linkInfo.Value, "共享");
                    }
                }
                catch (Exception)
                {
                    NotificationControl.Add("打开属性失败！");
                }
            }
        }


        public static class PropertiesDialog
        {
            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            private static extern bool SHObjectProperties(IntPtr hwnd, ObjectType shopObjectType, string pszObjectName, string? pszPropertyPage);

            /// <summary>对象类型</summary>
            public enum ObjectType : uint
            {
                /// <summary>打印机名称</summary>
                PrinterName = 0x01,
                /// <summary>文件或文件夹路径</summary>
                FilePath = 0x02,
                /// <summary>卷GUID</summary>
                VoumeGuid = 0x04,
            }

            /// <summary>显示指定对象的属性对话框</summary>
            /// <param name="objectType">对象类型</param>
            /// <param name="objectName">文件或文件夹路径、打印机名称、卷GUID</param>
            /// <param name="pageName">指定标题选项卡，未找到则为默认页，有UI语言差异</param>
            /// <returns>是否成功调用命令</returns>
            public static bool Show(ObjectType objectType, string objectName, string? pageName = null)
            {
                return SHObjectProperties(IntPtr.Zero, objectType, objectName, pageName);
            }
        }
    }
}
