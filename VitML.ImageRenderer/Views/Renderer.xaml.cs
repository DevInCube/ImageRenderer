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
        private List<long> timeList = new List<long>();

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
            stopWatch.Start();
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
                    lock (timeList)
                    {
                        int val = 0;
                        if (timeList.Count > 0)
                        {
                            double avgMillis = timeList.Average() / (double)TimeSpan.TicksPerMillisecond;
                            val = (avgMillis > 0) ? (int)Math.Ceiling(100 / avgMillis) : 0;
                            timeList.Clear();
                        }
                        Application.Current.Dispatcher.Invoke((Action)(() => {
                            FPS = val.ToString();
                        }));
                    }
                    Thread.Sleep(500);
                }
            }
            catch (ThreadInterruptedException)
            {
                //
            }
            catch (Exception)
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
                long ticks = stopWatch.ElapsedTicks;
                stopWatch.Reset();
                stopWatch.Start();
                if (ticks == 0) ticks = 1;
                lock (timeList)
                {
                    timeList.Add(ticks);
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.image1.Source = image;
                }));
            }
            catch (ThreadInterruptedException)
            {
                //
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
