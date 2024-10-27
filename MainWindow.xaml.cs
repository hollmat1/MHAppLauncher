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
using AppLauncher.ShellClasses;
using static AppLauncher.NativeMethods;
using System.Configuration;
using System.Windows.Controls;
using AppLauncher.FileHelpers;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AppLauncher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileSystemObjectInfo> FilesCollection { get; set; }
        private string _folderPath, _networkDrivesfilePath;
        private bool UseFileBrowserDialogToOpenFolderShortcuts = true;
        private NetUserDetail UserDetails = null;

        public MainWindow()
        {
            String assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            String assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.Title = $"App Launcher v{assemblyVersion}";

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
                    _folderPath = AppLauncherAppConfig.DesktopPath;

                    UseFileBrowserDialogToOpenFolderShortcuts = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseFileBrowserDialogToOpenFolderShortcuts"]) &&
                        ConfigurationManager.AppSettings["UseFileBrowserDialogToOpenFolderShortcuts"].Equals("true", StringComparison.OrdinalIgnoreCase);

                    try
                    {
                        EnsureExists();
                        EnumerateFiles();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred deleting file.  {ex.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }

                }
            }

            InitializeComponent();
            SetAppLauncherEnv();
            TryMapDrives();
        }

        private void TryMapDrives()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["AutoMapperFilePath"]))
            {
                return;
            }

            _networkDrivesfilePath = AppLauncherAppConfig.AutoMapperFilePath;

            try
            {
                var ParentPath = Directory.GetParent(_networkDrivesfilePath);

                if (!ParentPath.Exists)
                {
                    if (AppLauncherAppConfig.IsOneDriveFolder(ParentPath.FullName) && !AppLauncherAppConfig.OneDriveExists)
                    {
                        _networkDrivesfilePath = AppLauncherAppConfig.AutoMapperFileTempPath;

                        ParentPath = Directory.GetParent(_networkDrivesfilePath);
                    }

                    ParentPath.Create();
                }

                if(!File.Exists(_networkDrivesfilePath))
                {
                    return;
                }

                using(var rdr = new StreamReader(_networkDrivesfilePath))
                {
                    while(rdr.Peek() > -1)
                    {
                        var MappingLine = rdr.ReadLine();

                        if(MappingLine.StartsWith(@"'"))
                        {
                            continue;
                        }

                        try 
                        {
                            var expandedDriveInfo = Environment.ExpandEnvironmentVariables(MappingLine);
                            // todo : map network drive

                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch 
            {
                // intentionally not caught
            }


        }

        private void EnsureExists()
        {
            if (!Directory.Exists(_folderPath))
            {
                if (AppLauncherAppConfig.IsOneDriveFolder(_folderPath) && !AppLauncherAppConfig.OneDriveExists)
                {
                    MessageBox.Show($"OneDrive is not available.  Please ensure you are Signed-in to OneDrive.  Desktop changes will not be saved!",
                        "Sign-in to OneDrive", MessageBoxButton.OK, MessageBoxImage.Warning);

                    _folderPath = AppLauncherAppConfig.DesktopTempPath;
                }

                Directory.CreateDirectory(_folderPath);
                var optionalShortcuts = (NameValueCollection)ConfigurationManager.GetSection("OptionalShortcuts");

                if (optionalShortcuts != null)
                {
                    foreach (var shortcutKey in optionalShortcuts.AllKeys)
                    {
                        ShortCutHelper.CreateShortcut(shortcutKey, optionalShortcuts[shortcutKey], _folderPath, $"{shortcutKey}.lnk");
                    }
                }
            }

            var defaultShortcuts = (NameValueCollection)ConfigurationManager.GetSection("DefaultShortcuts");

            if (defaultShortcuts != null)
            {
                foreach (var shortcutKey in defaultShortcuts.AllKeys)
                {
                    ShortCutHelper.CreateShortcut(shortcutKey, defaultShortcuts[shortcutKey], _folderPath, $"{shortcutKey}.lnk");
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
            if (!Directory.Exists(_folderPath))
                return;

            //FilesCollection = new ObservableCollection<DownloadedFile>();
            FilesCollection = new ObservableCollection<FileSystemObjectInfo>();

            var folder = new DirectoryInfo(_folderPath);
            var shortcuts = folder.GetFiles("*");

            foreach (var file in shortcuts)
            {
                var fileInfo = new FileSystemObjectInfo(file);
                FilesCollection.Add(fileInfo);
            }
        }

        public void OpenApp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var f = ListBoxFiles.SelectedItem as FileSystemObjectInfo;

                if (f == null)
                    return;
                
                try
                {
                    if(UseFileBrowserDialogToOpenFolderShortcuts)
                    {
                        var Target = ShortCutHelper.ResolveShortcutTarget(f.FilePath);

                        if (ShortCutHelper.IsTargetNetworkPath(Target))
                        {
                            // Open File Dialog
                            var FileBrowser = new System.Windows.Forms.OpenFileDialog
                            {
                                InitialDirectory = Target
                            };

                            FileBrowser.ShowDialog();

                            if (!string.IsNullOrEmpty(FileBrowser.FileName))
                            {
                                try
                                {
                                    Process.Start(FileBrowser.FileName);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"An error occurred opening file {FileBrowser.FileName}.  {ex.Message}", "Error opening file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                                }
                            }

                            return;
                        }
                    }

                    Process.Start(f.FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred opening application {f.Name}.  {ex.Message}", "Error opening application", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void File_Drop(object sender, DragEventArgs e)
        {
            if (!Directory.Exists(_folderPath))
                return; 

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
                        dialog.Owner = this;
                        if (dialog.ShowDialog() == true)
                        {
                            var Name = dialog.NameText;
                            var Parent = Directory.GetParent(fi.FullName).FullName;

                            var newSCPath = Path.Combine(_folderPath, $"{Name}.lnk");

                            if ((File.Exists(newSCPath)))
                            {
                                MessageBoxResult result = MessageBox.Show($"Shortcut already exists?  Overwrite", "Overwrite existing shortcut?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

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
                        MessageBoxResult result = MessageBox.Show($"Shortcut already exists?  Overwrite", "Overwrite existing shortcut?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

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
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm.DataContext is FileSystemObjectInfo)
                {
                    var theFile = (FileSystemObjectInfo)cm.DataContext;
                    DeleteFile(theFile);
                }
            }
        }

        private void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm.DataContext is FileSystemObjectInfo)
                {
                    var theFile = (FileSystemObjectInfo)cm.DataContext;
                    RenameFile(theFile);
                }
            }
        }

        private void DeleteFile(FileSystemObjectInfo theFile)
        {
            string caption = "Deletion";

            if (theFile == null)
                return;

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {theFile.Name}?", caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                File.Delete(theFile.FilePath);
                FilesCollection.Remove(theFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred deleting file.  {ex.Message}", caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

        }

        private void RenameFile(FileSystemObjectInfo theFile)
        {
            if (theFile == null)
                return;

            string caption = "Rename";
            var dialog = new PromptNameDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var newSCPath = Path.Combine(_folderPath, $"{dialog.NameText}.lnk");
                    MessageBoxResult result;

                    if (File.Exists(newSCPath))
                    {
                        result = MessageBox.Show($"Shortcut with name {newSCPath} already exists.  Overwrite?", caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                        if (result == MessageBoxResult.Yes)
                        {
                            File.Delete(newSCPath);
                        }
                    }
                    else
                    {
                        result = MessageBox.Show($"Are you sure you want to rename {theFile.Name}?", caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    }

                    if (result == MessageBoxResult.No)
                        return;

                    File.Move(theFile.FilePath, newSCPath);
                    FilesCollection.Remove(theFile);
                    FilesCollection.Add(new FileSystemObjectInfo(new FileInfo(newSCPath)));

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred renaming file.  {ex.Message}", caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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

        private void Image_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ListBox lb = sender as ListBox;

                if (lb == null)
                {
                    e.Handled = true;
                    return;
                }

                DeleteFile(lb.SelectedItem as FileSystemObjectInfo);

                e.Handled = true;

            }

            if (e.Key == Key.F2)
            {
                ListBox lb = sender as ListBox;

                if (lb == null)
                {
                    e.Handled = true;
                    return;
                }

                RenameFile(lb.SelectedItem as FileSystemObjectInfo);

                e.Handled = true;

            }

        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(_folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred opening explorer.  {ex.Message}", "Error opening application", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                String assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                var output = GetKerberosTickets();

                if (output.Length > 200)
                    output = output.Substring(0, 200);

                if(UserDetails != null)
                {
                    MessageBox.Show($"Name:{assemblyName}, Version:{assemblyVersion}" +
                         Environment.NewLine + Environment.NewLine +
                         $"UserName: {UserDetails.Username + Environment.NewLine }" +
                         $"UserDomain: {UserDetails.DomainDns + Environment.NewLine}" +
                         $"Current Login Session: {Environment.NewLine + output}"
                         , "Logon Session Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Name:{assemblyName}, Version:{assemblyVersion}" +
                         Environment.NewLine + Environment.NewLine +
                         $"Current Login Session: {Environment.NewLine + output}"
                         , "Logon Session Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting session data.  {ex.Message}"
                    , "Logon Session Data", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }


        private string GetKerberosTickets()
        {
            ProcessStartInfo s = new ProcessStartInfo(@"klist.exe");
            s.Arguments = "tickets";
            s.RedirectStandardOutput = true;
            s.RedirectStandardError = true;
            s.UseShellExecute = false;
            s.CreateNoWindow = true;
            s.LoadUserProfile = false;

            Process process = new Process();
            process.StartInfo = s;

            // Start the process
            process.Start();

            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                EnumerateFiles();
                e.Handled = true;
            }
        }

        private void FileBrowser_Click(object sender, RoutedEventArgs e)
        {
            // Open File Dialog
            var FileBrowser = new System.Windows.Forms.OpenFileDialog {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            FileBrowser.ShowDialog();

            if(!string.IsNullOrEmpty(FileBrowser.FileName))
            {
                try
                {
                    Process.Start(FileBrowser.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred opening file {FileBrowser.FileName}.  {ex.Message}", "Error opening file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }

        }

        private NetUserDetail GetUserNetDetails()
        {
            try
            {
                const string UPNPatternMatch = "\\s+(?:(?<username>[^@]+)@(?<domaindns>.+))";
                string KListOutput = GetKerberosTickets();

                var Match = Regex.Match(KListOutput, UPNPatternMatch);

                if (Match.Success)
                {
                    return new NetUserDetail
                    {
                        Username = Match.Groups["username"].Value.Trim(),
                        DomainDns = Match.Groups["domaindns"].Value.Trim()
                    };
                }
            }
            catch
            {

            }

            return null;
        }

        private void SetAppLauncherEnv()
        {
            UserDetails = GetUserNetDetails();

            if (UserDetails != null)
            {
                Environment.SetEnvironmentVariable("CS_USERNAME", UserDetails.Username, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("CS_USERDOMAIN", UserDetails.DomainDns, EnvironmentVariableTarget.Process);
            }
        }

        private class NetUserDetail
        {
            public string Username { get; set; }
            public string DomainDns { get; set; }
        }
    }
}
