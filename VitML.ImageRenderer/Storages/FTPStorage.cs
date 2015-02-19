using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;

namespace VitML.ImageRenderer.Storages
{
    public class FTPStorage : ImageStorage
    {

        private string _Host;
        private string _ServerPath;
        private NetworkCredential _Credentials;

        private FTPFileSystemWatcher watcher;

        public string Directory { get { return CreateDirectoryURI(); } }

        public FTPStorage(
            string host,
            string directory,
            string username,
            string password)
        {
            _Host = host;
            _ServerPath = directory;
            _Credentials = new NetworkCredential(username, password);
            watcher = new FTPFileSystemWatcher(this);
            watcher.FTPFileCreated += watcher_FTPFileCreated;
        }

        public override void Connect()
        {
            watcher.Start();
        }

        public override void Disconnect()
        {
            watcher.Stop();
        }

        void watcher_FTPFileCreated(object sender, FileSystemEventArgs e)
        {
            NotifyAdded(e.Name);
        }

        public override IEnumerable<string> GetImages()
        {
            List<string> files = new List<string>();
            try
            {
                FtpWebRequest listFilesRequest = (FtpWebRequest)FtpWebRequest.Create(Directory);
                listFilesRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                listFilesRequest.Credentials = _Credentials;
                listFilesRequest.UsePassive = true;
                listFilesRequest.UseBinary = false;
                listFilesRequest.KeepAlive = false;
                
                string[] list = null;

                using (FtpWebResponse response = (FtpWebResponse)listFilesRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }

                foreach (string line in list)
                {
                    string name = Path.GetFileName(line);
                    files.Add(name);
                }

            }
            catch (WebException)
            {
                //
            }
            return files;
        }

        private bool IsDirectory(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException();
            }
            return Path.GetExtension(directory) == string.Empty;
        }

        public override bool IsAvailable(string id)
        {
            return true;// throw new NotImplementedException();
        }

        private string CreateURI(string id)
        {
            string fullHost = "ftp://" + _Host;
            string filePath = _ServerPath +"/"+ id;
            string ftpfullpath = fullHost + filePath;
            return ftpfullpath;
        }

        private string CreateDirectoryURI()
        {
            string ftpfullpath = "ftp://" + _Host + _ServerPath;
            return ftpfullpath;
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                BitmapImage image = null;
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    image.StreamSource = ms;
                    image.EndInit();
                }));
                return image;
            }
        }

        public override BitmapImage Load(string id)
        {
            BitmapImage image = null;
            try
            {
                string ftpfullpath = CreateURI(id);

                using (WebClient request = new WebClient())
                {
                    request.Credentials = _Credentials;
                    byte[] fileData = request.DownloadData(ftpfullpath);
                    image = ToImage(fileData);
                }
            }
            catch (WebException)
            {
                //
            }
            return image;
        }

        public override bool Save(string id, System.Windows.Media.Imaging.BitmapImage image)
        {
            bool saved = false;
             try
             {
                 throw new NotImplementedException();
             }
             catch (WebException)
             {
                 //
             }
             return saved;
        }

        public override bool Remove(string id)
        {
            bool removed = false;
            try
            {
                string serverUri = CreateURI(id);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = _Credentials;
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                removed = response.StatusCode == FtpStatusCode.FileActionOK;
                response.Close();
            }
            catch (WebException)
            {
                //
            }
            return removed;
        }

        public override bool Save(string id, byte[] buffer)
        {
            try
            {
                WebRequest request = WebRequest.Create(Directory + "/" + id);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = _Credentials;
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();
                return true;
            }
            catch (Exception e)
            {
                //
            }
            return false;
        }
    }
}
