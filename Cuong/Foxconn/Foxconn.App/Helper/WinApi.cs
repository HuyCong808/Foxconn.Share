using System;
using System.Runtime.InteropServices;

namespace Foxconn.App.Helper
{
    public class WinApi
    {
        public static IntPtr GetWindowHandle(string windowName)
        {
            IntPtr hwnd = PInvoke.User32.FindWindow(null, windowName);
            return hwnd;
        }

        public static void SetWindowSize(IntPtr hwnd, int width, int height)
        {
            // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
        }

        public static void PlacementCenterWindowInMonitorWin32(IntPtr hwnd)
        {
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            ClipOrCenterRectToMonitorWin32(ref rc);
            PInvoke.User32.SetWindowPos(hwnd, default, rc.left, rc.top, 0, 0, PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE |
                                                                              PInvoke.User32.SetWindowPosFlags.SWP_NOZORDER |
                                                                              PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private static void ClipOrCenterRectToMonitorWin32(ref PInvoke.RECT prc)
        {
            IntPtr hMonitor;
            PInvoke.RECT rc;
            int w = prc.right - prc.left;
            int h = prc.bottom - prc.top;

            hMonitor = PInvoke.User32.MonitorFromRect(ref prc, PInvoke.User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
            PInvoke.User32.MONITORINFO mi = new PInvoke.User32.MONITORINFO();
            mi.cbSize = (int)(uint)Marshal.SizeOf<PInvoke.User32.MONITORINFO>();

            PInvoke.User32.GetMonitorInfo(hMonitor, ref mi);

            rc = mi.rcWork;
            prc.left = rc.left + (rc.right - rc.left - w) / 2;
            prc.top = rc.top + (rc.bottom - rc.top - h) / 2;
            prc.right = prc.left + w;
            prc.bottom = prc.top + h;
        }

        public static void OnWindowMinimize(IntPtr hwnd)
        {
            PInvoke.User32.ShowWindow(hwnd,
                 PInvoke.User32.WindowShowStyle.SW_MINIMIZE);
        }

        public static void OnWindowMaximize(IntPtr hwnd)
        {
            PInvoke.User32.ShowWindow(hwnd,
                PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
        }

        public static void OnWindowRestore(IntPtr hwnd)
        {
            PInvoke.User32.ShowWindow(hwnd,
                PInvoke.User32.WindowShowStyle.SW_RESTORE);
        }

        public static void LoadIcon(IntPtr hwnd, string iconName)
        {
            //const int ICON_SMALL = 0;
            //const int ICON_BIG = 1;

            //fixed (char* nameLocal = iconName)
            //{
            //    HANDLE imageHandle = LoadImage(default,
            //        nameLocal,
            //        GDI_IMAGE_TYPE.IMAGE_ICON,
            //        GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSMICON),
            //        GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSMICON),
            //        IMAGE_FLAGS.LR_LOADFROMFILE | IMAGE_FLAGS.LR_SHARED);
            //    SendMessage(hwnd, WM_SETICON, ICON_SMALL, imageHandle.Value);
            //}

            //fixed (char* nameLocal = iconName)
            //{
            //    HANDLE imageHandle = LoadImage(default,
            //        nameLocal,
            //        GDI_IMAGE_TYPE.IMAGE_ICON,
            //        GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSMICON),
            //        GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSMICON),
            //        IMAGE_FLAGS.LR_LOADFROMFILE | IMAGE_FLAGS.LR_SHARED);
            //    SendMessage(hwnd, WM_SETICON, ICON_BIG, imageHandle.Value);
            //}
        }
    }
}
