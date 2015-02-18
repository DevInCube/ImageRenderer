using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
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
            Window window;
            //window = new MainWindow();
            window = new TestWindow();
            var player = new ImagePlayer();
            window.DataContext = player;
            string exeDir = Directory.GetCurrentDirectory();
            string configPath = exeDir + "\\config.xml";
            var configLoader = new XMLConfigLoader(configPath);
            WindowConfig config = configLoader.Load<WindowConfig>();
            player.Setup(config);
            window.Show();
            player.Start();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }
    }
}
