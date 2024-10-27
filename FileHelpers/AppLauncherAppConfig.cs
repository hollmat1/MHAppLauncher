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
        const string RootFolderName = "AppLauncher";
        const string DefaultFolder = "%OneDrive%";
        const string DefaultAltFolder = "%APPDATA%";
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
                return !Directory.Exists(OneDrivePath);
            }
        }

        public static string Folder 
        {
            get 
            {
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
                // some env variable was not resolved, so fallback
                return Path.Combine(Folder, RootFolderName, DesktopRelativePath);
            }
        }

        public static string AutoMapperFilePath
        {
            get
            {
                return Path.Combine(Folder, RootFolderName, AutoMapperRelativeFilePath);
            }
        }

        public static string DesktopAltPath
        {
            get
            {
                // some env variable was not resolved, so fallback
                return Path.Combine(AltFolder, RootFolderName, DesktopRelativePath);
            }
        }

        public static string AutoMapperFileAltPath
        {
            get
            {
                // some env variable was not resolved, so fallback
                return Path.Combine(AltFolder, RootFolderName, DesktopRelativePath);
            }
        }      

        public static string DesktopTempPath
        {
            get
            {
                return Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), RootFolderName + "/" + DesktopRelativePath);
            }
        }

        public static string AutoMapperFileTempPath
        {
            get
            {
                return Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), RootFolderName + "/"+ AutoMapperRelativeFilePath);
            }
        }

    }
}
