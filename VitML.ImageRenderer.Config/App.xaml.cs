using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.Config.ViewModels;

namespace VitML.ImageRenderer.Config
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            MainWindow w = new MainWindow();
            w.DataContext = new MainVM();
            (w.DataContext as MainVM).Load();
            w.Show();
        }
    }
}
