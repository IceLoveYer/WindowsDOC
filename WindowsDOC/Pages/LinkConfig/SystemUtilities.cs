using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace WindowsDOC.Pages.LinkConfig
{
    public static class SystemUtilities
    {
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
        public static List<string> GetSharedFolders()
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

        // 显示指定对象的属性对话框
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
