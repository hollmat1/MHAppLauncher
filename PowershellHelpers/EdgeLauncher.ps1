
function Get-Edge {
    $p = [AppLauncher.NativeMethods]::GetProcess()

    $windowHandle = $p[0].MainWindowHandle

    if ([AppLauncher.NativeMethods]::IsIconic($windowHandle)) {
        [AppLauncher.NativeMethods]::ShowWindow($windowHandle, [AppLauncher.NativeMethods.ShowWindowCommand]::Restore)
    }

    [AppLauncher.NativeMethods]::SetForegroundWindow($windowHandle);
}

$code = @"
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AppLauncher
{
    public static class NativeMethods
    {
        
        public static Process[] GetProcess(string Name = "msedge")
        {
            return Process.GetProcessesByName(Name);
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean IsIconic([In] IntPtr windowHandle);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean SetForegroundWindow([In] IntPtr windowHandle);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean ShowWindow([In] IntPtr windowHandle, [In] ShowWindowCommand command);

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
"@

Add-Type -Language CSharp $code

Get-Edge
