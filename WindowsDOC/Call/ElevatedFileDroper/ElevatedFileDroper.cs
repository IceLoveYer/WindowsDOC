using System;
using System.Windows.Interop;

namespace WindowsDOC
{
    public partial class ElevatedFileDroper
    {
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
    }
}