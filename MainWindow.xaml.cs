using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AppLauncher;
using AppLauncher.ShellClasses;
using static AppLauncher.NativeMethods;
using System.Configuration;

namespace AppLauncher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileSystemObjectInfo> FilesCollection { get; set; }
        public string _folderPath;

        public MainWindow()
        {
            String assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            using (Mutex mutex = new Mutex(false, assemblyName))
            {
                if (!mutex.WaitOne(0, false))
                {
                    //Boolean shownProcess = false;
                    Process currentProcess = Process.GetCurrentProcess();

                    foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
                    {
                        if (!process.Id.Equals(currentProcess.Id) && process.MainModule.FileName.Equals(currentProcess.MainModule.FileName) && !process.MainWindowHandle.Equals(IntPtr.Zero))
                        {
                            IntPtr windowHandle = process.MainWindowHandle;

                            if (NativeMethods.IsIconic(windowHandle))
                                NativeMethods.ShowWindow(windowHandle, ShowWindowCommand.Restore);

                            NativeMethods.SetForegroundWindow(windowHandle);

                            //shownProcess = true;
                        }
                    }

                    //if (!shownProcess)
                        //MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "An instance of {0} is already running!", assemblyName), assemblyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
                }
                else
                {
                    _folderPath = ConfigurationManager.AppSettings["Folder"];
                    EnumerateFiles();
                }
            }
        }


        // Minimize to system tray when application is minimized.
        protected override void OnStateChanged(EventArgs e)
        {
            //if (WindowState == WindowState.Minimized) this.Hide();

            base.OnStateChanged(e);
        }

        // Minimize to system tray when application is closed.
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            e.Cancel = true;

            //this.Hide();
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = true;

            base.OnClosing(e);
        }

        public void EnumerateFiles()
        {
            //FilesCollection = new ObservableCollection<DownloadedFile>();
            FilesCollection = new ObservableCollection<FileSystemObjectInfo>();

            var folder = new DirectoryInfo(_folderPath);
            var downloadedAttachments = folder.GetFiles("*");

            foreach (var file in downloadedAttachments)
            {
                var fileInfo = new FileSystemObjectInfo(file);
                FilesCollection.Add(fileInfo);
            }
        }

        public void openFile(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                foreach (object o in ListBoxFiles.SelectedItems)
                    Process.Start((o as FileSystemObjectInfo).FilePath);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
