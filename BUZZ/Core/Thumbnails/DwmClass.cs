using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BUZZ.Core.Thumbnails
{
    public class DwmClass
    {

        public const int DwmTnpVisible = 0x8,
            DwmTnpOpacity = 0x4,
            DwmTnpRectdestination = 0x1;

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(System.IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out Psize size);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DwmThumbnailProperties props);

        // User32 things

        public const int GWL_STYLE = -16;

        public const ulong WsVisible = 0x10000000L,
            TargetWindow =  WsVisible;

        [DllImport("user32")]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        public struct Psize
        {
            public int x;
            public int y;
        }

        public struct DwmThumbnailProperties
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        public struct Rect
        {
            public Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;

            public int Height => Bottom - Top;
        }
    }
}
