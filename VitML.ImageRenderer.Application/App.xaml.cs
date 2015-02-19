using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.App.Views;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Loaders;
using VitML.ImageRenderer.Storages;

namespace VitML.ImageRenderer.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Main(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;


            /*Configuration c = new Configuration();
            c.Connections.Add(new FileConnectionConfiguration());
            c.Connections.Add(new FtpConnectionConfiguration());
            c.Storages.Add(new StorageConfiguration());
            string res = SerializeObject(c);*/

            Window window = new MainWindow();
            window.Show();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

    }

}
