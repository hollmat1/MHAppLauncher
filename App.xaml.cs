using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            String assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            bool createdNew;

            _mutex = new Mutex(true, assemblyName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                Console.WriteLine("App already running!");
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }

    }
}
