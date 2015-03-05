using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
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

        private Thread winThread;
        private Configuration conf;
        private string content;

        private void Main(object sender, StartupEventArgs e)
        {            
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            string exD = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string confPath = Path.Combine(exD, "config.xml");
            content = File.ReadAllText(confPath);

            Start();
        }

        private void Start()
        {
            winThread = new Thread(new ThreadStart(() =>
            {
                try
                {                    
                    Window[] wnds = Init();
                    foreach (Window wnd in wnds)
                        wnd.Closed += (s, e) =>
                        {
                            Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                            Environment.Exit(0);
                        };
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (Exception tEx)
                {                     
                    Start();                    
                }
            }));
            
            winThread.SetApartmentState(ApartmentState.STA);
            winThread.IsBackground = true;
            winThread.Start();                 
        }

        private Window[] Init()
        {            
            conf = Configuration.Parse(content);
            conf.Initialize();
            conf.Start();
            List<Window> wnds = new List<Window>();
            foreach (var ins in conf.Instances)
                if (ins.Window != null)
                    wnds.Add(ins.Window);
            return wnds.ToArray();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

    }

}
