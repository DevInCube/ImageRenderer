using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VitML.ImageRenderer.Loaders;

namespace VitML.ImageRenderer.Models
{
    public class RendererConfig : IDynamicSerializable
    {

        public string Directory { get; set; }
        public uint UpdateFrequency { get; set; }
        public bool DeleteImages { get; set; }
        public bool Render { get; set; }
        public bool Compress { get; set; }
        public long CompressQuality { get; set; }
        public string CompressDirectory { get; set; }
        public bool Timed { get; set; }
        public bool ShowFPS { get; set; }

        public RendererConfig()
        {
            Directory = "C:\\";
            UpdateFrequency = 5;
            DeleteImages = false;
            Render = true;
            Compress = false;
            CompressQuality = 0L;
            CompressDirectory = "C:\\";
            Timed = true;
            ShowFPS = false;
        }

        public virtual void OnSerialize(SerializationInfo info)
        {
            info.Add("Directory", this.Directory);
            info.Add("FramesPerSecond", this.UpdateFrequency);
            info.Add("DeleteImages", this.DeleteImages);
            info.Add("Render", this.Render);
            info.Add("Compress", this.Compress);
            info.Add("CompressQuality", this.CompressQuality);
            info.Add("CompressDirectory", this.CompressDirectory);
            info.Add("Timed", this.Timed);
            info.Add("ShowFPS", this.ShowFPS);
        }

        public virtual void OnDeserialize(SerializationInfo info)
        {
            this.Directory = info.Get("Directory");
            this.UpdateFrequency = uint.Parse(info.Get("FramesPerSecond"));
            this.DeleteImages = bool.Parse(info.Get("DeleteImages"));
            this.Render = bool.Parse(info.Get("Render"));
            this.Compress = bool.Parse(info.Get("Compress"));
            this.CompressQuality = long.Parse(info.Get("CompressQuality"));
            this.CompressDirectory = info.Get("CompressDirectory");
            this.Timed = bool.Parse(info.Get("Timed"));
            this.ShowFPS = bool.Parse(info.Get("ShowFPS"));
        }
    }
}
