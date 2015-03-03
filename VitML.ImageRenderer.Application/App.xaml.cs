using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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

            string exD = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string confPath = Path.Combine(exD, "config.xml");
            string content = File.ReadAllText(confPath);
            Configuration conf = Configuration.Parse(content);
            conf.Initialize();
            conf.Start();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

    }

}
