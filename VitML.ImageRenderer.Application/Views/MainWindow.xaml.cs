using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VitML.ImageRenderer.App.ViewModels;
using VitML.ImageRenderer.Core;

namespace VitML.ImageRenderer.App.Views
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            close2.MouseEnter += close2_MouseEnter;
            close2.MouseLeave += close2_MouseLeave;
            close2.MouseUp += close2_MouseUp;

            this.MouseDown += MainWindow_MouseDown;
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if(!close2.IsMouseOver)
                    this.DragMove();
            }
            else
            {
                //throw new Exception();//@test
            }
        }

        void close2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        void close2_MouseLeave(object sender, MouseEventArgs e)
        {
            close2.Opacity = 0.0;
        }

        void close2_MouseEnter(object sender, MouseEventArgs e)
        {
            close2.Opacity = 0.6;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.DataContext != null)
                (this.DataContext as MainVM).Closed();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
                (this.DataContext as MainVM).Loaded();
        }
    }
}
