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
