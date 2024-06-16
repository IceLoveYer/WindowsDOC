using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WindowsDOC.Call
{
    public class ResourceIcon
    {
        #region WIN32

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint PrivateExtractIcons(string szFileName, int nIconIndex,
            int cxIcon, int cyIcon, IntPtr[] phicon, uint[] piconid, uint nIcons, uint flags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList? ppv);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left, top, right, bottom;
        }

        [Flags]
        private enum SHGFI : uint
        {
            SHGFI_ICON = 0x000000100,               // get icon
            SHGFI_DISPLAYNAME = 0x000000200,        // get display name
            SHGFI_TYPENAME = 0x000000400,           // get type name
            SHGFI_ATTRIBUTES = 0x000000800,         // get attributes
            SHGFI_ICONLOCATION = 0x000001000,       // get icon location
            SHGFI_EXETYPE = 0x000002000,            // return exe type
            SHGFI_SYSICONINDEX = 0x000004000,       // get system icon index
            SHGFI_LINKOVERLAY = 0x000008000,        // put a link overlay on icon
            SHGFI_SELECTED = 0x000010000,           // show icon in selected state
            SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
            SHGFI_LARGEICON = 0x000000000,          // get large icon
            SHGFI_SMALLICON = 0x000000001,          // get small icon
            SHGFI_OPENICON = 0x000000002,           // get open icon
            SHGFI_SHELLICONSIZE = 0x000000004,      // get shell size icon
            SHGFI_PIDL = 0x000000008,               // pszPath is a pidl
            SHGFI_USEFILEATTRIBUTES = 0x000000010,  // use passed dwFileAttribute
            SHGFI_ADDOVERLAYS = 0x000000020,        // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040        // get the index of the overlay in the upper 8 bits of the iIcon
        }

        [Flags]
        private enum ILD : uint
        {
            ILD_NORMAL = 0x00000000,
            ILD_TRANSPARENT = 0x00000001,
            ILD_BLEND25 = 0x00000002, //ILD_FOCUS
            ILD_BLEND50 = 0x00000004, //ILD_SELECTED ILD_BLEND
            ILD_MASK = 0x00000010,
            ILD_IMAGE = 0x00000020,
            ILD_ROP = 0x00000040,
            ILD_OVERLAYMASK = 0x00000F00,
            ILD_PRESERVEALPHA = 0x00001000,
            ILD_SCALE = 0x00002000,
            ILD_DPISCALE = 0x00004000,
            ILD_ASYNC = 0x00008000,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;
            public int yBitmap;
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [ComImport, Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList
        {
            [PreserveSig] int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
            [PreserveSig] int ReplaceIcon(int i, IntPtr hicon, ref int pi);
            [PreserveSig] int SetOverlayImage(int iImage, int iOverlay);
            [PreserveSig] int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
            [PreserveSig] int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
            [PreserveSig] int Draw(ref IMAGELISTDRAWPARAMS pimldp);
            [PreserveSig] int Remove(int i);
            [PreserveSig] int GetIcon(int i, ILD flags, ref IntPtr picon);
            [PreserveSig] int GetImageInfo(int i, ref IMAGEINFO pImageInfo);
            [PreserveSig] int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);
            [PreserveSig] int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int Clone(ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int GetImageRect(int i, ref RECT prc);
            [PreserveSig] int GetIconSize(ref int cx, ref int cy);
            [PreserveSig] int SetIconSize(int cx, int cy);
            [PreserveSig] int GetImageCount(ref int pi);
            [PreserveSig] int SetImageCount(int uNewCount);
            [PreserveSig] int SetBkColor(int clrBk, ref int pclr);
            [PreserveSig] int GetBkColor(ref int pclr);
            [PreserveSig] int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);
            [PreserveSig] int EndDrag();
            [PreserveSig] int DragEnter(IntPtr hwndLock, int x, int y);
            [PreserveSig] int DragLeave(IntPtr hwndLock);
            [PreserveSig] int DragMove(int x, int y);
            [PreserveSig] int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);
            [PreserveSig] int DragShowNolock(int fShow);
            [PreserveSig] int GetDragImage(ref Point ppt, ref Point pptHotspot, ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int GetItemFlags(int i, ref int dwFlags);
            [PreserveSig] int GetOverlayImage(int iOverlay, ref int piIndex);
        }

        /// <summary>图标大小</summary>
        public enum IconSize : int
        {
            /// <summary>Shell 标准小图标大小，通常为16x16</summary>
            Small = 1,
            /// <summary>Shell 标准图标大小，通常为32x32</summary>
            Large = 0,
            /// <summary>Shell 标准超大图标大小，通常为48x48</summary>
            ExtraLarge = 2,
            /// <summary>巨型图标，大小始终为256x256</summary>
            /// <remarks>Windows Vista 及更高版本</remarks>
            Jumbo = 4,
        }

        #endregion

        /// <summary>获取默认的文件或文件夹图标</summary>
        /// <param name="size">图标大小</param>
        /// <param name="isFile">true获取文件的，false获取文件夹的</param>
        /// <param name="autoScale">根据系统显示比例重新计算宽度和高度</param>
        public static Icon GetDefaultIcon(IconSize size, bool isFile, bool autoScale = true)
        {
            int w = 32;
            int h = 32;
            switch (size)
            {
                case IconSize.Small: w = h = 16; break;
                case IconSize.Large: w = h = 32; break;
                case IconSize.ExtraLarge: w = h = 48; break;
                case IconSize.Jumbo: w = h = 256; autoScale = false; break;
            }
            int index = isFile ? 0 : 3;
            var icon = GetIcon("shell32.dll", index, w, h, autoScale);
            return icon;
        }

        /// <summary>获取指定扩展名或指定文件夹的图标</summary>
        /// <param name="fileName">文件路径、扩展名或文件夹路径</param>
        /// <param name="size">图标大小</param>
        /// <param name="isFile">是否是文件路径或扩展名</param>
        public static Icon? GetIcon(string fileName, IconSize size, bool isFile = true)
        {
            if (isFile && string.IsNullOrEmpty(Path.GetExtension(fileName))) return GetDefaultIcon(size, isFile);

            var info = new SHFILEINFO();
            SHGFI shgfi = SHGFI.SHGFI_SYSICONINDEX;
            if (isFile) shgfi |= SHGFI.SHGFI_USEFILEATTRIBUTES;
            IntPtr hInfo = SHGetFileInfo(fileName, 0, ref info, (uint)Marshal.SizeOf(info), shgfi);
            if (hInfo.Equals(IntPtr.Zero)) return null;
            IImageList? list = null;
            Guid guid = typeof(IImageList).GUID;
            _ = SHGetImageList((int)size, ref guid, ref list);
            if (list == null) return null;
            IntPtr hIcon = IntPtr.Zero;
            list.GetIcon(info.iIcon, ILD.ILD_IMAGE | ILD.ILD_TRANSPARENT, ref hIcon);
            Marshal.ReleaseComObject(list);
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(info.hIcon);
            return icon;
        }

        /// <summary>获取指定文件中指定序号和指定大小的图标</summary>
        /// <param name="filePath">包含图标资源的exe、dll、ico文件路径</param>
        /// <param name="index">图标资源索引号或ID</param>
        /// <param name="width">图标宽度</param>
        /// <param name="height">图标高度</param>
        /// <param name="autoScale">根据系统显示比例重新计算宽度和高度</param>
        public static Icon GetIcon(string filePath, int index = 0,
                    int width = 32, int height = 32, bool autoScale = true)
        {
            if (autoScale)
            {
                using var src = new HwndSource(new HwndSourceParameters());
                var matrix = src.CompositionTarget.TransformToDevice;
                width = (int)(matrix.M22 * width);
                height = (int)(matrix.M22 * height);
            }
            IntPtr[] hIcons = new IntPtr[1];
            uint[] iconIds = new uint[1];
            _ = PrivateExtractIcons(filePath, index, width, height, hIcons, iconIds, 1, 0);
            Icon icon = (Icon)Icon.FromHandle(hIcons[0]).Clone();
            DestroyIcon(hIcons[0]);
            return icon;
        }

        /// <summary>获取指定大小的主程序图标</summary>
        /// <param name="width">图标宽度</param>
        /// <param name="height">图标高度</param>
        /// <param name="autoScale">根据系统显示比例重新计算宽度和高度</param>
        public static Icon GetAppIcon(int width, int height, bool autoScale)
        {

            string filePath = System.Windows.Forms.Application.ExecutablePath;
            return GetIcon(filePath, 0, width, height, autoScale);
        }


        // WPF获取最大图标至BitmapSource类型
        public static BitmapSource? GetIconWpfJumbo(string filePath)
        {
            Icon? icon = GetIcon(filePath, IconSize.Jumbo, !Directory.Exists(filePath));
            if (icon != null)
            {
                return CropTransparentPixels(CreateFormIcon(icon, true));
            }
            else
            {
                return null;
            }
        }

        // Icon转换BitmapSource
        public static BitmapSource CreateFormIcon(Icon icon, bool dispose)
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
            System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            if (dispose) icon.Dispose();
            return source;
        }
        // 裁剪透明区域，256的图有的带空白像素
        public static WriteableBitmap CropTransparentPixels(BitmapSource original)
        {
            int width = original.PixelWidth;
            int height = original.PixelHeight;
            int[] pixels = new int[width * height];
            int stride = width * 4;

            original.CopyPixels(pixels, stride, 0);

            int minX = width, minY = height, maxX = 0, maxY = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    int pixel = pixels[i];
                    byte a = (byte)(pixel >> 24 & 0xFF);

                    if (a != 0)  // Not transparent
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            int cropWidth = maxX - minX + 1;
            int cropHeight = maxY - minY + 1;

            WriteableBitmap croppedBitmap = new(cropWidth, cropHeight, original.DpiX, original.DpiY, original.Format, null);
            int[] croppedPixels = new int[cropWidth * cropHeight];

            for (int y = 0; y < cropHeight; y++)
            {
                for (int x = 0; x < cropWidth; x++)
                {
                    int i = (y + minY) * width + x + minX;
                    croppedPixels[y * cropWidth + x] = pixels[i];
                }
            }

            croppedBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, cropWidth, cropHeight), croppedPixels, cropWidth * 4, 0);

            return croppedBitmap;
        }

    }
}
