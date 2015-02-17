using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class Renderer : UserControl, INotifyPropertyChanged
    {

        private Stopwatch stopWatch;

        public RendererController RendererController { get; private set; }

        private Thread thread;
        private bool isRunning = true;
        private string _FPS;
        private Visibility _FPSVisible;
        private List<long> fpsList = new List<long>();

        public string FPS
        {
            get { return _FPS; }
            private set
            {
                _FPS = value;
                OnPropertyChanged("FPS");
            }
        }
        public Visibility FPSVisible
        {
            get { return _FPSVisible; }
            private set
            {
                _FPSVisible = value;
                OnPropertyChanged("FPSVisible");
            }
        }

        public Renderer()
        {
            InitializeComponent();
            this.Loaded += Renderer_Loaded;
            this.Unloaded += Renderer_Unloaded;

            RendererController = new RendererController(this);
            stopWatch = new Stopwatch();
            this.DataContext = this;

            thread = new Thread(() => { CalcFPS(); });
        }

        void Renderer_Loaded(object sender, RoutedEventArgs e)
        {
            thread.Start();
        }

        void Renderer_Unloaded(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            isRunning = false;
            thread.Interrupt();
        }

        private void CalcFPS()
        {
            try
            {
                while (isRunning)
                {
                    lock (fpsList)
                    {
                        int val = fpsList.Count > 0 ? (int)Math.Ceiling(fpsList.Average()) : 0;
                        Application.Current.Dispatcher.Invoke((Action)(() => {
                            FPS = val.ToString();
                            fpsList.Clear();
                        }));
                    }
                    Thread.Sleep(500);
                }
            }
            catch
            {
                //
                return;
            }
        }

        public void ShowFPS(bool fps)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                FPSVisible = (fps) ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public void Render(BitmapImage image)
        {
            try
            {
                long millis = stopWatch.ElapsedMilliseconds;
                if (millis == 0) millis = 1;
                long fps = (long)Math.Round(1000 / (double)millis);
                lock (fpsList)
                {
                    fpsList.Add(fps);
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.image1.Source = image;
                }));
                stopWatch.Reset();
            }
            catch (Exception)
            {
                //
                return;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
