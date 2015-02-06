using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
using VitML.ImageRenderer.Controllers;
using VitML.ImageRenderer.Loaders;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string configPath = @"C:\VIT.Projects\Projects\VitML.ImageRenderer\VitML.ImageRenderer.Test\bin\Debug\config.txt";
        private string pullDir = @"C:\VIT.Projects\Projects\VitML.ImageRenderer\srcImages";
        private string pushDir = @"C:\VIT.Projects\Projects\VitML.ImageRenderer\VitML.ImageRenderer.Application\bin\Debug\res";
        private ImagePusher pusher;
        private Timer watcher = new Timer();

        public MainWindow()
        {
            InitializeComponent();
            
            this.Closed += MainWindow_Closed;

            pusher = new ImagePusher(pullDir, pushDir);
            startTimer.Click += startTimer_Click;
            stopTimer.Click += stopTimer_Click;


            watcher.Interval = 500;
            watcher.Elapsed += watcher_Elapsed;
            watcher.Start();

            deleteAllBtn.Click += deleteAllBtn_Click;
        }

        void deleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(pushDir);
            foreach (FileInfo fi in di.GetFiles())
                fi.Delete();
        }

        void watcher_Elapsed(object sender, ElapsedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(pushDir);
            this.Dispatcher.Invoke((Action)(() =>
            {
                filesTb.Text = di.GetFiles().Length.ToString();
            }));
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            //
        }

        void stopTimer_Click(object sender, RoutedEventArgs e)
        {
            pusher.StopPush();
        }

        void startTimer_Click(object sender, RoutedEventArgs e)
        {
            pusher.StartPush();
        }

        private void TextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                pusher.Interval = 1000 / Int32.Parse((sender as TextBox).Text);
        }

    }
}
