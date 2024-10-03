using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean IsIconic([In] IntPtr windowHandle);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean SetForegroundWindow([In] IntPtr windowHandle);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean ShowWindow([In] IntPtr windowHandle, [In] ShowWindowCommand command);

        public enum ShowWindowCommand : int
        {
            Hide = 0x0,
            ShowNormal = 0x1,
            ShowMinimized = 0x2,
            ShowMaximized = 0x3,
            ShowNormalNotActive = 0x4,
            Minimize = 0x6,
            ShowMinimizedNotActive = 0x7,
            ShowCurrentNotActive = 0x8,
            Restore = 0x9,
            ShowDefault = 0xA,
            ForceMinimize = 0xB
        }
    }
}
