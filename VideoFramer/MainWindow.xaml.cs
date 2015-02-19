﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace VideoFramer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        class PushImage
        {
            public byte[] Image { get; set; }
            public string Name { get; set; }
        }

        private static string ConsumerKey = "2d177rkc7n7v2kx";
        private static string ConsumerSecret = "g5rc3w73pmymf3f";

        private int frameTime = 30;
        private System.Threading.Thread dbThread, frameThread;
        private bool running = true;
        private bool saveImages = false;
        private bool _OneFileMode = false;
        private List<PushImage> images = new List<PushImage>();

        public Visibility StartVisible
        {
            get { return !saveImages?Visibility.Visible:Visibility.Collapsed;}
        }
        public Visibility StopVisible
        {
            get { return saveImages ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Video
        {
            get { return @"C:\Users\Public\Videos\Sample Videos\WildLife.wmv"; }
        }

        public string OutputDirectory
        {
            get { return @"C:\Users\user\Desktop\res"; }
        }

        public bool SaveImages
        {
            get { return saveImages; }
            private set
            {
                saveImages = value;
                OnPropertyChanged("SaveImages");
                OnPropertyChanged("StartVisible");
                OnPropertyChanged("StopVisible");
            }
        }

        public bool OneFileMode
        {
            get { return _OneFileMode; }
            set
            {
                _OneFileMode = value;
                OnPropertyChanged("OneFileMode");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            startBtn.Click += startBtn_Click;
            stopBtn.Click += stopBtn_Click;
            VideoControl.LoadedBehavior = MediaState.Manual;
            VideoControl.UnloadedBehavior = MediaState.Manual;
            VideoControl.MediaEnded += VideoControl_MediaEnded;

            frameThread = new System.Threading.Thread(() => { FrameRun(); });
            dbThread = new System.Threading.Thread(() => { SaveImageRun(); });
            frameThread.Start();
            dbThread.Start();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            SaveImages = false;
            running = false;

            dbThread.Interrupt();
            frameThread.Interrupt();
        }

        void FrameRun()
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                while (running)
                {
                    int delay = frameTime;
                    if (saveImages)
                    {
                        sw.Start();
                        byte[] frame = null;
                        this.Dispatcher.Invoke((Action)(() =>
                            {
                                frame = VideoControl.GetFrame(0.5, 100);
                            }));
                        long ticks = (long)DateTime.Now.ToUniversalTime().Subtract(
                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                        ).Ticks;
                        string name = ticks + ".jpg";
                        lock (images)
                        {
                            images.Add(new PushImage() { Image = frame, Name = name });
                        }

                        int elapsed = (int)sw.ElapsedMilliseconds;
                        sw.Reset();
                        delay -= elapsed;
                        if (delay < 0) delay = 0;
                    }
                    System.Threading.Thread.Sleep(delay);
                }
            }
            catch (ThreadInterruptedException)
            {
                //
            }
            catch (Exception ee)
            {
                MessageBox.Show("Frame: " + ee.ToString());
                running = false;
            }
        }

        void SaveImageRun()
        {
            //var accessToken = GetAccessToken();
            //api = new DropboxApi(ConsumerKey, ConsumerSecret, accessToken);
            try
            {
                Stopwatch sw = new Stopwatch();
                while (running)
                {
                    sw.Start();
                    lock (images)
                    {
                        if (images.Count > 0)
                        {
                            PushImage item = images.First();
                            try
                            {
                                if (OneFileMode)
                                {
                                    string imageFile = "image.jpg";
                                    string temp = "image_tmp.jpg";
                                    SaveToDirectory(temp, item.Image);
                                    MoveToDirectory(temp, imageFile);
                                }
                                else
                                {
                                    SaveToDirectory(item.Name, item.Image);
                                }
                            }
                            catch (Exception)
                            {
                                //MessageBox.Show("SaveToDirectory: " + ee.ToString());
                            }
                            //var file = api.UploadFile2("dropbox\\imgTest", item.Name, item.Image);
                            images.Remove(item);
                        }
                    }
                    int elapsed = (int)sw.ElapsedMilliseconds;
                    if (elapsed < frameTime)
                        System.Threading.Thread.Sleep(frameTime - elapsed);
                    sw.Reset();
                }
            }
            catch (ThreadInterruptedException)
            {
                //
            }
            catch (Exception ee)
            {
                MessageBox.Show("DropBox: " + ee.ToString());
                running = false;
            }
        }

        /*private static OAuthToken GetAccessToken()
        {
            var oauth = new OAuth();
            var requestToken = oauth.GetRequestToken(new Uri(DropboxRestApi.BaseUri), ConsumerKey, ConsumerSecret);
            var authorizeUri = oauth.GetAuthorizeUri(new Uri(DropboxRestApi.AuthorizeBaseUri), requestToken);
            Process.Start(authorizeUri.AbsoluteUri);
            System.Threading.Thread.Sleep(5000); // Leave some time for the authorization step to complete
            return oauth.GetAccessToken(new Uri(DropboxRestApi.BaseUri), ConsumerKey, ConsumerSecret, requestToken);
        }*/

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VideoControl.Play();
        }

        void VideoControl_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                   {
                       VideoControl.Position = TimeSpan.Zero;
                       VideoControl.Play();
                   }));
            }
            catch (Exception ee)
            {
                MessageBox.Show("MediaEnded : " + ee.ToString());
            }
        }

        void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveImages = false;
            //timer.Stop();
        }

        void startBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveImages = true;
            int fps = int.Parse(fpsTb.Text);
            frameTime = (int)Math.Round(1000 / (double)fps);
        }

        void SaveToDirectory(string name, byte[] frame)
        {
            string dir = "";
            this.Dispatcher.Invoke((Action)(() =>
            {
                dir = dirTb.Text;
            }));
            string path = dir + "\\" + name;
            lock (locker)
            {
                File.WriteAllBytes(path, frame);
            }
        }

        void MoveToDirectory(string name1, string name2)
        {
            string dir = "";
            this.Dispatcher.Invoke((Action)(() =>
            {
                dir = dirTb.Text;
            }));
            string path1 = dir + "\\" + name1;
            string path2 = dir + "\\" + name2;
            lock (locker)
            {
                File.Replace(path1, path2, dir + "tmp");
            }
        }

        readonly object locker = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

    }

    public static class Ex
    {
        public static byte[] GetFrame(this UIElement source, double scale, int quality)
        {
            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;
            double renderHeight = actualHeight * scale;
            double renderWidth = actualWidth * scale;

            Size dpi = new Size(96, 96);
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth,
                (int)renderHeight, dpi.Width, dpi.Height, PixelFormats.Pbgra32);
            VisualBrush sourceBrush = new VisualBrush(source);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null,
                    new Rect(new Point(), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            JpegBitmapEncoder jpgEnconder = new JpegBitmapEncoder();
            jpgEnconder.QualityLevel = quality;
            jpgEnconder.Frames.Add(BitmapFrame.Create(renderTarget));

            Byte[] imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEnconder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }

            return imageArray;
        }

    }
}
