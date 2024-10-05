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
using System.Windows.Controls;
using AppLauncher.FileHelpers;
using System.Collections;
using System.Collections.Specialized;

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
                    _folderPath = Environment.ExpandEnvironmentVariables(_folderPath);

                    try
                    {
                        if (!Directory.Exists(_folderPath))
                        {
                            Directory.CreateDirectory(_folderPath);

                            var defaultShortcuts = (NameValueCollection)ConfigurationManager.GetSection("DefaultShortcuts");

                            if (defaultShortcuts != null)
                            {
                                foreach (var shortcutKey in defaultShortcuts.AllKeys)
                                {
                                    ShortCutHelper.CreateShortcut(shortcutKey, defaultShortcuts[shortcutKey], _folderPath, $"{shortcutKey}.lnk");
                                }
                            }
                        }

                        EnumerateFiles();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred deleting file.  {ex.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    }

                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            //if (WindowState == WindowState.Minimized) this.Hide();

            base.OnStateChanged(e);
        }

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

        private void File_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var f in files)
                {
                    var fi = new FileInfo(f);

                    // not a shortcut so create one
                    if (!f.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) 
                    {
                        var dialog = new PromptNameDialog();
                        if (dialog.ShowDialog() == true)
                        {
                            var Name = dialog.ResponseText;
                            var Parent = Directory.GetParent(fi.FullName).FullName;

                            var newSCPath = Path.Combine(_folderPath, $"{Name}.lnk");

                            if ((File.Exists(newSCPath)))
                            {
                                MessageBoxResult result = MessageBox.Show($"Shortcut already exists?  Overwrite", "Overwrite existing shortcut?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                                if (result == MessageBoxResult.Yes)
                                {
                                    File.Delete(newSCPath);
                                    ShortCutHelper.CreateShortcut(Name, fi.FullName, _folderPath, $"{Name}.lnk");
                                }
                            }
                            else
                            {
                                ShortCutHelper.CreateShortcut(Name, fi.FullName, _folderPath, $"{Name}.lnk");

                                FilesCollection.Add(new FileSystemObjectInfo(new FileInfo(newSCPath)));
                            }

                        }

                        continue;
                    }

                    var newPath = Path.Combine(_folderPath, fi.Name);

                    if ((File.Exists(newPath)))
                    {
                        MessageBoxResult result = MessageBox.Show($"Shortcut already exists?  Overwrite", "Overwrite existing shortcut?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                        if(result == MessageBoxResult.Yes)
                            File.Copy(f, Path.Combine(_folderPath, fi.Name), true);
                    }
                    else
                    {
                        File.Copy(f, Path.Combine(_folderPath, fi.Name), true);
                        FilesCollection.Add(new FileSystemObjectInfo(new FileInfo(newPath)));
                    }
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            string caption = "Deletion";

            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm.DataContext is FileSystemObjectInfo)
                {
                    var theFile = (FileSystemObjectInfo)cm.DataContext;

                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {theFile.Name}?", caption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                    if (result == MessageBoxResult.No)
                        return;

                    try
                    {
                        File.Delete(theFile.FilePath);
                        FilesCollection.Remove(theFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred deleting file.  {ex.Message}", caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    }

                    //Grid g = cm.PlacementTarget as Grid;
                    //if (g != null)
                    //{
                    //Console.WriteLine(g.Background); // Will print red
                    //}
                }
            }


        }

        private void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            string caption = "Rename";

            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm.DataContext is FileSystemObjectInfo)
                {
                    var theFile = (FileSystemObjectInfo)cm.DataContext;

                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {theFile.Name}?", caption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                    if (result == MessageBoxResult.No)
                        return;

                    try
                    {
                        File.Delete(theFile.FilePath);
                        FilesCollection.Remove(theFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred deleting file.  {ex.Message}", caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    }

                    //Grid g = cm.PlacementTarget as Grid;
                    //if (g != null)
                    //{
                    //Console.WriteLine(g.Background); // Will print red
                    //}
                }
            }


        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }
    }
}
