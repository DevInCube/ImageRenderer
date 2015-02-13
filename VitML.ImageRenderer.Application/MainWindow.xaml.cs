using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.Controllers;
using VitML.ImageRenderer.Loaders;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IImageLoader imageLoader;
        private IConfigLoader configLoader;
        private RendererController rendererController;
        private WindowConfig config;
        private Timer gcTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            string exeDir = Directory.GetCurrentDirectory();
            string configPath = exeDir + "\\config.xml";
            configLoader = new XMLConfigLoader(configPath);
            configLoader.SourceChanged += configLoader_SourceChanged;
            config = configLoader.Load<WindowConfig>();
            if (!string.IsNullOrEmpty(config.Title))
                this.Title = config.Title;

            rendererController = renderer.RendererController;
            renderer.ShowFPS(config.ShowFPS);

            if (config.Timed)
                imageLoader = new TimedImageLoader();
            else
                imageLoader = new SimpleImageLoader();
            imageLoader.Init(config);
            imageLoader.ImageLoaded += imageLoader_ImageLoaded;

            gcTimer = new Timer();
            gcTimer.Interval = 1000;
            gcTimer.Elapsed += gcTimer_Elapsed;
            gcTimer.Start();
        }

        void gcTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GC.Collect();
        }

        void configLoader_SourceChanged()
        {
            var newConfig = configLoader.Load<WindowConfig>();
            config.Copy(newConfig);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            configLoader.Save(config);
            imageLoader.Close();
        }

        void imageLoader_ImageLoaded(BitmapImage obj)
        {
            rendererController.Render(obj);
        }

    }
}
