using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.ViewModels;

namespace VitML.ImageRenderer.Config.ViewModels
{
    class MainVM : ObservableObject
    {

        private string confPath;
        private Configuration _Configuration;
        private WindowConfiguration _SelectedWindow;        

        public Configuration Configuration
        {
            get { return _Configuration; }
            set
            {
                _Configuration = value; OnPropertyChanged("Configuration");
            }
        }

        public WindowConfiguration SelectedWindow
        {
            get { return _SelectedWindow; }
            set
            {
                _SelectedWindow = value;
                OnPropertyChanged("SelectedWindow");
            }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand RunCommand { get; set; }
        public ICommand AddConnection { get; set; }
        public ICommand RemoveConnection { get; set; }
        public ICommand AddWindow { get; set; }
        public ICommand RemoveWindow { get; set; }


        public MainVM()
        {
            string exD = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            confPath = Path.Combine(exD, "config.xml");

            SaveCommand = new RelayCommand<object>(Save);
            LoadCommand = new RelayCommand<object>(Load);
            RunCommand = new RelayCommand<object>(Run);

            AddConnection = new RelayCommand<object>((o) => { 
                Configuration.Connections.Add(new HttpConnection3RingConfiguration());
                OnPropertyChanged("");
                OnPropertyChanged("Configuration");
                OnPropertyChanged("Configuration.Connections");
            });
            RemoveConnection = new RelayCommand<object>((o) =>
            {
                var con = SelectedWindow.Player.Source.Storage.Connection;
                Configuration.Connections.Remove(con);
                OnPropertyChanged("");
                OnPropertyChanged("Configuration");
                OnPropertyChanged("Configuration.Connections");
            });
            AddWindow = new RelayCommand<object>((o) => { 
                Configuration.Instances.Add(new WindowConfiguration());
                OnPropertyChanged("");
                OnPropertyChanged("Configuration");
                OnPropertyChanged("Configuration.Instances");
            });
            RemoveWindow = new RelayCommand<object>((o) => { 
                Configuration.Instances.Remove(SelectedWindow);
                OnPropertyChanged("");
                OnPropertyChanged("Configuration");
                OnPropertyChanged("Configuration.Instances");
            });
        }

        void Run(object x = null)
        {
            Configuration.Start();
        }

        internal void Load(object x = null)
        {
            string content = File.ReadAllText(confPath);
            var configuration = Configuration.Parse(content);
            configuration.Initialize();
            Configuration = configuration;
            SelectedWindow = Configuration.Instances.FirstOrDefault();
        }

        internal void Save(object x = null)
        {
            try
            {
                Configuration.Prepare();
                string ser = Configuration.Serialize();
                File.WriteAllText(confPath, ser);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        
    }
}
