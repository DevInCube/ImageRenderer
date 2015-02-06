using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VitML.ImageRenderer.Controllers;

namespace VitML.ImageRenderer.Views
{
    /// <summary>
    /// Interaction logic for Renderer.xaml
    /// </summary>
    public partial class Renderer : UserControl
    {

        private Stopwatch stopWatch;

        public RendererController RendererController { get; private set; }

        public Renderer()
        {
            InitializeComponent();

            RendererController = new RendererController(this);
            stopWatch = new Stopwatch();
            fpsLbl.Content = "";
            //fpsView.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void ShowFPS(bool fps)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (fps)
                    fpsView.Visibility = System.Windows.Visibility.Visible;
                else
                    fpsView.Visibility = System.Windows.Visibility.Collapsed;
            }));
        }

        public void Render(BitmapImage image)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    long millis = stopWatch.ElapsedMilliseconds;
                    if (millis == 0) millis = 1;
                    long fps = (long)Math.Round(1000 / (double)millis);
                    this.fpsLbl.Content = fps;
                    this.image1.Source = image;
                    stopWatch.Restart();
                }));
            }
            catch (Exception)
            {
                //
            }
        }

    }
}
