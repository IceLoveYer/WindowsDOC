using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsDOC
{
    public class IconExtractor
    {
        [DllImport("shell32.dll", EntryPoint = "#727")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

        [ComImport]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList
        {
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);
            int SetOverlayImage(int iImage, int iOverlay);
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
            int AddMasked(IntPtr hbmImage, uint crMask, ref int pi);
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);
            int Remove(int i);
            int GetIcon(int i, int flags, out IntPtr picon);
            // ... 省略其他方法
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
            public uint rgbBk;
            public uint rgbFg;
            public uint fStyle;
            public uint dwRop;
            public uint fState;
            public uint Frame;
            public uint crEffect;
        }

        public static ImageSource? ExtractJumboIcon(string filePath)
        {
            int SHIL_JUMBO = 0x4;
            Guid IID_IImageList = new("46EB5926-582E-4017-9FDF-E8998DAA0950");

            int hResult = SHGetImageList(SHIL_JUMBO, ref IID_IImageList, out IImageList imageList);

            if (hResult == 0 && imageList != null)
            {
                SHFILEINFO shinfo = new();
                SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), 0x100);
                int iconIndex = shinfo.iIcon;

                int res = imageList.GetIcon(iconIndex, 0x000000001, out IntPtr hIcon);  // ILD_TRANSPARENT

                if (res == 0)
                {
                    ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    //return wpfBitmap;
                    return CropTransparentPixels((BitmapSource)wpfBitmap);
                }
            }
            return null;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
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

        // 裁剪透明区域
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

            croppedBitmap.WritePixels(new Int32Rect(0, 0, cropWidth, cropHeight), croppedPixels, cropWidth * 4, 0);

            return croppedBitmap;
        }



    }
}