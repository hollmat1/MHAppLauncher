using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using AppLauncher.Enums;

namespace AppLauncher.ShellClasses
{
    public class FileSystemObjectInfo : BaseObject
    {
        public FileSystemObjectInfo(FileSystemInfo info)
        {
            //if (this is DummyFileSystemObjectInfo)
            //{
            //return;
            //}

            FileSystemInfo = info;

            if (info is DirectoryInfo)
            {
                ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
                //AddDummy();
            }
            else if (info is FileInfo)
            {
                ImageSource = FileManager.GetImageSource(info.FullName);
                //ImageSource = FileManager.GetImageSource(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", new Size(33, 33));
            }

            //PropertyChanged += new PropertyChangedEventHandler(FileSystemObjectInfo_PropertyChanged);
        }

        public FileSystemObjectInfo(DriveInfo drive)
            : this(drive.RootDirectory)
        {
        }

        #region Properties

        public ImageSource ImageSource
        {
            get { return GetValue<ImageSource>("ImageSource"); }
            private set { SetValue("ImageSource", value); }
        }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(FileSystemInfo.Extension))
                {
                    return FileSystemInfo.Name.Substring(0, FileSystemInfo.Name.Length - FileSystemInfo.Extension.Length);
                }

                return FileSystemInfo.Name;
            }

        }

        public FileSystemInfo FileSystemInfo
        {
            get { return GetValue<FileSystemInfo>("FileSystemInfo"); }
            private set { SetValue("FileSystemInfo", value); }
        }

        public string FilePath => Path.GetFullPath(FileSystemInfo.FullName);

        #endregion

    }
}
