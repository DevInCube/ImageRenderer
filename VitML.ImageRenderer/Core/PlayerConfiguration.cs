﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VitML.ImageRenderer.Core
{

    public interface IObjectProvider
    {
        T GetObject<T>(string key);
    }

    [Serializable()]
    [XmlInclude(typeof(FileConnectionConfiguration))]
    [XmlInclude(typeof(FtpConnectionConfiguration))]
    [XmlInclude(typeof(HttpConnectionConfiguration))]
    public abstract class ConnectionConfiguration
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        public ConnectionConfiguration()
        {
            Name = "";
        }
    }

    [XmlType("file")]
    public class FileConnectionConfiguration : ConnectionConfiguration
    {
        [XmlElement("disk")]
        public string Disk { get; set; }

        public FileConnectionConfiguration()
        {
            Disk = "C:";
        }
    }

    [XmlType("ftp")]
    public class FtpConnectionConfiguration : ConnectionConfiguration
    {
        [XmlElement("host")]
        public string Host { get; set; }
        [XmlElement("username")]
        public string UserName { get; set; }
        [XmlElement("password")]
        public string Password { get; set; }

        public FtpConnectionConfiguration()
        {
            Host = "";
            UserName = "";
            Password = "";
        }

    }

    [XmlType("http")]
    public class HttpConnectionConfiguration : ConnectionConfiguration
    {
        [XmlElement("image")]
        public string ImageUri { get; set; }
        [XmlElement("time")]
        public string TimeUri { get; set; }

        public HttpConnectionConfiguration()
        {
            ImageUri = "";
            TimeUri = "";
        }

    }

    public class StorageConfiguration
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("connection")]
        public string ConnectionName { get; set; }
        [XmlElement("directory")]
        public string Directory { get; set; }
        [XmlIgnore]
        public ConnectionConfiguration Connection { get; set; }

        public StorageConfiguration()
        {
            Name = "";
            ConnectionName = "";
            Directory = "";
        }

        public void Initialize(IObjectProvider conf)
        {
            this.Connection = conf.GetObject<ConnectionConfiguration>(ConnectionName);
        }
    }

    public class PlayerConfiguration
    {

        public class AStorageConfiguration
        {
            [XmlAttribute("storage")]
            public string StorageName { get; set; }
            [XmlIgnore]
            public StorageConfiguration Storage { get; set; }

            public AStorageConfiguration()
            {
                StorageName = "";
            }

            public void Initialize(IObjectProvider conf)
            {
                this.Storage = conf.GetObject<StorageConfiguration>(StorageName);
            }
        }

        public class RenderConfiguration
        {
            [XmlAttribute("fps")]
            public uint FPS { get; set; }
            [XmlAttribute("cleanup")]
            public bool CleanUp { get; set; }
            [XmlAttribute("deleteimages")]
            public bool DeleteImages { get; set; }

            public RenderConfiguration()
            {
                FPS = 0;
                CleanUp = false;
                DeleteImages = false;
            }
        }

        public class SignalConfiguration : AStorageConfiguration
        {
            [XmlAttribute("filename")]
            public string FileName { get; set; }


            public SignalConfiguration()
            {
                FileName = "0.listen";
                StorageName = "signal";
            }
        }

        public class SourceConfiguration : AStorageConfiguration
        {

            [XmlAttribute("pull")]
            public bool DoPull { get; set; }

            public SourceConfiguration()
            {
                StorageName = "input";
            }
        }

        public class CompressConfiguration : AStorageConfiguration
        {
            [XmlAttribute("enabled")]
            public bool IsEnabled { get; set; }
            [XmlAttribute("quality")]
            public uint Quality { get; set; }

            public CompressConfiguration()
            {
                IsEnabled = false;
                Quality = 0;
                StorageName = "";
            }
        }

        [XmlElement("render")]
        public RenderConfiguration Render { get; set; }
        [XmlElement("signal")]
        public SignalConfiguration Signal { get; set; }
        [XmlElement("source")]
        public SourceConfiguration Source { get; set; }
        [XmlElement("compress")]
        public CompressConfiguration Compress { get; set; }

        public PlayerConfiguration()
        {
            Render = new RenderConfiguration();
            Signal = new SignalConfiguration();
            Source = new SourceConfiguration();
            Compress = new CompressConfiguration();
        }

        public void Initialize(IObjectProvider configuration)
        {
            Signal.Initialize(configuration);
            Source.Initialize(configuration);
            Compress.Initialize(configuration);
        }
    }
}
