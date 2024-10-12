using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppLauncher.FileHelpers
{
    internal class ShortCutHelper
    {
        internal static void CreateShortcut(string Description, string Target, string ShortcutPath, string ShortcutName, string Arguments = null)
        {
            IShellLink link = (IShellLink)new ShellLink();

            if (string.IsNullOrEmpty(ShortcutPath)) 
            {
                ShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                
            }

            if(Target.ToLower().Contains("[/args]"))
            {
                Arguments = Target.Substring(Target.IndexOf("[/args]") +7);
                Target = Target.Substring(0, Target.IndexOf("[/args]")).Trim();
            }

            link.SetDescription(Description);
            link.SetPath(Target);
            if (!string.IsNullOrEmpty(Arguments))
            {
                link.SetArguments(Arguments);
            }
            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(ShortcutPath, ShortcutName), false);
        }

        public static string ResolveShortcutTarget(string filePath)
        {
            dynamic shortcut;
            dynamic windowsShell;

            try
            {

                if (Path.GetExtension(filePath)?.Equals(".lnk", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Type shellObjectType = Type.GetTypeFromProgID("WScript.Shell");
                    windowsShell = Activator.CreateInstance(shellObjectType);
                    shortcut = windowsShell.CreateShortcut(filePath);
                    var path = shortcut.TargetPath;
                    shortcut = null;
                    windowsShell = null;
                    return path;
                }
            }
            finally
            {
                shortcut = null;
                windowsShell = null;
            }

            return null;
        }

        public static bool IsTargetNetworkPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                return false;

            if (path.StartsWith(@"//") || path.StartsWith(@"\\"))
                return true;

            var root = Path.GetPathRoot(path);
            if (!root.Equals("C") && !root.Equals("D"))
                return true;

            return false;
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}

