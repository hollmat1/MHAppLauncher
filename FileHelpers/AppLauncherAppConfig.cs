using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppLauncher.FileHelpers
{
    internal static class AppLauncherAppConfig
    {
        const string DefaultFolderName = "AppLauncher";
        const string DefaultFolder = "%OneDrive%/AppLauncher";
        const string DefaultAltFolder = "%APPDATA%/AppLauncher";
        const string DefaultDesktopRelativePath = "Desktop";
        const string DefaultAutoMapperRelativeFilePath = "NetDrives/CSNetworkDrives.txt";

        public static bool IsOneDriveFolder(string Path)
        {
            return Path.StartsWith(OneDrivePath, StringComparison.OrdinalIgnoreCase);
        }

        public static string OneDrivePath
        {
            get
            {
                var oneDrivePath =
                    Environment.ExpandEnvironmentVariables("%OneDrive%");

                if(oneDrivePath.Equals("%OneDrive%"))
                    return string.Empty;

                return oneDrivePath;

            }
        }

        public static bool OneDriveExists
        {
            get
            {
                return Directory.Exists(OneDrivePath);
            }
        }

        public static string Folder {
            get {
                var path = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Folder"]) ?
                    DefaultFolder : ConfigurationManager.AppSettings["Folder"];

                return Environment.ExpandEnvironmentVariables(path);
            }
        }

        public static string AltFolder
        {
            get
            {
                var path = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AltFolder"]) ?
                    DefaultAltFolder : ConfigurationManager.AppSettings["AltFolder"];

                return Environment.ExpandEnvironmentVariables(path);

            }
        }

        public static string DesktopRelativePath
        {
            get
            {
                return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DesktopRelativePath"]) ?
                    DefaultDesktopRelativePath : ConfigurationManager.AppSettings["DesktopRelativePath"];
            }
        }

        public static string AutoMapperRelativeFilePath
        {
            get
            {
                return string.IsNullOrEmpty(ConfigurationManager.AppSettings["AutoMapperRelativePath"]) ?
                    DefaultAutoMapperRelativeFilePath : ConfigurationManager.AppSettings["AutoMapperRelativePath"];
            }
        }

        public static string DesktopPath 
        { 
            get
            {
                if (Folder.IndexOf("%", 0) == -1)
                    return Path.Combine(Folder, DesktopRelativePath);

                // some env variable was not resolved, so fallback
                return Path.Combine(AltFolder, DesktopRelativePath);
            }
        }

        public static string AutoMapperFilePath
        {
            get
            {
                if (Folder.IndexOf("%", 0) == -1)
                    return Path.Combine(Folder, DesktopRelativePath);

                // some env variable was not resolved, so fallback
                return Path.Combine(AltFolder, AutoMapperRelativeFilePath);
            }
        }

        public static string DesktopTempPath
        {
            get
            {
                return Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), DefaultFolderName + "/" + DesktopRelativePath);
            }
        }

        public static string AutoMapperFileTempPath
        {
            get
            {
                return Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), DefaultFolderName + "/"+ AutoMapperRelativeFilePath);
            }
        }

    }
}
