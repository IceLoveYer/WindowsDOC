using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace WindowsDOC
{
    /// <summary>
    /// ElevatedFileDroper.xaml 的交互逻辑
    /// </summary>
    public partial class ElevatedFileDroper : Window
    {
        public readonly ElevatedFileDroperClass droper = new();

        public ElevatedFileDroper()
        {
            InitializeComponent();


            this.SourceInitialized += (sender, e) =>
            {
                var helper = new WindowInteropHelper(this);
                droper.HwndSource = HwndSource.FromHwnd(helper.Handle);
                droper.AddHook();
            };
            this.Closing += (sender, e) =>
            {
                droper.RemoveHook();
            };
            //droper.DragDrop += (sender, e) =>
            //{
            //    MessageBox.Show(string.Join(Environment.NewLine, droper.DropFilePaths));
            //};
        }


        public partial class ElevatedFileDroperClass
        {
            // ElevatedFileDroper.cs
            public event EventHandler? DragDrop;
            public string[] DropFilePaths { get; private set; } = Array.Empty<string>();
            public POINT DropPoint { get; private set; }
            public HwndSource? HwndSource { get; set; }

            public void AddHook()
            {
                if (this.HwndSource == null) return;

                this.RemoveHook();
                this.HwndSource.AddHook(WndProc);
                IntPtr handle = this.HwndSource.Handle;
                if (IsUserAnAdmin()) _ = RevokeDragDrop(handle);
                DragAcceptFiles(handle, true);
                ChangeMessageFilter(handle);
            }

            public void RemoveHook()
            {
                if (this.HwndSource == null) return;

                this.HwndSource.RemoveHook(WndProc);
                DragAcceptFiles(this.HwndSource.Handle, false);
            }

            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {

                if (TryGetDropInfo(msg, wParam, out string[]? filePaths, out POINT point))
                {
                    DropPoint = point;
                    DropFilePaths = filePaths ?? Array.Empty<string>();
                    DragDrop?.Invoke(this, EventArgs.Empty);
                    handled = true;
                }
                return IntPtr.Zero;
            }



            // ElevatedFileDroper.win32.cs
            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, uint action, ref CHANGEFILTERSTRUCT pChangeFilterStruct);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool ChangeWindowMessageFilter(uint msg, uint flags);

            [DllImport("shell32.dll")]
            private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            private static extern uint DragQueryFile(IntPtr hWnd, uint iFile, StringBuilder? lpszFile, int cch);

            [DllImport("shell32.dll")]
            private static extern bool DragQueryPoint(IntPtr hDrop, out POINT lppt);

            [DllImport("shell32.dll")]
            private static extern void DragFinish(IntPtr hDrop);

            [DllImport("ole32.dll")]
            private static extern int RevokeDragDrop(IntPtr hWnd);

            [DllImport("shell32.dll")]
            private static extern bool IsUserAnAdmin();

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct CHANGEFILTERSTRUCT
            {
                public uint cbSize;
                public uint ExtStatus;
            }

            const uint WM_COPYGLOBALDATA = 0x0049;
            const uint WM_COPYDATA = 0x004A;
            const uint WM_DROPFILES = 0x0233;
            const uint MSGFLT_ALLOW = 1;
            const uint MSGFLT_ADD = 1;
            const uint MAX_PATH = 260;

            private static void ChangeMessageFilter(IntPtr handle)
            {
                Version ver = Environment.OSVersion.Version;
                bool isVistaOrHigher = ver >= new Version(6, 0);
                bool isWin7OrHigher = ver >= new Version(6, 1);
                if (isVistaOrHigher)
                {
                    var status = new CHANGEFILTERSTRUCT { cbSize = 8 };
                    foreach (uint msg in new[] { WM_DROPFILES, WM_COPYGLOBALDATA, WM_COPYDATA })
                    {
                        bool error = false;
                        if (isWin7OrHigher) error = !ChangeWindowMessageFilterEx(handle, msg, MSGFLT_ALLOW, ref status);
                        else error = !ChangeWindowMessageFilter(msg, MSGFLT_ADD);
                        if (error) throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }

            private static bool TryGetDropInfo(int msg, IntPtr wParam, out string[]? dropFilePaths, out POINT dropPoint)
            {
                dropFilePaths = null;
                dropPoint = new POINT();
                if (msg != WM_DROPFILES) return false;
                uint fileCount = DragQueryFile(wParam, uint.MaxValue, null, 0);
                dropFilePaths = new string[fileCount];
                for (uint i = 0; i < fileCount; i++)
                {
                    var sb = new StringBuilder((int)MAX_PATH);
                    uint result = DragQueryFile(wParam, i, sb, sb.Capacity);
                    if (result > 0) dropFilePaths[i] = sb.ToString();
                }
                DragQueryPoint(wParam, out dropPoint);
                DragFinish(wParam);
                return true;
            }
        }
    }
}
