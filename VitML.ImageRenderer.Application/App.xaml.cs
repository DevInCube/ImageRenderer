using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.App.Views;
using VitML.ImageRenderer.Core;
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
            WindowConfig config = new WindowConfig();
            config.Directory = @"C:\Users\DevInCube\Desktop\res";
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
